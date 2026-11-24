using Microsoft.EntityFrameworkCore;
using SmartDealFinder.Models;
using SmartDealFinder.Models.Core;
using SmartDealFinder.Service;

namespace SmartCompare.Tests;

public sealed class PhoneOtpServiceTests
{
    [Fact]
    public async Task SendAndVerify_ConsumesOtpAfterOneSuccessfulUse()
    {
        await using var database = CreateDatabase();
        var delivery = new CapturingOtpDelivery();
        var service = new PhoneOtpService(database, delivery);

        var sent = await service.SendAsync("+1 (415) 555-2671");
        var verified = await service.VerifyAsync("+14155552671", delivery.Code);
        var reused = await service.VerifyAsync("+14155552671", delivery.Code);

        Assert.True(sent.Succeeded);
        Assert.Equal("+14155552671", delivery.PhoneNumber);
        Assert.True(verified.Succeeded);
        Assert.False(reused.Succeeded);
    }

    [Fact]
    public async Task Verify_RejectsExpiredOtp()
    {
        await using var database = CreateDatabase();
        var delivery = new CapturingOtpDelivery();
        var service = new PhoneOtpService(database, delivery);
        await service.SendAsync("+14155552671");
        var challenge = await database.PhoneOtpChallenges.SingleAsync();
        challenge.ExpiresAt = DateTime.UtcNow.AddSeconds(-1);
        await database.SaveChangesAsync();

        var result = await service.VerifyAsync("+14155552671", delivery.Code);

        Assert.False(result.Succeeded);
        Assert.Contains("expired", result.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Verify_RejectsCorrectOtpAfterFiveFailedAttempts()
    {
        await using var database = CreateDatabase();
        var delivery = new CapturingOtpDelivery();
        var service = new PhoneOtpService(database, delivery);
        await service.SendAsync("+14155552671");
        var incorrectCode = delivery.Code == "000000" ? "000001" : "000000";

        for (var attempt = 0; attempt < 5; attempt++)
            await service.VerifyAsync("+14155552671", incorrectCode);

        var result = await service.VerifyAsync("+14155552671", delivery.Code);

        Assert.False(result.Succeeded);
        Assert.Equal(5, (await database.PhoneOtpChallenges.SingleAsync()).FailedAttempts);
    }

    [Fact]
    public async Task Send_EnforcesCooldownAndThreeSendsPerWindow()
    {
        await using var database = CreateDatabase();
        var delivery = new CapturingOtpDelivery();
        var service = new PhoneOtpService(database, delivery);
        Assert.True((await service.SendAsync("+14155552671")).Succeeded);

        var cooldownResult = await service.SendAsync("+14155552671");
        Assert.False(cooldownResult.Succeeded);
        Assert.Contains("one minute", cooldownResult.Message, StringComparison.OrdinalIgnoreCase);

        var challenge = await database.PhoneOtpChallenges.SingleAsync();
        for (var send = 2; send <= 3; send++)
        {
            challenge.LastSentAt = DateTime.UtcNow.AddMinutes(-2);
            await database.SaveChangesAsync();
            Assert.True((await service.SendAsync("+14155552671")).Succeeded);
        }

        challenge.LastSentAt = DateTime.UtcNow.AddMinutes(-2);
        await database.SaveChangesAsync();
        var limitResult = await service.SendAsync("+14155552671");

        Assert.False(limitResult.Succeeded);
        Assert.Contains("limit", limitResult.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(3, challenge.SendsInWindow);
    }

    private static DatabaseContext CreateDatabase()
    {
        var options = new DbContextOptionsBuilder<DatabaseContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new DatabaseContext(options);
    }

    private sealed class CapturingOtpDelivery : IOtpDelivery
    {
        public string Code { get; private set; } = string.Empty;
        public string PhoneNumber { get; private set; } = string.Empty;

        public Task<string?> SendAsync(string phoneNumber, string code, CancellationToken cancellationToken)
        {
            PhoneNumber = phoneNumber;
            Code = code;
            return Task.FromResult<string?>(code);
        }
    }
}
