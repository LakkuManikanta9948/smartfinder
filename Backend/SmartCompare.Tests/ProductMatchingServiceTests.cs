using SmartDealFinder.Models;
using SmartDealFinder.Service;

namespace SmartCompare.Tests;

public sealed class ProductMatchingServiceTests
{
    [Fact]
    public void RankCandidates_PrioritizesExactGtinAndKeepsUncertainCandidatesReviewable()
    {
        var source = new Product
        {
            Id = 1,
            Title = "Acme Phone X 256GB",
            Gtin = "12345678901234",
            Brand = new Brand { Name = "Acme" },
            CategoryId = 4
        };
        var exact = new Product
        {
            Id = 2,
            Title = "Acme X Smartphone 256GB",
            Gtin = "12345678901234",
            Brand = new Brand { Name = "Acme" },
            CategoryId = 4
        };
        var conflictingGtin = new Product
        {
            Id = 3,
            Title = "Acme Phone X 256GB",
            Gtin = "99999999999999",
            Brand = new Brand { Name = "Acme" },
            CategoryId = 4
        };

        var result = new ProductMatchingService().RankCandidates(source, [exact, conflictingGtin]);

        var match = Assert.Single(result);
        Assert.Equal(exact, match.Product);
        Assert.True(match.Confidence >= 0.65m);
        Assert.Contains("matching GTIN", match.Reason);
        Assert.False(match.RequiresManualReview);
    }

    [Fact]
    public void RankCandidates_RejectsConflictingModelAndVariant()
    {
        var source = new Product
        {
            Id = 1,
            Title = "Acme Phone X",
            ModelNumber = "X100",
            Variant = "Blue 256GB",
            Brand = new Brand { Name = "Acme" }
        };
        var wrongModel = new Product
        {
            Id = 2,
            Title = "Acme Phone X",
            ModelNumber = "X200",
            Variant = "Blue 256GB",
            Brand = new Brand { Name = "Acme" }
        };
        var wrongVariant = new Product
        {
            Id = 3,
            Title = "Acme Phone X",
            ModelNumber = "X100",
            Variant = "Black 128GB",
            Brand = new Brand { Name = "Acme" }
        };

        var result = new ProductMatchingService().RankCandidates(source, [wrongModel, wrongVariant]);

        Assert.Empty(result);
    }
}
