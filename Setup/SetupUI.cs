using Godot;
using GodotUtils;
using System.IO;

namespace Framework.Setup;

public partial class SetupUI : Node
{
    // Fields
    private string _prevGameName = string.Empty;
    private LineEdit _gameNameLineEdit;
    private RichTextLabel _namePreviewLabel;
    private PopupPanel _popupPanel;

    // Godot Overrides
    public override void _Ready()
    {
        _gameNameLineEdit = GetNode<LineEdit>("%GameName");
        _namePreviewLabel = GetNode<RichTextLabel>("%NamePreview");
        _popupPanel = GetNode<PopupPanel>("%NodePopupPanel");

        SetupUtils.DisplayGameNamePreview("Undefined", _namePreviewLabel);
    }

    // Subscribers
#pragma warning disable IDE1006 // Naming Styles
    private void _OnYesPressed()
    {
        string rawGameName = _gameNameLineEdit.Text;
        string formattedGameName = SetupUtils.FormatGameName(rawGameName);
        string path = ProjectSettings.GlobalizePath("res://");

        if (SetupUtils.IsGameNameBad(rawGameName))
            return;

        // The IO functions ran below will break if empty folders exist
        DirectoryUtils.DeleteEmptyDirectories(path);

        SetupUtils.SetMainScene(path, "Level");
        SetupUtils.RenameProjectFiles(path, formattedGameName);
        SetupUtils.RenameAllNamespaces(path, formattedGameName);

        // Delete the "res://Setup" directory
        Directory.Delete(Path.Combine(path, "Setup"), recursive: true);

        // Ensure all empty folders are deleted when finished
        DirectoryUtils.DeleteEmptyDirectories(path);

        SetupEditor.Restart();
        GetTree().Quit();
    }

    private void _OnGameNameTextChanged(string newText)
    {
        if (string.IsNullOrWhiteSpace(newText))
            return;

        // Since this name is being used for the namespace its first character must not be
        // a number and every other character must be alphanumeric
        if (!SetupUtils.IsAlphaNumericAndAllowSpaces(newText) || char.IsNumber(newText.Trim()[0]))
        {
            SetupUtils.DisplayGameNamePreview(_prevGameName, _namePreviewLabel);
            _gameNameLineEdit.Text = _prevGameName;
            _gameNameLineEdit.CaretColumn = _prevGameName.Length;
            return;
        }

        SetupUtils.DisplayGameNamePreview(newText, _namePreviewLabel);
        _prevGameName = newText;
    }

    private void _OnNoPressed()
    {
        _popupPanel.Hide();
    }

    private void _OnApplyChangesPressed() 
    {
        string gameName = SetupUtils.FormatGameName(_gameNameLineEdit.Text);

        if (string.IsNullOrWhiteSpace(gameName))
        {
            GD.Print("Please type a game name first!");
            return;
        }

        _popupPanel.PopupCentered();
    }
#pragma warning restore IDE1006 // Naming Styles
}
