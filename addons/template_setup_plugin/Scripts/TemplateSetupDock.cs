#if TOOLS
using Godot;
using GodotUtils;

[Tool]
public partial class TemplateSetupDock : VBoxContainer
{
    private Label _gameNamePreview;
    private LineEdit _projectNameEdit;

    public override void _Ready()
    {
        MarginContainer margin = MarginContainerFactory.Create(30);
        HBoxContainer hbox = new();

        hbox.AddChild(new Label { Text = "Project Name:" });

        _projectNameEdit = new()
        {
            SizeFlagsHorizontal = SizeFlags.ShrinkBegin,
            CustomMinimumSize = new Vector2(200, 0)
        };

        hbox.AddChild(_projectNameEdit);
        margin.AddChild(hbox);

        AddChild(margin);

        Button applyButton = new()
        {
            Text = "Apply Setup",
            SizeFlagsHorizontal = SizeFlags.ShrinkCenter,
            CustomMinimumSize = new Vector2(200, 0)
        };

        applyButton.Pressed += OnApplyPressed;
        AddChild(applyButton);
    }

    private void OnApplyPressed()
    {
        string projectName = _projectNameEdit.Text;

        GD.Print(projectName);
    }
}
#endif
