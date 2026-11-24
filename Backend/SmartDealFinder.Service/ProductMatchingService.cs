using System.Text.RegularExpressions;
using SmartDealFinder.Models;

namespace SmartDealFinder.Service;

public sealed record ProductMatchCandidate(Product Product, decimal Confidence, bool RequiresManualReview, string Reason);

public interface IProductMatchingService
{
    IReadOnlyList<ProductMatchCandidate> RankCandidates(Product source, IEnumerable<Product> candidates, decimal minimumConfidence = 0.65m);
}

public sealed class ProductMatchingService : IProductMatchingService
{
    public IReadOnlyList<ProductMatchCandidate> RankCandidates(Product source, IEnumerable<Product> candidates, decimal minimumConfidence = 0.65m)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(candidates);
        if (minimumConfidence is < 0 or > 1)
            throw new ArgumentOutOfRangeException(nameof(minimumConfidence));

        var sourceModel = Normalize(source.ModelNumber ?? source.MerchantSku);
        var sourceGtin = Normalize(source.Gtin);
        var sourceBrand = Normalize(source.Brand?.Name);
        var sourceVariant = Normalize(source.Variant);
        var sourceTitle = Tokens(source.Title);
        var results = new List<ProductMatchCandidate>();
        foreach (var candidate in candidates)
        {
            if (candidate.Id == source.Id || candidate.Status != 1 || !candidate.Active)
                continue;

            var candidateBrand = Normalize(candidate.Brand?.Name);
            var candidateModel = Normalize(candidate.ModelNumber ?? candidate.MerchantSku);
            var candidateGtin = Normalize(candidate.Gtin);
            var candidateVariant = Normalize(candidate.Variant);
            if (!string.IsNullOrEmpty(sourceBrand) && !string.IsNullOrEmpty(candidateBrand)
                && !string.Equals(sourceBrand, candidateBrand, StringComparison.Ordinal))
                continue;
            if (!string.IsNullOrEmpty(sourceModel) && !string.IsNullOrEmpty(candidateModel)
                && !string.Equals(sourceModel, candidateModel, StringComparison.Ordinal))
                continue;
            if (!string.IsNullOrEmpty(sourceGtin) && !string.IsNullOrEmpty(candidateGtin)
                && !string.Equals(sourceGtin, candidateGtin, StringComparison.Ordinal))
                continue;
            if (!string.IsNullOrEmpty(sourceVariant) && !string.IsNullOrEmpty(candidateVariant)
                && !string.Equals(sourceVariant, candidateVariant, StringComparison.Ordinal))
                continue;

            var score = 0m;
            var reasons = new List<string>();
            if (!string.IsNullOrEmpty(sourceGtin) && sourceGtin == candidateGtin)
            {
                score += 0.7m;
                reasons.Add("matching GTIN");
            }
            if (!string.IsNullOrEmpty(sourceModel) && sourceModel == candidateModel)
            {
                score += 0.5m;
                reasons.Add("matching model/SKU");
            }
            if (!string.IsNullOrEmpty(sourceBrand) && sourceBrand == candidateBrand)
            {
                score += 0.15m;
                reasons.Add("matching brand");
            }
            if (source.CategoryId == candidate.CategoryId)
            {
                score += 0.05m;
                reasons.Add("matching category");
            }

            var candidateTitle = Tokens(candidate.Title);
            var union = sourceTitle.Union(candidateTitle, StringComparer.Ordinal).Count();
            var titleSimilarity = union == 0 ? 0m : (decimal)sourceTitle.Intersect(candidateTitle, StringComparer.Ordinal).Count() / union;
            score += titleSimilarity * 0.1m;
            if (!string.IsNullOrEmpty(sourceVariant) && sourceVariant == candidateVariant)
            {
                score += 0.1m;
                reasons.Add("matching variant");
            }
            if (titleSimilarity > 0.5m)
                reasons.Add("similar product title");

            if (score >= minimumConfidence)
                results.Add(new ProductMatchCandidate(candidate, Math.Min(score, 1m), score < 0.85m, string.Join(", ", reasons)));
        }

        return results.OrderByDescending(item => item.Confidence).ThenBy(item => item.Product.Title).ToList();
    }

    private static HashSet<string> Tokens(string value)
        => Regex.Matches(Normalize(value), @"[a-z0-9]+", RegexOptions.CultureInvariant)
            .Select(match => match.Value)
            .Where(token => token.Length > 1)
            .ToHashSet(StringComparer.Ordinal);

    private static string Normalize(string? value)
        => string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim().ToLowerInvariant();
}
