namespace WinForge.Models;

public sealed class ToolAction
{
    public required string Title { get; init; }
    public required string Description { get; init; }
    public required string Script { get; init; }
    public bool RequiresAdmin { get; init; }
    public bool IsDestructive { get; init; }
}

public sealed class SnapshotInfo
{
    public required string Name { get; init; }
    public required string Path { get; init; }
    public DateTime CreatedAt { get; init; }
    public int AppliedCount { get; init; }
}
