using Godot;
using GodotSharp.SourceGenerators;
using GodotUtils;

namespace __TEMPLATE__.UI;

[SceneTree]
public partial class Options : PanelContainer
{
    [OnInstantiate]
    private void Init()
    {

    }

    public override void _Ready()
    {
        ThemeUtils.Adjust(this);
    }
}

public class ThemeUtils
{
    private static readonly StringName Button = "Button";

    public static void Adjust(Node root)
    {
        root.TraverseNodes(OnTraverseNode);
    }

    private static void OnTraverseNode(Node node)
    {
        if (node is not Control control)
            return;

        Theme theme = control.Theme;

        if (theme == null)
            return;

        StyleBoxFlat normalStyleBox = GetNormalStyleBox(theme);

        StyleBoxFlat hoverStyleBox = normalStyleBox.Duplicate() as StyleBoxFlat;
        hoverStyleBox.BgColor = normalStyleBox.BgColor + GrayColor(10);

        StyleBoxFlat pressedStyleBox = hoverStyleBox.Duplicate() as StyleBoxFlat;
        pressedStyleBox.BgColor = hoverStyleBox.BgColor + GrayColor(10);

        StyleBoxFlat focusStyleBox = normalStyleBox;

        theme.SetStylebox("hover", Button, hoverStyleBox);
        theme.SetStylebox("pressed", Button, pressedStyleBox);
        theme.SetStylebox("focus", Button, focusStyleBox);
    }

    private static Color GrayColor(byte saturation)
    {
        return Color.Color8(saturation, saturation, saturation);
    }

    private static StyleBoxFlat GetNormalStyleBox(Theme theme)
    {
        if (theme.GetStylebox("normal", Button) is StyleBoxFlat styleBox)
        {
            return styleBox;
        }

        return null;
    }
}
