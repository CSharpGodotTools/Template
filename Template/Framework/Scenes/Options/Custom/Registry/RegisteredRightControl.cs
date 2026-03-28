namespace __TEMPLATE__.Ui;

/// <summary>
/// Runtime wrapper for a right-side control definition attached to an option row.
/// </summary>
internal sealed class RegisteredRightControl
{
    private readonly int _id;
    private readonly OptionRightControlDefinition _definition;

    public RegisteredRightControl(int id, OptionRightControlDefinition definition)
    {
        _id = id;
        _definition = definition;
    }

    public int Id => _id;
    public OptionRightControlDefinition Definition => _definition;
}
