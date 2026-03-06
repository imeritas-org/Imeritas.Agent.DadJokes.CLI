using Imeritas.Agent.DadJokes.CLI.Models;
using Imeritas.Agent.DadJokes.CLI.Services;
using Xunit;

namespace Imeritas.Agent.DadJokes.CLI.Tests;

public class JokeRepositoryTests
{
    [Fact]
    public void GetByCategory_ValidCategory_ReturnsMatchingJokes()
    {
        var categories = JokeRepository.GetAllCategories();
        var category = categories[0];

        var results = JokeRepository.GetByCategory(category);

        Assert.NotEmpty(results);
        Assert.All(results, joke =>
            Assert.Contains(joke.Categories, c =>
                string.Equals(c, category, StringComparison.OrdinalIgnoreCase)));
    }

    [Fact]
    public void GetByCategory_UnknownCategory_ReturnsEmpty()
    {
        var results = JokeRepository.GetByCategory("nonexistent_xyz_category");

        Assert.Empty(results);
    }

    [Fact]
    public void GetByCategory_IsCaseInsensitive()
    {
        var categories = JokeRepository.GetAllCategories();
        var category = categories[0];

        var results = JokeRepository.GetByCategory(category.ToUpperInvariant());

        Assert.NotEmpty(results);
    }

    [Fact]
    public void GetRandom_ReturnsNonNullJoke()
    {
        var joke = JokeRepository.GetRandom();

        Assert.NotNull(joke);
        Assert.NotEmpty(joke.Setup);
        Assert.NotEmpty(joke.Punchline);
    }

    [Fact]
    public void GetAllCategories_ReturnsSortedDistinctCategories()
    {
        var categories = JokeRepository.GetAllCategories();

        Assert.True(categories.Count >= 2);
        Assert.Equal(
            categories.OrderBy(c => c, StringComparer.OrdinalIgnoreCase).Distinct(StringComparer.OrdinalIgnoreCase).ToList(),
            categories);
    }

    [Fact]
    public void Collection_ContainsAtLeast20Jokes()
    {
        var categories = JokeRepository.GetAllCategories();
        var allJokes = categories
            .SelectMany(c => JokeRepository.GetByCategory(c))
            .DistinctBy(j => j.Id)
            .ToList();

        Assert.True(allJokes.Count >= 20);
    }
}
