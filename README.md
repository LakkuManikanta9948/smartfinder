# SmartCompare

SmartCompare is an ASP.NET Core MVC product catalog and comparison application backed by MySQL and EF Core. Retailer offers are only treated as live/verified when supplied by an authorized provider; development examples are labeled and are not purchaseable.

## Run locally

Requirements: .NET 10 SDK and MySQL Server 8.x running on localhost:3306. The app connects to the `smart_deal_finder` schema. Keep database, OAuth, and bootstrap-admin credentials in User Secrets or a secret manager, never in `appsettings.json` or source control.

From the repository root in PowerShell, configure the local connection string and optional Google OAuth credentials, then run:

```powershell
dotnet restore SmartDealFinder.slnx
$env:ConnectionStrings__DefaultConnection = "Server=localhost;Port=3306;Database=smart_deal_finder;User=root;Password=$env:MYSQL_ROOT_PASSWORD"
$env:Bootstrap__OrgAdminPhone = "+14155552671"
dotnet run --project Backend\SmartDealFinder.UI\SmartDealFinder.UI.csproj
```

On startup, the application applies EF Core migrations (creating the schema if the MySQL account has database-creation privileges), creates the system roles, and provisions the configured ORGADMIN phone if it does not exist. Set `Bootstrap__OrgAdminPhone` only in a trusted environment; never set it from public registration. The development OTP provider returns the code in the login flow and writes it to development logs. Do not enable this provider in production.

To apply migrations explicitly, use the same `ConnectionStrings__DefaultConnection` environment variable:

```powershell
dotnet ef database update --project Backend\SmartDealFinder.Models\SmartDealFinder.Models.csproj --startup-project Backend\SmartDealFinder.UI\SmartDealFinder.UI.csproj
```

Alternatively, in MySQL Workbench, connect to MySQL 8 on localhost, open `Database\\Scripts\\InitialSmartCompare.mysql.sql`, and execute it only for a new/empty schema. Do not run that script against an existing application database. On startup, the app recognizes the supported legacy catalog layout, renames its five old tables to `legacy_*_v1`, applies EF migrations, and copies categories, brands, merchants, and products into the new schema. The original tables are retained as a data backup; legacy wishlists remain archived because their old user records cannot be safely mapped to ASP.NET Identity accounts. The app stops without renaming tables if it finds an unsupported or partial schema. Keep a separate database backup before upgrades.

## Production configuration

- Configure `ConnectionStrings__DefaultConnection` using a managed secret store or environment variables; do not commit credentials.
- Configure Google OAuth with `Authentication__Google__ClientId` and `Authentication__Google__ClientSecret` in User Secrets or a managed secret store. Register the app's `/signin-google` callback URL with Google. Google users must have a verified email; phone OTP is an alternative sign-in path.
- Provision the initial ORGADMIN with `Bootstrap__OrgAdminEmail` and `Bootstrap__OrgAdminPassword` in trusted secret storage, or use `Bootstrap__OrgAdminPhone` for OTP-based provisioning. Do not expose bootstrap settings publicly.
- Configure the SMS adapter with `Sms__UseHttpProvider=true`, an HTTPS `Sms__Endpoint`, `Sms__ApiToken`, and `Sms__SenderId`. The adapter expects the gateway to accept the documented JSON payload and a bearer token. The gateway's delivery contract must be verified before production use.
- Remove bootstrap credentials from the environment after initial provisioning and rotate any credentials that have been shared outside trusted secret storage.
- Use HTTPS and a production-grade managed MySQL service. Local MySQL and sample offers are development-only.
- No live Amazon, Flipkart, or other retailer integration is configured in this repository. Implement and configure authorized APIs, affiliate feeds, or approved providers before claiming live pricing. Do not scrape retailer sites or present sample data as live.

## Tests

```powershell
dotnet test Backend\SmartCompare.Tests\SmartCompare.Tests.csproj
```

The tests cover OTP expiry, one-time use, throttling/attempt limits, and safe retailer-link validation.
# smartfinder
