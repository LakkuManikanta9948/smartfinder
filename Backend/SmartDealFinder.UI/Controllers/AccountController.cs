using System.Security.Claims;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using SmartDealFinder.Models;
using SmartDealFinder.Service;
using SmartDealFinder.UI.Models;

namespace SmartDealFinder.UI.Controllers;

public sealed class AccountController(
    PhoneOtpService otpService,
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager,
    IWebHostEnvironment environment,
    IConfiguration configuration) : Controller
{
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> Login(string? returnUrl = null)
    {
        var input = new RequestOtpInput
        {
            ReturnUrl = Url.IsLocalUrl(returnUrl) ? returnUrl : null
        };
        var externalInfo = await signInManager.GetExternalLoginInfoAsync();
        if (externalInfo?.LoginProvider == "Google")
        {
            input.DisplayName = externalInfo.Principal.FindFirstValue(ClaimTypes.Name) ?? externalInfo.Principal.Identity?.Name ?? string.Empty;
            ViewData["GoogleSignInPending"] = true;
        }

        SetGoogleLoginViewData();
        return View(input);
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    [EnableRateLimiting("otp")]
    public async Task<IActionResult> RequestCode(RequestOtpInput input, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            SetGoogleLoginViewData();
            return View("Login", input);
        }

        var result = await otpService.SendAsync(input.PhoneNumber, cancellationToken);
        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, result.Message);
            SetGoogleLoginViewData();
            return View("Login", input);
        }
        // Retrieve external login info to get the display name if available
        var externalInfo = await signInManager.GetExternalLoginInfoAsync();
        var displayName = externalInfo?.LoginProvider == "Google"
            ? externalInfo.Principal.FindFirstValue(ClaimTypes.Name) ?? externalInfo.Principal.Identity?.Name ?? input.DisplayName
            : input.DisplayName;
        return View("Verify", new VerifyOtpInput
        {
            PhoneNumber = input.PhoneNumber,
            DisplayName = displayName,
            ReturnUrl = Url.IsLocalUrl(input.ReturnUrl) ? input.ReturnUrl : null,
            Message = result.Message,
            DevelopmentCode = environment.IsDevelopment() ? result.DevelopmentCode : null
        });
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    [EnableRateLimiting("otp")]
    public async Task<IActionResult> PasswordLogin(PasswordLoginInput input)
    {
        if (!ModelState.IsValid)
        {
            TempData["Error"] = "Enter a valid email address and password.";
            return RedirectToAction(nameof(Login), new { returnUrl = Url.IsLocalUrl(input.ReturnUrl) ? input.ReturnUrl : null });
        }

        var user = await userManager.FindByEmailAsync(input.Email.Trim());
        if (user == null || !user.IsActive)
        {
            TempData["Error"] = "The email or password is incorrect, or the account is inactive.";
            return RedirectToAction(nameof(Login), new { returnUrl = Url.IsLocalUrl(input.ReturnUrl) ? input.ReturnUrl : null });
        }

        var result = await signInManager.PasswordSignInAsync(user.UserName!, input.Password, isPersistent: false, lockoutOnFailure: true);
        if (!result.Succeeded)
        {
            TempData["Error"] = result.IsLockedOut
                ? "This account is temporarily locked. Try again later."
                : "The email or password is incorrect, or the account is inactive.";
            return RedirectToAction(nameof(Login), new { returnUrl = Url.IsLocalUrl(input.ReturnUrl) ? input.ReturnUrl : null });
        }

        if (!string.IsNullOrWhiteSpace(input.ReturnUrl) && Url.IsLocalUrl(input.ReturnUrl))
            return LocalRedirect(input.ReturnUrl);
        if (await userManager.IsInRoleAsync(user, "ORGADMIN"))
            return RedirectToAction("Index", "AdminDashboard");
        return RedirectToAction("Index", "Home");
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    [EnableRateLimiting("otp")]
    public async Task<IActionResult> Verify(VerifyOtpInput input, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return View(input);

        var verification = await otpService.VerifyAsync(input.PhoneNumber, input.Code, cancellationToken);
        if (!verification.Succeeded)
        {
            ModelState.AddModelError(nameof(input.Code), verification.Message);
            return View(input);
        }

        var externalInfo = await signInManager.GetExternalLoginInfoAsync();
        var normalizedPhone = Regex.Replace(input.PhoneNumber.Trim(), "[\\s()\\-]", string.Empty, RegexOptions.CultureInvariant);
        var user = await userManager.FindByNameAsync(normalizedPhone);
        if (user == null)
        {
            user = new ApplicationUser
            {
                UserName = normalizedPhone,
                PhoneNumber = normalizedPhone,
                PhoneNumberConfirmed = true,
                DisplayName = string.IsNullOrWhiteSpace(input.DisplayName) ? "SmartCompare user" : input.DisplayName.Trim(),
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            var createResult = await userManager.CreateAsync(user);
            if (!createResult.Succeeded)
            {
                foreach (var error in createResult.Errors)
                    ModelState.AddModelError(string.Empty, error.Description);
                return View("Login", new RequestOtpInput { PhoneNumber = normalizedPhone, DisplayName = input.DisplayName });
            }

            var roleResult = await userManager.AddToRoleAsync(user, "USER");
            if (!roleResult.Succeeded)
                throw new InvalidOperationException($"Unable to assign the default USER role: {string.Join(", ", roleResult.Errors.Select(error => error.Code))}");
        }
        else if (!user.IsActive)
        {
            ModelState.AddModelError(string.Empty, "This account is inactive. Contact support for assistance.");
            return View("Login", new RequestOtpInput { PhoneNumber = normalizedPhone });
        }

        user.PhoneNumber = normalizedPhone;
        user.PhoneNumberConfirmed = true;
        user.UpdatedAt = DateTime.UtcNow;
        var updateResult = await userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
            throw new InvalidOperationException($"Unable to update the verified phone number: {string.Join(", ", updateResult.Errors.Select(error => error.Code))}");

        if (externalInfo?.LoginProvider == "Google")
        {
            var linkedUser = await userManager.FindByLoginAsync(externalInfo.LoginProvider, externalInfo.ProviderKey);
            if (linkedUser != null && linkedUser.Id != user.Id)
            {
                TempData["Error"] = "That Google account is already linked to another account. Sign in with its linked phone number.";
                return RedirectToAction(nameof(Login));
            }

            if (linkedUser == null)
            {
                var linkResult = await userManager.AddLoginAsync(user, externalInfo);
                if (!linkResult.Succeeded)
                {
                    TempData["Error"] = "Your phone was verified, but the Google account could not be linked. Sign in using phone verification and try again.";
                    return RedirectToAction(nameof(Login));
                }
            }
        }

        var claims = new List<Claim> { new("auth_method", externalInfo?.LoginProvider == "Google" ? "google_phone_otp" : "phone_otp") };
        await signInManager.SignInWithClaimsAsync(user, isPersistent: false, claims);

        if (!string.IsNullOrWhiteSpace(input.ReturnUrl) && Url.IsLocalUrl(input.ReturnUrl))
            return LocalRedirect(input.ReturnUrl);
        if (await userManager.IsInRoleAsync(user, "ORGADMIN"))
            return RedirectToAction("Index", "AdminDashboard");

        return RedirectToAction("Index", "Home");
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public IActionResult ExternalLogin(string provider, string? returnUrl = null)
    {
        if (!string.Equals(provider, "Google", StringComparison.Ordinal))
            return BadRequest("Unsupported external login provider.");
        if (string.IsNullOrWhiteSpace(configuration["Authentication:Google:ClientId"]) ||
            string.IsNullOrWhiteSpace(configuration["Authentication:Google:ClientSecret"]))
        {
            TempData["Error"] = "Google sign-in is not configured yet. Add the Google OAuth client ID and secret to User Secrets.";
            return RedirectToAction(nameof(Login));
        }

        var safeReturnUrl = Url.IsLocalUrl(returnUrl) ? returnUrl : null;
        var callbackUrl = Url.Action(nameof(ExternalLoginCallback), "Account", new { returnUrl = safeReturnUrl });
        var properties = signInManager.ConfigureExternalAuthenticationProperties(provider, callbackUrl!);
        return Challenge(properties, provider);
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> ExternalLoginCallback(string? returnUrl = null, string? remoteError = null)
    {
        if (!string.IsNullOrWhiteSpace(remoteError))
        {
            TempData["Error"] = "Google sign-in was cancelled or could not be completed.";
            return RedirectToAction(nameof(Login));
        }

        var externalInfo = await signInManager.GetExternalLoginInfoAsync();
        if (externalInfo?.LoginProvider != "Google")
        {
            TempData["Error"] = "Google sign-in could not be verified. Please try again.";
            return RedirectToAction(nameof(Login));
        }

        var verifiedEmail = externalInfo.Principal.FindFirstValue(ClaimTypes.Email);
        var emailVerified = externalInfo.Principal.FindFirstValue("email_verified");
        if (string.IsNullOrWhiteSpace(verifiedEmail) || !string.Equals(emailVerified, "true", StringComparison.OrdinalIgnoreCase))
        {
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);
            TempData["Error"] = "Google did not provide a verified email address. Verify your Google email and try again.";
            return RedirectToAction(nameof(Login));
        }

        var linkedUser = await userManager.FindByLoginAsync(externalInfo.LoginProvider, externalInfo.ProviderKey)
            ?? await userManager.FindByEmailAsync(verifiedEmail);
        if (linkedUser != null && !linkedUser.IsActive)
        {
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);
            TempData["Error"] = "This account is inactive. Contact support for assistance.";
            return RedirectToAction(nameof(Login));
        }

        if (linkedUser == null)
        {
            linkedUser = new ApplicationUser
            {
                UserName = verifiedEmail,
                Email = verifiedEmail,
                EmailConfirmed = true,
                DisplayName = externalInfo.Principal.FindFirstValue(ClaimTypes.Name) ?? verifiedEmail,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            var createResult = await userManager.CreateAsync(linkedUser);
            if (!createResult.Succeeded)
            {
                await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);
                TempData["Error"] = "We could not create your account. Please try signing in again or use phone verification.";
                return RedirectToAction(nameof(Login));
            }

            var roleResult = await userManager.AddToRoleAsync(linkedUser, "USER");
            if (!roleResult.Succeeded)
            {
                await userManager.DeleteAsync(linkedUser);
                throw new InvalidOperationException($"Unable to assign the default USER role to the Google account: {string.Join(", ", roleResult.Errors.Select(error => error.Code))}");
            }
        }

        var existingLogin = await userManager.FindByLoginAsync(externalInfo.LoginProvider, externalInfo.ProviderKey);
        if (existingLogin == null)
        {
            var linkResult = await userManager.AddLoginAsync(linkedUser, externalInfo);
            if (!linkResult.Succeeded)
            {
                await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);
                TempData["Error"] = "The Google account could not be linked. Please try again or use phone verification.";
                return RedirectToAction(nameof(Login));
            }
        }

        var signInResult = await signInManager.ExternalLoginSignInAsync(
            externalInfo.LoginProvider,
            externalInfo.ProviderKey,
            isPersistent: false,
            bypassTwoFactor: true);
        if (signInResult.Succeeded)
        {
            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
                return LocalRedirect(returnUrl);
            if (await userManager.IsInRoleAsync(linkedUser, "ORGADMIN"))
                return RedirectToAction("Index", "AdminDashboard");
            return RedirectToAction("Index", "Home");
        }

        await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);
        TempData["Error"] = signInResult.IsLockedOut
            ? "This account is temporarily locked. Try again later."
            : "Google sign-in could not be completed. Please try again.";
        return RedirectToAction(nameof(Login));
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await signInManager.SignOutAsync();
        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult AccessDenied() => View();

    private void SetGoogleLoginViewData()
    {
        ViewData["GoogleLoginEnabled"] =
            !string.IsNullOrWhiteSpace(configuration["Authentication:Google:ClientId"]) &&
            !string.IsNullOrWhiteSpace(configuration["Authentication:Google:ClientSecret"]);
    }
}
