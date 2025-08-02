using Godot;
using GodotUtils;
using System.IO;

namespace __TEMPLATE__.Setup;

[SceneTree]
public partial class SetupUI : Node
{
    private string _prevGameName = string.Empty;

    public override void _Ready()
    {
        SetupUtils.DisplayGameNamePreview("Undefined", NamePreview);
    }

    private void _OnYesPressed()
    {
        string gameName = SetupUtils.FormatGameName(GameName.Text);
        string path = ProjectSettings.GlobalizePath("res://");

        // Prevent namespace being the same name as a class name in the project
        bool namespaceSameAsClassName = false;

        DirectoryUtils.Traverse("res://", fullFilePath =>
        {
            if (Path.GetFileName(fullFilePath).Equals(GameName.Text + ".cs"))
            {
                namespaceSameAsClassName = true;
                return true;
            }

            return false;
        });

        if (namespaceSameAsClassName)
        {
            GD.PrintErr($"Namespace {GameName.Text} is the same name as {GameName.Text}.cs");
            return;
        }

        // The IO functions ran below will break if empty folders exist
        DirectoryUtils.DeleteEmptyDirectories(path);

        SetupUtils.SetMainScene(path, "Level");
        SetupUtils.RenameProjectFiles(path, gameName);
        SetupUtils.RenameAllNamespaces(path, gameName);

        // Delete the "res://Setup" directory
        Directory.Delete(Path.Combine(path, "Setup"), recursive: true);

        // Ensure all empty folders are deleted when finished
        DirectoryUtils.DeleteEmptyDirectories(path);

        GetTree().Quit();
        SetupEditor.Restart();
    }

    private void _OnGameNameTextChanged(string newText)
    {
        if (string.IsNullOrWhiteSpace(newText))
            return;

        // Since this name is being used for the namespace its first character must not be
        // a number and every other character must be alphanumeric
        if (!SetupUtils.IsAlphaNumericAndAllowSpaces(newText) || char.IsNumber(newText.Trim()[0]))
        {
            SetupUtils.DisplayGameNamePreview(_prevGameName, NamePreview);
            GameName.Text = _prevGameName;
            GameName.CaretColumn = _prevGameName.Length;
            return;
        }

        SetupUtils.DisplayGameNamePreview(newText, NamePreview);
        _prevGameName = newText;
    }

    private void _OnNoPressed()
    {
        NodePopupPanel.Hide();
    }

    private void _OnApplyChangesPressed() 
    {
        string gameName = SetupUtils.FormatGameName(GameName.Text);

        if (string.IsNullOrWhiteSpace(gameName))
        {
            GD.Print("Please type a game name first!");
            return;
        }

        NodePopupPanel.PopupCentered();
    }
}
