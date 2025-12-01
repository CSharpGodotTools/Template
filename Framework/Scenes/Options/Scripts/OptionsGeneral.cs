using Godot;

namespace GodotUtils.UI;

public class OptionsGeneral(Options options)
{
    private ResourceOptions _options;

    public void Initialize()
    {
        _options = OptionsManager.GetOptions();

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
