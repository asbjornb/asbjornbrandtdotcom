using System.Collections.Concurrent;

namespace SiteGenerator;

/// <summary>
/// Centralised helper for emitting one-time generation warnings.
/// </summary>
public static class GenerationWarnings
{
    private static readonly ConcurrentDictionary<string, byte> _seenWarnings = new();

    private static void Emit(string key, string message)
    {
        if (_seenWarnings.TryAdd(key, 0))
        {
            Console.WriteLine(message);
        }
    }

    public static void NoteMissingTitle(string noteSlug)
    {
        Emit(
            key: $"missing-title:{noteSlug}",
            message: $"Warning: Note '{noteSlug}' does not contain a top-level heading (<h1>); using the slug as the title."
        );
    }

    public static void NoteSlugHasDoubleSeparator(string noteSlug)
    {
        Emit(
            key: $"double-separator:{noteSlug}",
            message: $"Warning: Note slug '{noteSlug}' contains consecutive separators; consider renaming it for clarity."
        );
    }
}
