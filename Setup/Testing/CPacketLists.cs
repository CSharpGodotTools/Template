using Framework.Netcode;
using System.Collections.Generic;

namespace Template.Setup.Testing;

public partial class CPacketLists : ClientPacket
{
    public List<int> IntValues { get; set; }
    public List<string> StringValues { get; set; }
    public List<float> FloatValues { get; set; }
}
