namespace __TEMPLATE__.Netcode;

public class Cmd<TOpcode>(TOpcode opcode, params object[] data)
{
    public TOpcode Opcode { get; set; } = opcode;
    public object[] Data { get; set; } = data;
}

