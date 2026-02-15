using Godot;

namespace Framework.Setup;

public sealed class GameNameValidator
{
    private const double FeedbackResetTime = 2.0;

    private readonly Label _gameNamePreview;
    private readonly Timer _feedbackResetTimer;
    private readonly LineEdit _projectNameEdit;

    private string _previousValidGameName = string.Empty;

    public GameNameValidator(Label gameNamePreview, Timer feedbackResetTimer, LineEdit projectNameEdit)
    {
        _gameNamePreview = gameNamePreview;
        _feedbackResetTimer = feedbackResetTimer;
        _projectNameEdit = projectNameEdit;
    }

    public void Validate(string gameName)
    {
        _feedbackResetTimer.Stop();

        if (string.IsNullOrWhiteSpace(gameName))
        {
            _gameNamePreview.Text = string.Empty;
            _previousValidGameName = string.Empty;
            return;
        }

        if (char.IsNumber(gameName.Trim()[0]))
        {
            _gameNamePreview.Text = "The first character cannot be a number";
            _feedbackResetTimer.Start(FeedbackResetTime);
            ResetNameEdit();
            return;
        }

        if (!GameNameRules.IsAlphaNumericAndAllowSpaces(gameName))
        {
            _gameNamePreview.Text = "Special characters are not allowed";
            _feedbackResetTimer.Start(FeedbackResetTime);
            ResetNameEdit();
            return;
        }

        _gameNamePreview.Text = GameNameRules.FormatGameName(gameName);
        _previousValidGameName = gameName;

        void ResetNameEdit()
        {
            _projectNameEdit.Text = _previousValidGameName;
            _projectNameEdit.CaretColumn = _previousValidGameName.Length;
        }
    }

    public void RestorePreviousGameNamePreview()
    {
        _gameNamePreview.Text = _previousValidGameName;
    }
}
