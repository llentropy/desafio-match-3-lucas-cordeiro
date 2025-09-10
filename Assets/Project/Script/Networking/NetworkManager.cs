using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using TMPro;
using Unity.Collections;
using Unity.Networking.Transport;
using UnityEngine;
using UnityEngine.SceneManagement;


namespace Gazeus.DesafioMatch3
{
    public enum ConnectionMode
    {
        Client, Server, Disconnected
    }

    //Reference for implementation:
    //https://docs.unity3d.com/Packages/com.unity.transport@2.5/manual/client-server-simple.html
    public class NetworkManager : MonoBehaviour
    {
        [SerializeField] private TMP_InputField _nameTextArea;
        [SerializeField] private TextMeshProUGUI _connectionMenuStatus;
        [SerializeField] private TMP_InputField _hostAddressTextArea;

        public string PlayerName { private set; get; }
        public string OpponentName { private set; get; }

        public ConnectionMode _connectionMode { private set; get; } = ConnectionMode.Disconnected;

        private FixedString128Bytes _queuedMessage = "";

        NetworkDriver _networkDriver;
        NetworkConnection _networkConnection;

        public List<string> GetLocalIPAddresses()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());

            List<string> localIps = new();
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    localIps.Add(ip.ToString());
                }
            }


            return localIps;
        }
        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }

        public void StartOfflineMatch()
        {
            StopNetworking();
            SceneManager.LoadScene("Gameplay");
        }

        private void SetQueuedMessage(string message)
        {
            if (_queuedMessage.IsEmpty)
            {
                _queuedMessage = message;
            }
        }

        private void EmptyQueuedMessage()
        {
            _queuedMessage = "";
        }

        private void SetupPlayerName() {
            if (_nameTextArea.text.Length == 0)
            {
                PlayerName = $"Player{UnityEngine.Random.Range(0, 100)}";
            }
            else
            {
                PlayerName = _nameTextArea.text;
            }
        }
        public void StartAsServer()
        {
            var settings = new NetworkSettings();
            settings.WithNetworkConfigParameters(
                connectTimeoutMS: 500,
                maxConnectAttempts: 10,
                disconnectTimeoutMS: 10000);

            _networkDriver = NetworkDriver.Create(settings);

            ushort connectionPort = GameConstants.DefaultConnectionPort;

            var endpoint = NetworkEndpoint.AnyIpv4.WithPort(connectionPort);


            while (!_networkDriver.Bound)
            {
                try
                {
                    _networkDriver.Bind(endpoint);
                    endpoint = NetworkEndpoint.AnyIpv4.WithPort((ushort)(connectionPort));
                } catch
                {
                    connectionPort++;
                    if (connectionPort > GameConstants.DefaultConnectionPort + 10)
                    {
                        Debug.LogError("Failed to bind to a port after 10 attempts");
                        return;
                    }
                }
                
            }
            _networkDriver.Listen();

            SetupPlayerName();

            var ipList = GetLocalIPAddresses();
            string myIps = "";

            foreach(var ip in ipList)
            {
                myIps += $"\n{ip}:{connectionPort}";
            }

            _connectionMenuStatus.text = $"Chosen name: {PlayerName}\nWaiting for a player\nLocal IP(s): {myIps}";

            _connectionMode = ConnectionMode.Server;

            Debug.Log("Started Server");
        }

        public void PopulateDefaultHostAddress()
        {
            var myIps = GetLocalIPAddresses();

            _hostAddressTextArea.text = $"{myIps[0]}:{GameConstants.DefaultConnectionPort}";
        }

        public void StartAsClient()
        {
            _networkDriver = NetworkDriver.Create();

            NetworkEndpoint endpoint;

            

            if(_hostAddressTextArea.text.Length > 0)
            {
                string[] splitAddress = _hostAddressTextArea.text.Split(":");
                var ip = splitAddress[0];
                Debug.Log(splitAddress[1]);

                int port = int.Parse(splitAddress[1]);
                

                endpoint = NetworkEndpoint.Parse(ip, (ushort) port, NetworkFamily.Ipv4);
            } else
            {
                endpoint = NetworkEndpoint.LoopbackIpv4.WithPort(GameConstants.DefaultConnectionPort);
            }

            _networkConnection = _networkDriver.Connect(endpoint);

            SetupPlayerName();
            _connectionMode = ConnectionMode.Client;

            Debug.Log("Started Client");
        }

        public void StopNetworking()
        {
            _connectionMode = ConnectionMode.Disconnected;

            _networkDriver.Dispose();

            Debug.Log("network communications stopped");
        }

        void Update()
        {
            if (!_networkDriver.IsCreated)
            {
                return;
            }
            _networkDriver.ScheduleUpdate().Complete();


            if (_connectionMode == ConnectionMode.Server && !_networkConnection.IsCreated)
            {
                NetworkConnection connection;
                while ((connection = _networkDriver.Accept()) != default)
                {
                    _networkConnection = connection;
                    Debug.Log("Accepted connection from client");
                    SetQueuedMessage($"ConnectedAs;{PlayerName}");
                }
            }

            if (!_networkConnection.IsCreated)
            {
                return;
            }

            DataStreamReader streamReader;
            NetworkEvent.Type command;
            while ((command = _networkConnection.PopEvent(_networkDriver, out streamReader)) != NetworkEvent.Type.Empty)
            {

                switch (command)
                {
                    case NetworkEvent.Type.Connect:
                        Debug.Log("Connected to server");
                        SetQueuedMessage($"ConnectedAs;{PlayerName}");
                        break;

                    case NetworkEvent.Type.Data:
                        FixedString128Bytes message = streamReader.ReadFixedString128();
                        Debug.Log($"Received message {message} from client");
                        ParseMessage(message);
                        break;

                    case NetworkEvent.Type.Disconnect:
                        Disconnect();
                        break;
                }
            }
            if (!_queuedMessage.IsEmpty)
            {
                _networkDriver.BeginSend(NetworkPipeline.Null, _networkConnection, out var writer);
                writer.WriteFixedString128(_queuedMessage);
                _networkDriver.EndSend(writer);
                EmptyQueuedMessage();
            }

        }

        private void Disconnect()
        {
            if (_networkConnection.IsCreated)
            {
                _networkConnection.Disconnect(_networkDriver);
            }
            _networkConnection = default;
            Debug.Log("Disconnected");
            SceneManager.LoadScene("MainMenu");
        }

        private void OnDestroy()
        {
            Disconnect();
            StopNetworking();
        }

        private void ParseMessage(FixedString128Bytes message)
        {
            string messageAsString = message.ToString();
            string[] messageParts = messageAsString.Split(';');

            switch (messageParts[0])
            {
                case "ConnectedAs":
                    OpponentName = messageParts[1];
                    Debug.Log(OpponentName);
                    SceneManager.LoadScene("Gameplay");
                    break;
            }
        }
    }
}
