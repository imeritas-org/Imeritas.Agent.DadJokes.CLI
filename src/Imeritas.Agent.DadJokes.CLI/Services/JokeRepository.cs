using Imeritas.Agent.DadJokes.CLI.Models;

namespace Imeritas.Agent.DadJokes.CLI.Services;

/// <summary>
/// Static repository of embedded dad jokes. No instantiation required.
/// </summary>
public static class JokeRepository
{
    private static readonly IReadOnlyList<Joke> Jokes = new List<Joke>
    {
        new("joke-01", "Why do programmers prefer dark mode?", "Because light attracts bugs.", ["tech"]),
        new("joke-02", "What do you call a fake noodle?", "An impasta.", ["food"]),
        new("joke-03", "What do you call a bear with no teeth?", "A gummy bear.", ["animals"]),
        new("joke-04", "Why did the scarecrow get a promotion?", "Because he was outstanding in his field.", ["work"]),
        new("joke-05", "Why can't you trust atoms?", "They make up everything.", ["science"]),
        new("joke-06", "Why was the JavaScript developer sad?", "Because he didn't Node how to Express himself.", ["tech"]),
        new("joke-07", "What did the gingerbread man put on his bed?", "A cookie sheet.", ["food", "holiday"]),
        new("joke-08", "What do you call an alligator in a vest?", "An investigator.", ["animals"]),
        new("joke-09", "Why did the golfer bring two pairs of pants?", "In case he got a hole in one.", ["sports"]),
        new("joke-10", "Why did the musician get arrested?", "Because she got caught in a treble.", ["music"]),
        new("joke-11", "What's a computer's least favorite food?", "Spam.", ["tech", "work"]),
        new("joke-12", "Why did the chemist break up?", "There was no reaction.", ["science"]),
        new("joke-13", "What do you call cheese that isn't yours?", "Nacho cheese.", ["food"]),
        new("joke-14", "Why don't oysters share?", "Because they're shellfish.", ["animals", "science"]),
        new("joke-15", "What do snowmen eat for breakfast?", "Frosted Flakes.", ["holiday"]),
        new("joke-16", "Why did the bicycle fall over?", "Because it was two-tired.", ["sports"]),
        new("joke-17", "What's Beethoven's favorite fruit?", "Ba-na-na-naaa.", ["music"]),
        new("joke-18", "Why did the coffee file a police report?", "It got mugged.", ["work", "food"]),
        new("joke-19", "Why did the computer go to the doctor?", "Because it had a virus.", ["tech", "science"]),
        new("joke-20", "Why did the turkey join the band?", "Because it had the drumsticks.", ["holiday", "food"]),
    }.AsReadOnly();

    /// <summary>
    /// Returns jokes matching the given category (case-insensitive).
    /// Returns an empty list for unknown categories.
    /// </summary>
    public static IReadOnlyList<Joke> GetByCategory(string category)
    {
        return Jokes
            .Where(j => j.Categories.Any(c =>
                string.Equals(c, category, StringComparison.OrdinalIgnoreCase)))
            .ToList()
            .AsReadOnly();
    }

    /// <summary>
    /// Returns a single random joke from the full collection.
    /// </summary>
    public static Joke GetRandom()
    {
        return Jokes[Random.Shared.Next(Jokes.Count)];
    }

    /// <summary>
    /// Returns all distinct categories, sorted alphabetically.
    /// </summary>
    public static IReadOnlyList<string> GetAllCategories()
    {
        return Jokes
            .SelectMany(j => j.Categories)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(c => c, StringComparer.OrdinalIgnoreCase)
            .ToList()
            .AsReadOnly();
    }
}
