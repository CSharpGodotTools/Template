using Godot;
using GodotUtils;
using System;

namespace __TEMPLATE__.UI;

public class OptionsGeneral : IDisposable
{
    #region Fields
    private ResourceOptions _options;
    private Button _generalBtn;
    private readonly Options options;
    private OptionButton _languageBtn;
    #endregion

    public OptionsGeneral(Options options, Button generalBtn)
    {
        this.options = options;
        _generalBtn = generalBtn;
        _languageBtn = options.GetNode<OptionButton>("%LanguageButton");

        GetOptions();
        SetupLanguage();
    }

    private void GetOptions()
    {
        _options = Game.Options.GetOptions();
    }

    private void SetupLanguage()
    {
        _languageBtn.FocusNeighborLeft = _generalBtn.GetPath();
        _languageBtn.ItemSelected += OnLanguageItemSelected;
        _languageBtn.Select((int)_options.Language);
    }

    private void OnLanguageItemSelected(long index)
    {
        string locale = ((Language)index).ToString().Substring(0, 2).ToLower();

        TranslationServer.SetLocale(locale);

        _options.Language = (Language)index;
    }

    public void Dispose()
    {
        _languageBtn.ItemSelected -= OnLanguageItemSelected;
    }
}

public enum Language
{
    English,
    French,
    Japanese
}
