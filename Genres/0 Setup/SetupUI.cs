using Godot;
using GodotUtils;
using System.IO;

namespace __TEMPLATE__.Setup;

[SceneTree]
public partial class SetupUI : Node
{
    private Genre _genre;
    private string _prevGameName = string.Empty;

    public override void _Ready()
    {
        GenreInfo.Text = "Genre info";

        _genre = (Genre)GenreBtn.Selected;
        SetupUtils.SetGenreSelectedInfo(GenrePreview, _genre);
        SetupUtils.DisplayGameNamePreview("Undefined", NamePreview);
        SetupUtils.SetGenreTipInfo(GenreInfo, Genre.None);
    }

    private void _on_yes_pressed()
    {
        string gameName = SetupUtils.FormatGameName(GameName.Text);
        string path = ProjectSettings.GlobalizePath("res://");

        // The IO functions ran below will break if empty folders exist
        DirectoryUtils.DeleteEmptyDirectories(path);

        string genreFolder = Path.Combine(path, "Genres", SetupUtils.FolderNames[_genre]);
        
        if (!Directory.Exists(genreFolder))
        {
            GD.PrintErr($"Genre folder '{genreFolder}' does not exist.");
            return;
        }

        SetupManager.RenameProjectFiles(path, gameName);
        SetupManager.RenameAllNamespaces(path, gameName);
        SetupManager.SetupVSCodeTemplates(GodotExe.Text, gameName);

        if (MoveProjectFiles.ButtonPressed)
        {
            SetupManager.MoveProjectFiles(_genre,
                pathFrom: Path.Combine(path, "Genres"), 
                pathTo: path, 
                deleteOtherGenres: DeleteOtherGenres.ButtonPressed);

            SceneFileUtils.FixBrokenDependencies();
        }

        if (DeleteSetupScene.ButtonPressed)
        {
            // Delete the "0 Setup" directory
            Directory.Delete(Path.Combine(path, "Genres", "0 Setup"), true);
        }

        if (DeleteSandboxFolder.ButtonPressed)
        {
            // Delete the "Sandbox" directory
            Directory.Delete(Path.Combine(path, "Sandbox"), true);
        }

        // Ensure all empty folders are deleted when finished
        DirectoryUtils.DeleteEmptyDirectories(path);

        GetTree().Quit();
        SetupEditor.Restart();
    }

    private void _on_genre_item_selected(int index)
    {
        _genre = (Genre)index;
        SetupUtils.SetGenreSelectedInfo(GenrePreview, _genre);
        SetupUtils.SetGenreTipInfo(GenreInfo, _genre);
    }

    private void _on_game_name_text_changed(string newText)
    {
        if (string.IsNullOrWhiteSpace(newText))
        {
            return;
        }

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

    private void _on_no_pressed()
    {
        NodePopupPanel.Hide();
    }

    private void _on_apply_changes_pressed() 
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
