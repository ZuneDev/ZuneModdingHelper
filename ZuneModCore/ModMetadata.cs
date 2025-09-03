namespace ZuneModCore;

public record ModMetadata
{
    public required string Id { get; init; }

    public required string Title { get; init; }

    public required string Author { get; init; }

    public required ReleaseVersion Version { get; init; }

    public required string Description { get; init; }

    public string? ExtendedDescription { get; init; }
}
