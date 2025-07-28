namespace __TEMPLATE__.Netcode;

public class ENetOptions
{
    public bool PrintPacketData     { get; set; } = false;
    public bool PrintPacketByteSize { get; set; } = false;
    public bool PrintPacketReceived { get; set; } = true;
    public bool PrintPacketSent     { get; set; } = true;
}
