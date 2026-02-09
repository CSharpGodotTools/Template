using GdUnit4;
using static GdUnit4.Assertions;
using Framework.Netcode;
using Framework.Netcode.Examples.Topdown;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Framework;

namespace Template.Setup.Testing;

[TestSuite]
public class Tests
{
    [TestCase]
    public static void StringToLower()
    {
        AssertString("AbcD".ToLower()).IsEqual("abcd");
    }

    [TestCase]
    [RequireGodotRuntime]
    public static async Task ServerClientConnects()
    {
        GameServer server = new();
        GameClient client = new();

        server.Start(25565, 100, new ENetOptions());

        Task connectTask = client.Connect("127.0.0.1", 25565, new ENetOptions());

        try
        {
            bool connected = false;
            Stopwatch stopwatch = Stopwatch.StartNew();

            while (stopwatch.Elapsed < TimeSpan.FromSeconds(2))
            {
                if (client.IsConnected)
                {
                    connected = true;
                    break;
                }

                await Task.Delay(10);
            }

            AssertBool(connected).IsTrue();
        }
        finally
        {
            client.Stop();
            server.Stop();
            await connectTask;
        }
    }
}
