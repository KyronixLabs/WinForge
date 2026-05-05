namespace WinForge.Models;

public sealed class OptimisationRun
{
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public string MachineName { get; set; } = Environment.MachineName;
    public string WindowsUser { get; set; } = Environment.UserName;
    public List<AppliedOption> AppliedOptions { get; set; } = new();
}

public sealed class AppliedOption
{
    public required string Id { get; set; }
    public required string Title { get; set; }
    public string? RevertScript { get; set; }
    public DateTime AppliedAt { get; set; } = DateTime.Now;
}
