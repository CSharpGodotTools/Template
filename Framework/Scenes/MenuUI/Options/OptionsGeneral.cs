using Godot;

namespace __TEMPLATE__.UI;

public partial class OptionsGeneral : Control
{
    private ResourceOptions _options;

    public override void _Ready()
    {
        _options = GetNode<OptionsManager>(AutoloadPaths.OptionsManager).Options;
        SetupLanguage();
    }

    private void SetupLanguage()
    {
        OptionButton optionButtonLanguage = GetNode<OptionButton>("%LanguageButton");
        optionButtonLanguage.Select((int)_options.Language);
    }

    private void _OnLanguageItemSelected(int index)
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
