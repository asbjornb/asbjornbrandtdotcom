using System.Threading;
using Markdig;

namespace SiteGenerator.MarkdownSupport;

public static class MarkdownPipelineFactory
{
    private static readonly Lazy<MarkdownPipeline> _pipeline =
        new(
            () =>
                new MarkdownPipelineBuilder()
                    .UseAutoLinks()
                    .UseEmphasisExtras()
                    .UseTaskLists()
                    .UseEmojiAndSmiley()
                    .Build(),
            LazyThreadSafetyMode.ExecutionAndPublication
        );

    public static MarkdownPipeline GetPipeline() => _pipeline.Value;
}
