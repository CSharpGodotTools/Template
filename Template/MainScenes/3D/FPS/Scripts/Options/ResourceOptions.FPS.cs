namespace Framework;

public enum Difficulty
{
    Easy,
    Normal,
    Hard
}

public partial class ResourceOptions
{
    public Difficulty Difficulty { get; set; } = Difficulty.Normal;
    public float MouseSensitivity { get; set; } = 0.85f;
}
