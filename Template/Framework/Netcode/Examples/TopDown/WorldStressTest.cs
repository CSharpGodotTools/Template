using ENet;
using Godot;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;

namespace __TEMPLATE__.Netcode.Examples.Topdown;

public partial class World
{
    /// <summary>
    /// UI-driven stress-test harness that spins up bot clients and tracks runtime metrics.
    /// </summary>
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
        private SharedBotHost? _currentBotHost;
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

            _targetClientsInput.TextSubmitted += OnTargetClientsChanged;

            SetUiDefaults();
            UpdateStatsUi();

            _startButton.Pressed += OnStartPressed;
            _stopButton.Pressed += OnStopPressed;
        }

        /// <summary>
        /// Handles target-client count edits from the UI.
        /// </summary>
        /// <param name="newText">User-entered target client count text.</param>
        private void OnTargetClientsChanged(string newText)
        {
            _targetClients = ReadInt(_targetClientsInput.Text, DefaultTargetClients, minValue: 1);
        }

        /// <summary>
        /// Starts stress test orchestration and ensures server/client prerequisites are met.
        /// </summary>
        public void Start()
        {
            // Ignore repeated starts while the stress test is already active.
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

            // Recreate server when runtime settings no longer match stress settings.
            if (ShouldRestartServer())
            {
                RequestServerRestart();
                UpdateStatsUi();
                return;
            }

            EnsureServerRunning();
            _paused = !IsServerRunning();
            EnsureLocalClientRunning();

            // Spawn immediately so the loop starts with at least one active bot.
            if (!_paused)
                SpawnBot();

            UpdateStatsUi();
        }

        /// <summary>
        /// Advances stress test lifecycle each frame.
        /// </summary>
        /// <param name="deltaSeconds">Frame delta in seconds.</param>
        public void Tick(float deltaSeconds)
        {
            // Keep labels current even when the stress test is stopped.
            if (!_started)
            {
                // Refresh peer count when the server keeps running outside this harness.
                if (IsServerRunning())
                    UpdateStatsUi();

                return;
            }

            // Wait for server shutdown before starting a clean restart.
            if (_serverRestartPending)
            {
                // Resume once the old server is fully down.
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

            // Pause spawning while server is unavailable.
            if (!IsServerRunning())
            {
                // Stop all bots once when transitioning into paused mode.
                if (!_paused)
                {
                    StopBots();
                    _paused = true;
                }

                return;
            }

            // Restart spawn flow once server comes back from a paused state.
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

        /// <summary>
        /// Stops stress test, disconnects bots, and resets world/state UI.
        /// </summary>
        public void Stop()
        {
            StopBots();
            _started = false;
            _paused = false;
            _serverRestartPending = false;
            _elapsedSeconds = 0f;
            _world.ClearRemotePlayers();

            // Force-disconnect all peers from the server side so ConnectedPeerCount
            // resets immediately rather than draining slowly via bot-side disconnects.
            if (TryGetNet(out Net<GameClient, GameServer>? net) && net.Server != null)
                net.Server.KickAll(DisconnectOpcode.Stopping);

            _world.SetProcess(true);
            UpdateStatsUi();
        }

        /// <summary>
        /// Disposes stress-test resources and UI/event bindings.
        /// </summary>
        public void Dispose()
        {
            Stop();
            _targetClientsInput.TextSubmitted -= OnTargetClientsChanged;
            _startButton.Pressed -= OnStartPressed;
            _stopButton.Pressed -= OnStopPressed;
        }

        /// <summary>
        /// Spawns one bot connection via shared host infrastructure.
        /// </summary>
        private void SpawnBot()
        {
            // Respect requested client cap.
            if (_totalBots >= _targetClients)
                return;

            // Allocate another shared host when the current one is unavailable or full.
            if (_currentBotHost?.IsAtCapacity != false)
            {
                _currentBotHost = new SharedBotHost(
                    "127.0.0.1", _port, _world.GetScreenCenter(),
                    _circleRadius, _angularSpeed, _sendIntervalSeconds);
                _botHosts.Add(_currentBotHost);
            }

            _currentBotHost.AddBot();
            _totalBots++;
        }

        /// <summary>
        /// Starts server if not running and net coordinator is available.
        /// </summary>
        private void EnsureServerRunning()
        {
            // Start the managed server lazily when net is ready but not running.
            if (TryGetNet(out Net<GameClient, GameServer>? net) && net.Server?.IsRunning == false)
                StartServerWithSettings();
        }

        /// <summary>
        /// Syncs stress-test port with currently running server, when present.
        /// </summary>
        private void ApplyRunningServerSettings()
        {
            // Mirror the running server port into the stress test controls.
            if (IsServerRunning())
            {
                // Read from net only when coordinator is currently available.
                if (TryGetNet(out Net<GameClient, GameServer>? net))
                    _port = net.ServerPort;
            }
        }

        /// <summary>
        /// Starts local client when server is running but client is not.
        /// </summary>
        private void EnsureLocalClientRunning()
        {
            // Keep one local client connected so world state can be observed.
            if (TryGetNet(out Net<GameClient, GameServer>? net) && net.Client?.IsRunning == false)
                net.StartClient("127.0.0.1", _port);
        }

        /// <summary>
        /// Returns whether the managed server instance is currently running.
        /// </summary>
        /// <returns><see langword="true"/> when server is active.</returns>
        private bool IsServerRunning()
        {
            // Check server state only when coordinator and server are available.
            if (TryGetNet(out Net<GameClient, GameServer>? net) && net.Server != null)
                return net.Server.IsRunning;

            return false;
        }

        /// <summary>
        /// Starts server using current stress-test settings and tracks ownership metadata.
        /// </summary>
        private void StartServerWithSettings()
        {
            // Guard startup until net coordinator is resolved.
            if (TryGetNet(out Net<GameClient, GameServer>? net))
            {
                // +1 reserves a peer slot for the local game client.
                net.StartServer(_port, _targetClients + 1, CreateSilentOptions());
                _serverStartedByStressTest = true;
                _lastServerPort = _port;
                _lastServerCapacity = _targetClients;
            }
        }

        /// <summary>
        /// Determines whether running server settings differ from current stress-test settings.
        /// </summary>
        /// <returns><see langword="true"/> when restart is needed.</returns>
        private bool ShouldRestartServer()
        {
            // No restart needed when server is not active.
            if (!TryGetNet(out Net<GameClient, GameServer>? net) || net.Server?.IsRunning != true)
                return false;

            // Do not restart servers this stress test did not start.
            if (!_serverStartedByStressTest)
                return false;

            // Restart when selected port changed.
            if (_lastServerPort != _port)
                return true;

            // Restart when target capacity changed.
            if (_lastServerCapacity != _targetClients)
                return true;

            return false;
        }

        /// <summary>
        /// Requests a managed server restart sequence controlled by stress-test logic.
        /// </summary>
        private void RequestServerRestart()
        {
            // Stop current managed server and switch into restart-pending state.
            if (TryGetNet(out Net<GameClient, GameServer>? net) && net.Server != null && _serverStartedByStressTest)
            {
                _serverRestartPending = true;
                _paused = true;
                StopBots();
                net.StopServer();
            }
        }

        /// <summary>
        /// Stops and clears all shared bot hosts.
        /// </summary>
        private void StopBots()
        {
            foreach (SharedBotHost host in _botHosts)
                host.Stop();

            _botHosts.Clear();
            _currentBotHost = null;
            _totalBots = 0;
        }

        /// <summary>
        /// Handles start button press.
        /// </summary>
        private void OnStartPressed()
        {
            _world.GetViewport().GuiReleaseFocus();
            Start();
        }

        /// <summary>
        /// Handles stop button press.
        /// </summary>
        private void OnStopPressed()
        {
            _world.GetViewport().GuiReleaseFocus();
            Stop();
        }

        /// <summary>
        /// Initializes stress-test UI inputs with default values.
        /// </summary>
        private void SetUiDefaults()
        {
            _targetClientsInput.Text = DefaultTargetClients.ToString(CultureInfo.InvariantCulture);
            _spawnIntervalInput.Text = DefaultSpawnIntervalSeconds.ToString(CultureInfo.InvariantCulture);
            _circleRadiusInput.Text = DefaultCircleRadius.ToString(CultureInfo.InvariantCulture);
            _angularSpeedInput.Text = DefaultAngularSpeed.ToString(CultureInfo.InvariantCulture);
            _sendIntervalInput.Text = DefaultSendIntervalSeconds.ToString(CultureInfo.InvariantCulture);
            _portInput.Text = DefaultPort.ToString(CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Reads and applies stress-test settings from UI controls.
        /// </summary>
        private void ApplySettingsFromUi()
        {
            _targetClients = ReadInt(_targetClientsInput.Text, DefaultTargetClients, minValue: 1);
            _spawnIntervalSeconds = ReadFloat(_spawnIntervalInput.Text, DefaultSpawnIntervalSeconds, minValue: 0.01f);
            _circleRadius = ReadFloat(_circleRadiusInput.Text, DefaultCircleRadius, minValue: 0.01f);
            _angularSpeed = ReadFloat(_angularSpeedInput.Text, DefaultAngularSpeed, minValue: 0.01f);
            _sendIntervalSeconds = ReadFloat(_sendIntervalInput.Text, DefaultSendIntervalSeconds, minValue: 0.01f);
            _port = ReadUShort(_portInput.Text, DefaultPort);
        }

        /// <summary>
        /// Parses a bounded integer with fallback.
        /// </summary>
        /// <param name="text">Raw text input.</param>
        /// <param name="fallback">Fallback value when parse fails.</param>
        /// <param name="minValue">Minimum accepted value.</param>
        /// <returns>Parsed and clamped integer value.</returns>
        private static int ReadInt(string text, int fallback, int minValue)
        {
            // Accept user value when parse succeeds; otherwise use fallback.
            if (int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out int value))
                return value < minValue ? minValue : value;

            return fallback;
        }

        /// <summary>
        /// Parses an unsigned short with fallback.
        /// </summary>
        /// <param name="text">Raw text input.</param>
        /// <param name="fallback">Fallback value when parse fails.</param>
        /// <returns>Parsed value or fallback.</returns>
        private static ushort ReadUShort(string text, ushort fallback)
        {
            // Accept valid unsigned-port text and fall back when invalid.
            if (ushort.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out ushort value))
                return value;

            return fallback;
        }

        /// <summary>
        /// Parses a bounded float with fallback.
        /// </summary>
        /// <param name="text">Raw text input.</param>
        /// <param name="fallback">Fallback value when parse fails.</param>
        /// <param name="minValue">Minimum accepted value.</param>
        /// <returns>Parsed and clamped float value.</returns>
        private static float ReadFloat(string text, float fallback, float minValue)
        {
            // Accept user value when parse succeeds; otherwise use fallback.
            if (float.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out float value))
                return value < minValue ? minValue : value;

            return fallback;
        }

        /// <summary>
        /// Creates low-noise ENet options for stress-test server/client traffic.
        /// </summary>
        /// <returns>Silent ENet options preset.</returns>
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

        /// <summary>
        /// Attempts to resolve active net coordinator from the world panel.
        /// </summary>
        /// <param name="net">Resolved net coordinator when available.</param>
        /// <returns><see langword="true"/> when coordinator is available.</returns>
        private bool TryGetNet([NotNullWhen(true)] out Net<GameClient, GameServer>? net)
        {
            net = null;

            // Resolve net coordinator only when the control panel has been initialized.
            if (_world._netControlPanel != null)
                net = _world._netControlPanel.Net;

            return net != null;
        }

        /// <summary>
        /// Refreshes status and counters displayed in the stress-test panel.
        /// </summary>
        private void UpdateStatsUi()
        {
            string status;

            // Show top-level stress state in priority order.
            if (!_started)
                status = "Idle";
            // Show restarting while waiting for the server restart sequence.
            else if (_serverRestartPending)
                status = "Restarting";
            // Show paused while waiting for server availability.
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

            // Read connected-peer count only when the server is present.
            if (TryGetNet(out Net<GameClient, GameServer>? net) && net.Server != null)
                peerCount = net.Server.ConnectedPeerCount;
            _peersLabel.Text = peerCount.ToString(CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Worker-thread state for a connected stress-test bot peer.
        /// </summary>
        private sealed class BotPeerState
        {
            /// <summary>
            /// Gets peer transport handle.
            /// </summary>
            public Peer Peer { get; }

            /// <summary>
            /// Gets or sets whether initial spawn position was sent.
            /// </summary>
            public bool SentSpawn { get; set; }

            /// <summary>
            /// Gets or sets current orbit angle in radians.
            /// </summary>
            public float Angle { get; set; }

            /// <summary>
            /// Gets or sets send interval accumulator in seconds.
            /// </summary>
            public float SendAccumulator { get; set; }

            /// <summary>
            /// Creates bot-peer state for a connected ENet peer.
            /// </summary>
            /// <param name="peer">Connected peer handle.</param>
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

            /// <summary>
            /// Creates a shared bot host worker.
            /// </summary>
            /// <param name="ip">Server IP.</param>
            /// <param name="port">Server port.</param>
            /// <param name="center">Orbit center.</param>
            /// <param name="circleRadius">Orbit radius.</param>
            /// <param name="angularSpeed">Orbit angular speed.</param>
            /// <param name="sendIntervalSeconds">Position send interval.</param>
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

            /// <summary>Schedules a new bot connection on the worker thread.</summary>
            public void AddBot()
            {
                _addQueue.Enqueue(0);
                Interlocked.Increment(ref _botCount);
            }

            /// <summary>Signals the worker to stop all connections and exit.</summary>
            public void Stop()
            {
                _cts.Cancel();
            }

            /// <summary>
            /// Runs shared bot host loop: connect bots, process events, and send movement updates.
            /// </summary>
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
                        // Send first position packet immediately after the bot connects.
                        if (!state.SentSpawn)
                        {
                            positionPacket.Position = ComputeOrbitPosition(state.Angle);
                            positionPacket.Write();
                            SendBytes(state.Peer, positionPacket.GetData());
                            state.SentSpawn = true;
                        }

                        state.Angle += _angularSpeed * delta;
                        state.SendAccumulator += delta;

                        // Skip send until the configured interval has elapsed.
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
                        // Consume queued ENet events first without blocking.
                        if (host.CheckEvents(out Event netEvent) > 0)
                        {
                            // Queued event — process without consuming the Service budget.
                        }

                        // Fall back to timed service polling when queue is empty.
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
                    state.Peer.DisconnectNow(0);

                host.Flush();
            }

            /// <summary>
            /// Computes orbit position for a bot at the specified angle.
            /// </summary>
            /// <param name="angle">Orbit angle in radians.</param>
            /// <returns>Orbit position in world coordinates.</returns>
            private Vector2 ComputeOrbitPosition(float angle)
            {
                return _center + (new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * _circleRadius);
            }

            /// <summary>
            /// Sends raw payload bytes through a peer as a reliable ENet packet.
            /// </summary>
            /// <param name="peer">Target peer.</param>
            /// <param name="data">Serialized payload bytes.</param>
            private static void SendBytes(Peer peer, byte[] data)
            {
                Packet packet = default;
                packet.Create(data, PacketFlags.Reliable);
                peer.Send(DefaultChannelId, ref packet);
            }
        }
    }
}
