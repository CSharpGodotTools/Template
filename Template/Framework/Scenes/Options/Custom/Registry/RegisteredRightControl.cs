namespace __TEMPLATE__.Ui;

/// <summary>
/// Runtime wrapper for a right-side control definition attached to an option row.
/// </summary>
internal sealed class RegisteredRightControl
{
    private readonly int _id;
    private readonly OptionRightControlDefinition _definition;

    /// <summary>
    /// Initializes a registered right-control wrapper.
    /// </summary>
    /// <param name="id">Stable registration id.</param>
    /// <param name="definition">Source right-control definition.</param>
    public RegisteredRightControl(int id, OptionRightControlDefinition definition)
    {
        _id = id;
        _definition = definition;
    }

    /// <summary>
    /// Gets stable registration id.
    /// </summary>
    public int Id => _id;

    /// <summary>
    /// Gets source right-control definition.
    /// </summary>
    public OptionRightControlDefinition Definition => _definition;
}
