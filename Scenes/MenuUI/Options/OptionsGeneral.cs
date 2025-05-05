using Godot;

namespace Template.UI;

public partial class OptionsGeneral : Control
{
    [Export] private OptionsManager _optionsManager;
    private ResourceOptions _options;

    public override void _Ready()
    {
        _options = _optionsManager.Options;
        SetupLanguage();
    }

    private void SetupLanguage()
    {
        OptionButton optionButtonLanguage = GetNode<OptionButton>("%LanguageButton");
        optionButtonLanguage.Select((int)_options.Language);
    }

    private void _on_language_item_selected(int index)
    {
        string locale = ((Language)index).ToString().Substring(0, 2).ToLower();

        TranslationServer.SetLocale(locale);

        _options.Language = (Language)index;
    }
}

public enum Language
{
    English,
    French,
    Japanese
}

