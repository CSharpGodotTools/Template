using Framework.Setup;
using Godot;

namespace Framework.Setup;

public class GameNameValidator(Label gameNamePreview, Timer feedbackResetTimer, LineEdit projectNameEdit)
{
    private const double FeedbackResetTime = 2.0;

    private string _prevGameName = "";

    public void Validate(string gameName)
    {
        feedbackResetTimer.Stop();

        if (string.IsNullOrWhiteSpace(gameName))
        {
            gameNamePreview.Text = "";
            _prevGameName = "";
            return;
        }

        if (char.IsNumber(gameName.Trim()[0]))
        {
            gameNamePreview.Text = "The first character cannot be a number";
            feedbackResetTimer.Start(FeedbackResetTime);
            ResetNameEdit();
            return;
        }

        if (!SetupUtils.IsAlphaNumericAndAllowSpaces(gameName))
        {
            gameNamePreview.Text = "Special characters are not allowed";
            feedbackResetTimer.Start(FeedbackResetTime);
            ResetNameEdit();
            return;
        }

        gameNamePreview.Text = SetupUtils.FormatGameName(gameName);
        _prevGameName = gameName;
        return;

        void ResetNameEdit()
        {
            projectNameEdit.Text = _prevGameName;
            projectNameEdit.CaretColumn = _prevGameName.Length;
        }
    }
}
