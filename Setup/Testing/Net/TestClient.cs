using Framework.Netcode.Client;

namespace Template.Setup.Testing;

public sealed class TestClient : GodotClient
{
    public int OutgoingCount => Outgoing.Count;
    public int GodotPacketCount => GodotPackets.Count;
    public int CommandCount => ENetCmds.Count;

    public uint PeerId => _peer.ID;
}
