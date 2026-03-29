namespace __TEMPLATE__.Ui;

public interface IOptionsTabRegistrar
{
    string TabName { get; }

    void Register(IOptionsService optionsService);
}
