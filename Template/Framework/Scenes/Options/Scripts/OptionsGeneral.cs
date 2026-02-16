using Godot;
using GodotUtils;
using System;

namespace Framework.UI;

public class OptionsGeneral : IDisposable
{
    // Fields
    private ResourceOptions _resourceOptions;
    private readonly Button _generalBtn;
    private readonly Options _options;
    private readonly OptionButton _languageBtn;

    public OptionsGeneral(Options options, Button generalBtn)
    {
        _options = options;
        _generalBtn = generalBtn;
        _languageBtn = options.GetNode<OptionButton>("%LanguageButton");

        GetOptions();
        SetupLanguage();
    }

    private void GetOptions()
    {
        _resourceOptions = GameFramework.Options.GetOptions();
    }

    private void SetupLanguage()
    {
        _languageBtn.FocusNeighborLeft = _generalBtn.GetPath();
        _languageBtn.ItemSelected += OnLanguageItemSelected;
        _languageBtn.Select((int)_resourceOptions.Language);
    }

    private void OnLanguageItemSelected(long index)
    {
        string locale = ((Language)index).ToString()[..2].ToLower();

        TranslationServer.SetLocale(locale);

        _resourceOptions.Language = (Language)index;
    }

    public void Dispose()
    {
        _languageBtn.ItemSelected -= OnLanguageItemSelected;
        GC.SuppressFinalize(this);
    }
}

public enum Language
{
    English,
    French,
    Japanese
}
