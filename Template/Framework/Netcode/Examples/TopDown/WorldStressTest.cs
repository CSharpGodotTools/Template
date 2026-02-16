using Framework.Netcode;
using Godot;
using GodotUtils;
using System.Collections.Generic;
using System.Globalization;
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
        private const int DefaultMaxClients = 500;

        private readonly World _world;
        private readonly List<BotClient> _bots = [];
        private readonly Button _startButton;
        private readonly Button _stopButton;
        private readonly LineEdit _targetClientsInput;
        private readonly LineEdit _spawnIntervalInput;
        private readonly LineEdit _circleRadiusInput;
        private readonly LineEdit _angularSpeedInput;
        private readonly LineEdit _sendIntervalInput;
        private readonly LineEdit _portInput;
        private readonly LineEdit _maxClientsInput;

        private int _targetClients = DefaultTargetClients;
        private float _spawnIntervalSeconds = DefaultSpawnIntervalSeconds;
        private float _circleRadius = DefaultCircleRadius;
        private float _angularSpeed = DefaultAngularSpeed;
        private float _sendIntervalSeconds = DefaultSendIntervalSeconds;
        private ushort _port = DefaultPort;
        private int _maxClients = DefaultMaxClients;
        private float _spawnAccumulator;
        private bool _started;
        private bool _paused;
        private bool _serverRestartPending;
        private bool _serverStartedByStressTest;
        private ushort _lastServerPort = DefaultPort;
        private int _lastServerMaxClients = DefaultMaxClients;

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
            _maxClientsInput = _world.GetNode<LineEdit>("%StressMaxClients");

            SetUiDefaults();

            _startButton.Pressed += OnStartPressed;
            _stopButton.Pressed += OnStopPressed;
        }

        public void Start()
        {
            if (_started)
                return;

            _started = true;
            _paused = false;
            _serverRestartPending = false;
            _spawnAccumulator = 0f;
            ApplySettingsFromUi();
            ApplyRunningServerSettings();
            _world.SetProcess(true);
            if (ShouldRestartServer())
            {
                RequestServerRestart();
                return;
            }

            EnsureServerRunning();
            _paused = !IsServerRunning();
            EnsureLocalClientRunning();

            if (!_paused)
                SpawnBot();
        }

        public void Tick(float deltaSeconds)
        {
            if (!_started)
                return;

            if (_serverRestartPending)
            {
                if (!IsServerRunning())
                {
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
            while (_bots.Count < _targetClients && _spawnAccumulator >= _spawnIntervalSeconds)
            {
                _spawnAccumulator -= _spawnIntervalSeconds;
                SpawnBot();
            }

            foreach (BotClient bot in _bots)
            {
                bot.Tick(deltaSeconds);
            }
        }

        public void Stop()
        {
            StopBots();
            _started = false;
            _paused = false;
            _serverRestartPending = false;
            _world.ClearRemotePlayers();
        }

        public void Dispose()
        {
            Stop();
            _startButton.Pressed -= OnStartPressed;
            _stopButton.Pressed -= OnStopPressed;
        }

        private void SpawnBot()
        {
            if (_bots.Count >= _targetClients)
                return;

            BotClient bot = new(_world.GetScreenCenter(), _circleRadius, _angularSpeed, _sendIntervalSeconds, _port);
            _bots.Add(bot);
        }

        private void EnsureServerRunning()
        {
            if (TryGetNet(out Net net) && net.Server != null && !net.Server.IsRunning)
            {
                StartServerWithSettings();
            }
        }

        private void ApplyRunningServerSettings()
        {
            if (IsServerRunning())
            {
                if (TryGetNet(out Net net))
                {
                    _port = net.ServerPort;
                    _maxClients = net.ServerMaxClients;
                }

                if (_targetClients > _maxClients)
                    _targetClients = _maxClients;
            }
        }

        private void EnsureLocalClientRunning()
        {
            if (TryGetNet(out Net net) && net.Client != null && !net.Client.IsRunning)
            {
                Task startTask = net.StartClient("127.0.0.1", _port);
                _ = startTask.ContinueWith(
                    t => GameFramework.Logger.LogErr(t.Exception, "WorldStressTest"),
                    TaskContinuationOptions.OnlyOnFaulted);
            }
        }

        private bool IsServerRunning()
        {
            if (TryGetNet(out Net net) && net.Server != null)
            {
                return net.Server.IsRunning;
            }

            return false;
        }

        private void StartServerWithSettings()
        {
            if (TryGetNet(out Net net))
            {
                net.StartServer(_port, _maxClients, CreateSilentOptions());
                _serverStartedByStressTest = true;
                _lastServerPort = _port;
                _lastServerMaxClients = _maxClients;
            }
        }

        private bool ShouldRestartServer()
        {
            if (!TryGetNet(out Net net) || net.Server == null || !net.Server.IsRunning)
                return false;

            if (!_serverStartedByStressTest)
                return false;

            if (_lastServerPort != _port || _lastServerMaxClients != _maxClients)
                return true;

            return false;
        }

        private void RequestServerRestart()
        {
            if (TryGetNet(out Net net) && net.Server != null && _serverStartedByStressTest)
            {
                _serverRestartPending = true;
                _paused = true;
                StopBots();
                net.StopServer();
            }
        }

        private void StopBots()
        {
            foreach (BotClient bot in _bots)
            {
                bot.Stop();
            }

            _bots.Clear();
        }

        private void OnStartPressed()
        {
            _world.GetTree().UnfocusCurrentControl();
            Start();
        }

        private void OnStopPressed()
        {
            _world.GetTree().UnfocusCurrentControl();
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
            _maxClientsInput.Text = DefaultMaxClients.ToString(CultureInfo.InvariantCulture);
        }

        private void ApplySettingsFromUi()
        {
            _targetClients = ReadInt(_targetClientsInput.Text, DefaultTargetClients, minValue: 1);
            _spawnIntervalSeconds = ReadFloat(_spawnIntervalInput.Text, DefaultSpawnIntervalSeconds, minValue: 0.01f);
            _circleRadius = ReadFloat(_circleRadiusInput.Text, DefaultCircleRadius, minValue: 0.01f);
            _angularSpeed = ReadFloat(_angularSpeedInput.Text, DefaultAngularSpeed, minValue: 0.01f);
            _sendIntervalSeconds = ReadFloat(_sendIntervalInput.Text, DefaultSendIntervalSeconds, minValue: 0.01f);
            _port = ReadUShort(_portInput.Text, DefaultPort);
            _maxClients = ReadInt(_maxClientsInput.Text, DefaultMaxClients, minValue: 1);
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

        private bool TryGetNet(out Net net)
        {
            net = null;
            if (_world._netControlPanel != null)
            {
                net = _world._netControlPanel.Net;
            }

            return net != null;
        }

        private sealed class BotClient
        {
            private readonly GameClient _client;
            private readonly Vector2 _center;
            private readonly float _circleRadius;
            private readonly float _angularSpeed;
            private readonly float _sendIntervalSeconds;
            private float _angle;
            private float _sendAccumulator;
            private bool _sentSpawn;

            public BotClient(Vector2 center, float circleRadius, float angularSpeed, float sendIntervalSeconds, ushort port)
            {
                _center = center;
                _circleRadius = circleRadius;
                _angularSpeed = angularSpeed;
                _sendIntervalSeconds = sendIntervalSeconds;
                _client = new GameClient();

                Task connectTask = _client.Connect("127.0.0.1", port, CreateSilentOptions());
                _ = connectTask.ContinueWith(
                    t => GameFramework.Logger.LogErr(t.Exception, "WorldStressTest"),
                    TaskContinuationOptions.OnlyOnFaulted);
            }

            public void Tick(float deltaSeconds)
            {
                _client.HandlePackets();

                if (!_client.IsConnected)
                    return;

                if (!_sentSpawn)
                {
                    _client.SendPosition(_center);
                    _sentSpawn = true;
                }

                _angle += _angularSpeed * deltaSeconds;
                _sendAccumulator += deltaSeconds;

                if (_sendAccumulator < _sendIntervalSeconds)
                    return;

                _sendAccumulator = 0f;
                Vector2 position = _center + new Vector2(Mathf.Cos(_angle), Mathf.Sin(_angle)) * _circleRadius;
                _client.SendPosition(position);
            }

            public void Stop()
            {
                if (_client.IsRunning)
                    _client.Stop();
            }
        }
    }
}
