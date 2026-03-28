using Godot;
using System;

namespace __TEMPLATE__.Ui;

public class OptionsGeneral : IDisposable
{
    // Fields
    private readonly OptionsManager _optionsManager;
    private readonly Button _generalBtn;
    private readonly OptionButton _languageBtn;

    public OptionsGeneral(Options options, Button generalBtn, OptionsManager optionsManager)
    {
        _generalBtn = generalBtn;
        _languageBtn = options.GetNode<OptionButton>("%LanguageButton");
        _optionsManager = optionsManager;

        SetupLanguage();
    }

    private void SetupLanguage()
    {
        _languageBtn.FocusNeighborLeft = _generalBtn.GetPath();
        _languageBtn.ItemSelected += OnLanguageItemSelected;
        _languageBtn.Select((int)_optionsManager.Settings.Language);
    }

    private void OnLanguageItemSelected(long index)
    {
        _optionsManager.SetLanguage((Language)index);
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
