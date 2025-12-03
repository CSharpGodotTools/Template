using Godot;

namespace __TEMPLATE__.UI;

public class OptionsGeneral(Options options)
{
    private ResourceOptions _options;

    public void Initialize()
    {
        GetOptions();
        SetupLanguage();
    }

    private void GetOptions()
    {
        _options = Game.Options.GetOptions();
    }

    private void SetupLanguage()
    {
        OptionButton languageBtn = options.GetNode<OptionButton>("%LanguageButton");
        languageBtn.ItemSelected += OnLanguageItemSelected;
        languageBtn.Select((int)_options.Language);
    }

    private void OnLanguageItemSelected(long index)
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
