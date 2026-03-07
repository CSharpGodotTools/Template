using ENet;
using Framework.Netcode;
using Godot;
using GodotUtils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;

namespace Framework.Netcode.Examples.Topdown;

public partial class World
{
    private sealed class WorldStressTest
    {
        private const int DefaultTargetClients = 250;
        private const float DefaultSpawnIntervalSeconds = 0.01f;
        private const float DefaultCircleRadius = 200f;
        private const float DefaultAngularSpeed = Mathf.Pi * 2f / 6f;
        private const float DefaultSendIntervalSeconds = 0.05f;
        private const ushort DefaultPort = 25565;

        private readonly World _world;
        private readonly List<SharedBotHost> _botHosts = [];
        private SharedBotHost _currentBotHost;
        private int _totalBots;
        private readonly Button _startButton;
        private readonly Button _stopButton;
        private readonly LineEdit _targetClientsInput;
        private readonly LineEdit _spawnIntervalInput;
        private readonly LineEdit _circleRadiusInput;
        private readonly LineEdit _angularSpeedInput;
        private readonly LineEdit _sendIntervalInput;
        private readonly LineEdit _portInput;
        private readonly Label _statusLabel;
        private readonly Label _activeBotsLabel;
        private readonly Label _elapsedLabel;
        private readonly Label _peersLabel;
        private readonly Label _spawnRateLabel;

        private int _targetClients = DefaultTargetClients;
        private float _spawnIntervalSeconds = DefaultSpawnIntervalSeconds;
        private float _circleRadius = DefaultCircleRadius;
        private float _angularSpeed = DefaultAngularSpeed;
        private float _sendIntervalSeconds = DefaultSendIntervalSeconds;
        private ushort _port = DefaultPort;
        private float _spawnAccumulator;
        private bool _started;
        private bool _paused;
        private bool _serverRestartPending;
        private bool _serverStartedByStressTest;
        private ushort _lastServerPort = DefaultPort;
        private int _lastServerCapacity = DefaultTargetClients;
        private float _elapsedSeconds;

        public bool IsRunning => _started;

        public WorldStressTest(World world)
        {
            _world = world;
            _startButton = _world.GetNode<Button>("%StartStressTest");
            _stopButton = _world.GetNode<Button>("%StopStressTest");
            _targetClientsInput = _world.GetNode<LineEdit>("%TargetClients");
            _spawnIntervalInput = _world.GetNode<LineEdit>("%SpawnInterval");
            _circleRadiusInput = _world.GetNode<LineEdit>("%CircleRadius");
            _angularSpeedInput = _world.GetNode<LineEdit>("%AngularSpeed");
            _sendIntervalInput = _world.GetNode<LineEdit>("%SendInterval");
            _portInput = _world.GetNode<LineEdit>("%StressPort");
            _statusLabel = _world.GetNode<Label>("%StatusLabel");
            _activeBotsLabel = _world.GetNode<Label>("%ActiveBotsLabel");
            _elapsedLabel = _world.GetNode<Label>("%ElapsedLabel");
            _peersLabel = _world.GetNode<Label>("%PeersLabel");
            _spawnRateLabel = _world.GetNode<Label>("%SpawnRateLabel");

            _targetClientsInput.TextSubmitted += OnTargetClientsChanged;

            SetUiDefaults();
            UpdateStatsUi();

            _startButton.Pressed += OnStartPressed;
            _stopButton.Pressed += OnStopPressed;
        }

        ~WorldStressTest()
        {
            _targetClientsInput.TextSubmitted -= OnTargetClientsChanged;
        }

        private void OnTargetClientsChanged(string newText)
        {
            _targetClients = ReadInt(_targetClientsInput.Text, DefaultTargetClients, minValue: 1);
        }

        public void Start()
        {
            if (_started)
                return;

            _started = true;
            _paused = false;
            _serverRestartPending = false;
            _elapsedSeconds = 0f;
            _spawnAccumulator = 0f;
            ApplySettingsFromUi();
            ApplyRunningServerSettings();
            _world.SetProcess(true);
            if (ShouldRestartServer())
            {
                RequestServerRestart();
                UpdateStatsUi();
                return;
            }

            EnsureServerRunning();
            _paused = !IsServerRunning();
            EnsureLocalClientRunning();

            if (!_paused)
                SpawnBot();

            UpdateStatsUi();
        }

        public void Tick(float deltaSeconds)
        {
            if (!_started)
                return;

            if (_serverRestartPending)
            {
                if (!IsServerRunning())
                {
                    EnsureLocalClientRunning();
                    StartServerWithSettings();
                    _serverRestartPending = false;
                    _paused = false;
                    _spawnAccumulator = 0f;
                    SpawnBot();
                }

                return;
            }

            if (!IsServerRunning())
            {
                if (!_paused)
                {
                    StopBots();
                    _paused = true;
                }

                return;
            }

            if (_paused)
            {
                _paused = false;
                _spawnAccumulator = 0f;
                SpawnBot();
            }

            _spawnAccumulator += deltaSeconds;
            while (_totalBots < _targetClients && _spawnAccumulator >= _spawnIntervalSeconds)
            {
                _spawnAccumulator -= _spawnIntervalSeconds;
                SpawnBot();
            }

            _elapsedSeconds += deltaSeconds;

            UpdateStatsUi();
        }

        public void Stop()
        {
            StopBots();
            _started = false;
            _paused = false;
            _serverRestartPending = false;
            _elapsedSeconds = 0f;
            _world.ClearRemotePlayers();
            UpdateStatsUi();
        }

        public void Dispose()
        {
            Stop();
            _startButton.Pressed -= OnStartPressed;
            _stopButton.Pressed -= OnStopPressed;
        }

        private void SpawnBot()
        {
            if (_totalBots >= _targetClients)
                return;

            if (_currentBotHost == null || _currentBotHost.IsAtCapacity)
            {
                _currentBotHost = new SharedBotHost(
                    "127.0.0.1", _port, _world.GetScreenCenter(),
                    _circleRadius, _angularSpeed, _sendIntervalSeconds);
                _botHosts.Add(_currentBotHost);
            }

            _currentBotHost.AddBot();
            _totalBots++;
        }

        private void EnsureServerRunning()
        {
            if (TryGetNet(out Net<GameClient, GameServer> net) && net.Server != null && !net.Server.IsRunning)
            {
                StartServerWithSettings();
            }
        }

        private void ApplyRunningServerSettings()
        {
            if (IsServerRunning())
            {
                if (TryGetNet(out Net<GameClient, GameServer> net))
                {
                    _port = net.ServerPort;
                }
            }
        }

        private void EnsureLocalClientRunning()
        {
            if (TryGetNet(out Net<GameClient, GameServer> net) && net.Client != null && !net.Client.IsRunning)
            {
                net.StartClient("127.0.0.1", _port);
            }
        }

        private bool IsServerRunning()
        {
            if (TryGetNet(out Net<GameClient, GameServer> net) && net.Server != null)
            {
                return net.Server.IsRunning;
            }

            return false;
        }

        private void StartServerWithSettings()
        {
            if (TryGetNet(out Net<GameClient, GameServer> net))
            {
                // +1 reserves a peer slot for the local game client.
                net.StartServer(_port, _targetClients + 1, CreateSilentOptions());
                _serverStartedByStressTest = true;
                _lastServerPort = _port;
                _lastServerCapacity = _targetClients;
            }
        }

        private bool ShouldRestartServer()
        {
            if (!TryGetNet(out Net<GameClient, GameServer> net) || net.Server == null || !net.Server.IsRunning)
                return false;

            if (!_serverStartedByStressTest)
                return false;

            if (_lastServerPort != _port)
                return true;

            if (_lastServerCapacity != _targetClients)
                return true;

            return false;
        }

        private void RequestServerRestart()
        {
            if (TryGetNet(out Net<GameClient, GameServer> net) && net.Server != null && _serverStartedByStressTest)
            {
                _serverRestartPending = true;
                _paused = true;
                StopBots();
                net.StopServer();
            }
        }

        private void StopBots()
        {
            foreach (SharedBotHost host in _botHosts)
            {
                host.Stop();
            }

            _botHosts.Clear();
            _currentBotHost = null;
            _totalBots = 0;
        }

        private void OnStartPressed()
        {
            _world.GetViewport().GuiReleaseFocus();
            Start();
        }

        private void OnStopPressed()
        {
            _world.GetViewport().GuiReleaseFocus();
            Stop();
        }

        private void SetUiDefaults()
        {
            _targetClientsInput.Text = DefaultTargetClients.ToString(CultureInfo.InvariantCulture);
            _spawnIntervalInput.Text = DefaultSpawnIntervalSeconds.ToString(CultureInfo.InvariantCulture);
            _circleRadiusInput.Text = DefaultCircleRadius.ToString(CultureInfo.InvariantCulture);
            _angularSpeedInput.Text = DefaultAngularSpeed.ToString(CultureInfo.InvariantCulture);
            _sendIntervalInput.Text = DefaultSendIntervalSeconds.ToString(CultureInfo.InvariantCulture);
            _portInput.Text = DefaultPort.ToString(CultureInfo.InvariantCulture);
        }

        private void ApplySettingsFromUi()
        {
            _targetClients = ReadInt(_targetClientsInput.Text, DefaultTargetClients, minValue: 1);
            _spawnIntervalSeconds = ReadFloat(_spawnIntervalInput.Text, DefaultSpawnIntervalSeconds, minValue: 0.01f);
            _circleRadius = ReadFloat(_circleRadiusInput.Text, DefaultCircleRadius, minValue: 0.01f);
            _angularSpeed = ReadFloat(_angularSpeedInput.Text, DefaultAngularSpeed, minValue: 0.01f);
            _sendIntervalSeconds = ReadFloat(_sendIntervalInput.Text, DefaultSendIntervalSeconds, minValue: 0.01f);
            _port = ReadUShort(_portInput.Text, DefaultPort);
        }

        private static int ReadInt(string text, int fallback, int minValue)
        {
            if (int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out int value))
                return value < minValue ? minValue : value;

            return fallback;
        }

        private static ushort ReadUShort(string text, ushort fallback)
        {
            if (ushort.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out ushort value))
                return value;

            return fallback;
        }

        private static float ReadFloat(string text, float fallback, float minValue)
        {
            if (float.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out float value))
                return value < minValue ? minValue : value;

            return fallback;
        }

        private static ENetOptions CreateSilentOptions()
        {
            return new ENetOptions
            {
                PrintPacketByteSize = false,
                PrintPacketData = false,
                PrintPacketReceived = false,
                PrintPacketSent = false
            };
        }

        private bool TryGetNet(out Net<GameClient, GameServer> net)
        {
            net = null;
            if (_world._netControlPanel != null)
            {
                net = _world._netControlPanel.Net;
            }

            return net != null;
        }

        private void UpdateStatsUi()
        {
            string status;
            if (!_started)
                status = "Idle";
            else if (_serverRestartPending)
                status = "Restarting";
            else if (_paused)
                status = "Paused";
            else
                status = "Running";

            _statusLabel.Text = status;
            _activeBotsLabel.Text = $"{_totalBots} / {_targetClients}";

            int minutes = (int)(_elapsedSeconds / 60f);
            int seconds = (int)(_elapsedSeconds % 60f);
            _elapsedLabel.Text = $"{minutes:D2}:{seconds:D2}";

            int peerCount = 0;
            if (TryGetNet(out Net<GameClient, GameServer> net) && net.Server != null)
                peerCount = net.Server.ConnectedPeerCount;
            _peersLabel.Text = peerCount.ToString(CultureInfo.InvariantCulture);

            float spawnRate = _spawnIntervalSeconds > 0f ? 1f / _spawnIntervalSeconds : 0f;
            _spawnRateLabel.Text = spawnRate.ToString("F1", CultureInfo.InvariantCulture);
        }

        private sealed class BotPeerState
        {
            public Peer Peer { get; }
            public bool SentSpawn { get; set; }
            public float Angle { get; set; }
            public float SendAccumulator { get; set; }

            public BotPeerState(Peer peer)
            {
                Peer = peer;
            }
        }

        // Manages up to MaxBotsPerHost ENet connections on a single OS thread, reducing
        // thread count from O(bots) to O(bots / MaxBotsPerHost) for drastically less
        // scheduling contention and fewer peer timeouts under high bot counts.
        private sealed class SharedBotHost
        {
            public const int MaxBotsPerHost = 100;

            // Liberal timeouts: bots are stress-test noise, not real players.
            private const uint BotTimeoutMinMs = 10_000;
            private const uint BotTimeoutMaxMs = 60_000;
            private const uint BotPingIntervalMs = 5_000;
            private const byte DefaultChannelId = 0;

            private readonly ConcurrentQueue<int> _addQueue = new();
            private readonly CancellationTokenSource _cts = new();
            private readonly string _ip;
            private readonly ushort _port;
            private readonly Vector2 _center;
            private readonly float _circleRadius;
            private readonly float _angularSpeed;
            private readonly float _sendIntervalSeconds;
            private int _botCount;

            public int BotCount => Interlocked.CompareExchange(ref _botCount, 0, 0);
            public bool IsAtCapacity => BotCount >= MaxBotsPerHost;

            public SharedBotHost(string ip, ushort port, Vector2 center, float circleRadius,
                float angularSpeed, float sendIntervalSeconds)
            {
                _ip = ip;
                _port = port;
                _center = center;
                _circleRadius = circleRadius;
                _angularSpeed = angularSpeed;
                _sendIntervalSeconds = sendIntervalSeconds;

                Task.Factory.StartNew(
                    WorkerLoop,
                    CancellationToken.None,
                    TaskCreationOptions.LongRunning,
                    TaskScheduler.Default);
            }

            /// <summary>Thread-safe. Schedules a new bot connection on the worker thread.</summary>
            public void AddBot()
            {
                _addQueue.Enqueue(0);
                Interlocked.Increment(ref _botCount);
            }

            /// <summary>Thread-safe. Signals the worker to stop all connections and exit.</summary>
            public void Stop()
            {
                _cts.Cancel();
            }

            private void WorkerLoop()
            {
                // Pre-serialize join packet once — same for every bot, never changes.
                CPacketPlayerJoinLeave joinPacket = new() { Joined = true };
                joinPacket.Write();
                byte[] joinBytes = joinPacket.GetData();

                // Reused for all position sends; safe because this loop is single-threaded.
                CPacketPlayerPosition positionPacket = new();

                Address serverAddress = new() { Port = _port };
                serverAddress.SetHost(_ip);

                using Host host = new();
                host.Create(MaxBotsPerHost, 0);

                Dictionary<uint, BotPeerState> states = [];
                long lastTicks = Stopwatch.GetTimestamp();

                while (!_cts.IsCancellationRequested)
                {
                    // Connect any pending bot-add requests.
                    while (_addQueue.TryDequeue(out _))
                    {
                        Peer peer = host.Connect(serverAddress);
                        peer.Timeout(0, BotTimeoutMinMs, BotTimeoutMaxMs);
                        peer.PingInterval(BotPingIntervalMs);
                    }

                    // Compute elapsed time since the last iteration.
                    long nowTicks = Stopwatch.GetTimestamp();
                    float delta = (float)(nowTicks - lastTicks) / Stopwatch.Frequency;
                    lastTicks = nowTicks;

                    // Tick every connected bot peer (dictionary not modified here).
                    foreach (BotPeerState state in states.Values)
                    {
                        if (!state.SentSpawn)
                        {
                            positionPacket.Position = ComputeOrbitPosition(state.Angle);
                            positionPacket.Write();
                            SendBytes(state.Peer, positionPacket.GetData());
                            state.SentSpawn = true;
                        }

                        state.Angle += _angularSpeed * delta;
                        state.SendAccumulator += delta;

                        if (state.SendAccumulator < _sendIntervalSeconds)
                            continue;

                        state.SendAccumulator = 0f;
                        positionPacket.Position = ComputeOrbitPosition(state.Angle);
                        positionPacket.Write();
                        SendBytes(state.Peer, positionPacket.GetData());
                    }

                    // Drain all queued events, then call Service once (mirrors ENetLow pattern).
                    bool serviced = false;
                    while (!serviced)
                    {
                        Event netEvent;
                        if (host.CheckEvents(out netEvent) > 0)
                        {
                            // Queued event — process without consuming the Service budget.
                        }
                        else if (host.Service(15, out netEvent) > 0)
                        {
                            serviced = true;
                        }
                        else
                        {
                            break;
                        }

                        switch (netEvent.Type)
                        {
                            case EventType.Connect:
                                states[netEvent.Peer.ID] = new BotPeerState(netEvent.Peer);
                                SendBytes(netEvent.Peer, joinBytes);
                                break;

                            case EventType.Disconnect:
                            case EventType.Timeout:
                                states.Remove(netEvent.Peer.ID);
                                break;

                            case EventType.Receive:
                                netEvent.Packet.Dispose();
                                break;
                        }
                    }
                }

                // Disconnect all remaining peers before tearing down the host.
                foreach (BotPeerState state in states.Values)
                {
                    state.Peer.DisconnectNow(0);
                }

                host.Flush();
            }

            private Vector2 ComputeOrbitPosition(float angle)
            {
                return _center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * _circleRadius;
            }

            private static void SendBytes(Peer peer, byte[] data)
            {
                Packet packet = default;
                packet.Create(data, PacketFlags.Reliable);
                peer.Send(DefaultChannelId, ref packet);
            }
        }
    }
}
