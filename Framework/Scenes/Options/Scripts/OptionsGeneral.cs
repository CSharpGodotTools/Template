using Godot;
using GodotUtils;
using GodotUtils.UI;

namespace __TEMPLATE__.UI;

public partial class OptionsGeneral : Control
{
    private ResourceOptions _options;

    public override void _Ready()
    {
        _options = OptionsManager.GetOptions();
        SetupLanguage();
    }

    private void _OnLanguageItemSelected(int index)
    {
        string locale = ((Language)index).ToString().Substring(0, 2).ToLower();

        TranslationServer.SetLocale(locale);

        _options.Language = (Language)index;
    }

    private void SetupLanguage()
    {
        OptionButton optionButtonLanguage = GetNode<OptionButton>("%LanguageButton");
        optionButtonLanguage.Select((int)_options.Language);
    }
}

public enum Language
{
    English,
    French,
    Japanese
}
