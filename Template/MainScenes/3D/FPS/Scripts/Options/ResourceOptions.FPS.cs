using System;

namespace __TEMPLATE__;

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

    partial void NormalizeExtended()
    {
        if (!Enum.IsDefined(typeof(Difficulty), Difficulty))
            Difficulty = Difficulty.Normal;

        MouseSensitivity = Math.Clamp(MouseSensitivity, 0.1f, 2.0f);
    }
}
