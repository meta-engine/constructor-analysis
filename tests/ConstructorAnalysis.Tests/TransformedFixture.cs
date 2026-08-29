namespace ConstructorAnalysis.Tests;

public class TransformedFixture
{
    public required string Email { get; set; }
    public required string FullName { get; set; }
    public TransformedFixture(string email, string first, string last)
    {
        Email = email.ToLowerInvariant();
        FullName = $"{first.Trim()}::{last.ToUpperInvariant()}";
    }
}
