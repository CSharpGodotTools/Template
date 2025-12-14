using Godot;

namespace __TEMPLATE__.UI;

public class OptionsGeneral
{
    private ResourceOptions _options;
    private Button _generalBtn;
    private readonly Options options;

    public OptionsGeneral(Options options, Button generalBtn)
    {
        this.options = options;
        _generalBtn = generalBtn;

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
        languageBtn.FocusNeighborLeft = _generalBtn.GetPath();
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
