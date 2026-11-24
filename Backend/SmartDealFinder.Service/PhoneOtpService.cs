using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using SmartDealFinder.Models;
using SmartDealFinder.Models.Core;

namespace SmartDealFinder.Service;

public interface IOtpDelivery
{
    Task<string?> SendAsync(string phoneNumber, string code, CancellationToken cancellationToken);
}

public sealed record OtpSendResult(bool Succeeded, string Message, string? DevelopmentCode = null);
public sealed record OtpVerificationResult(bool Succeeded, string Message);

public sealed class PhoneOtpService
{
    private static readonly TimeSpan OtpLifetime = TimeSpan.FromMinutes(5);
    private static readonly TimeSpan SendCooldown = TimeSpan.FromSeconds(60);
    private static readonly TimeSpan SendWindow = TimeSpan.FromMinutes(15);
    private const int MaxSendsPerWindow = 3;
    private const int MaxFailedAttempts = 5;

    private readonly DatabaseContext _db;
    private readonly IOtpDelivery _delivery;

    public PhoneOtpService(DatabaseContext db, IOtpDelivery delivery)
    {
        _db = db;
        _delivery = delivery;
    }

    public async Task<OtpSendResult> SendAsync(string phoneNumber, CancellationToken cancellationToken = default)
    {
        var normalizedPhone = NormalizePhone(phoneNumber);
        if (normalizedPhone == null)
            return new(false, "Enter a valid phone number in international format, for example +14155552671.");

        var now = DateTime.UtcNow;
        var challenge = await _db.PhoneOtpChallenges
            .SingleOrDefaultAsync(item => item.PhoneNumber == normalizedPhone, cancellationToken);

        if (challenge != null && !challenge.IsConsumed && now - challenge.LastSentAt < SendCooldown)
            return new(false, "Wait one minute before requesting another code.");

        if (challenge == null)
        {
            challenge = new PhoneOtpChallenge { PhoneNumber = normalizedPhone };
            _db.PhoneOtpChallenges.Add(challenge);
        }

        if (now - challenge.WindowStartedAt >= SendWindow || challenge.IsConsumed)
        {
            challenge.WindowStartedAt = now;
            challenge.SendsInWindow = 0;
            challenge.FailedAttempts = 0;
            challenge.IsConsumed = false;
        }

        if (challenge.SendsInWindow >= MaxSendsPerWindow)
            return new(false, "OTP request limit reached. Try again in 15 minutes.");

        var code = RandomNumberGenerator.GetInt32(0, 1_000_000).ToString("D6");
        challenge.Salt = RandomNumberGenerator.GetBytes(32);
        challenge.CodeHash = HashCode(challenge.Salt, code);
        challenge.ExpiresAt = now.Add(OtpLifetime);
        challenge.LastSentAt = now;
        challenge.SendsInWindow++;
        challenge.FailedAttempts = 0;

        await _db.SaveChangesAsync(cancellationToken);
        var developmentCode = await _delivery.SendAsync(normalizedPhone, code, cancellationToken);
        return new(true, "A verification code was sent. It expires in five minutes.", developmentCode);
    }

    public async Task<OtpVerificationResult> VerifyAsync(string phoneNumber, string? code, CancellationToken cancellationToken = default)
    {
        var normalizedPhone = NormalizePhone(phoneNumber);
        var normalizedCode = code?.Trim();
        if (normalizedPhone == null || normalizedCode == null || !Regex.IsMatch(normalizedCode, "^\\d{6}$", RegexOptions.CultureInvariant))
            return new(false, "The verification code is invalid or expired.");

        var challenge = await _db.PhoneOtpChallenges
            .SingleOrDefaultAsync(item => item.PhoneNumber == normalizedPhone, cancellationToken);
        if (challenge == null || challenge.IsConsumed || challenge.ExpiresAt <= DateTime.UtcNow || challenge.FailedAttempts >= MaxFailedAttempts)
            return new(false, "The verification code is invalid or expired. Request a new code.");

        var submittedHash = HashCode(challenge.Salt, normalizedCode);
        if (!CryptographicOperations.FixedTimeEquals(challenge.CodeHash, submittedHash))
        {
            challenge.FailedAttempts++;
            await _db.SaveChangesAsync(cancellationToken);
            var remaining = Math.Max(0, MaxFailedAttempts - challenge.FailedAttempts);
            return new(false, remaining == 0 ? "Too many attempts. Request a new code." : $"Incorrect code. {remaining} attempt(s) remain.");
        }

        challenge.IsConsumed = true;
        await _db.SaveChangesAsync(cancellationToken);
        return new(true, "Phone number verified.");
    }

    private static string? NormalizePhone(string? phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
            return null;

        var normalized = Regex.Replace(phoneNumber.Trim(), "[\\s()\\-]", string.Empty, RegexOptions.CultureInvariant);
        return Regex.IsMatch(normalized, "^\\+[1-9]\\d{7,14}$", RegexOptions.CultureInvariant) ? normalized : null;
    }

    private static byte[] HashCode(byte[] salt, string code)
        => SHA256.HashData([.. salt, .. Encoding.UTF8.GetBytes(code)]);
}
