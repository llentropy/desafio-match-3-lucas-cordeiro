using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Collections;
using Unity.Networking.Transport;
using UnityEngine;
using UnityEngine.SceneManagement;


namespace Gazeus.DesafioMatch3
{
    enum ConnectionMode
    {
        Client, Server, Disconnected
    }

    //Reference for implementation:
    //https://docs.unity3d.com/Packages/com.unity.transport@2.5/manual/client-server-simple.html
    public class NetworkManager : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _nameTextArea;
        [SerializeField] private TextMeshProUGUI _connectionMenuStatus;

        public string PlayerName { private set; get; }
        public string OpponentName { private set; get; }

        private ConnectionMode _connectionMode = ConnectionMode.Disconnected;

        private FixedString128Bytes _queuedMessage = "";

        NetworkDriver _networkDriver;
        NetworkConnection _networkConnection;
        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
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
                PlayerName = $"Player{Random.Range(0, 100)}";
            }
            else
            {
                PlayerName = _nameTextArea.text;
            }
        }
        public void StartAsServer()
        {
            _networkDriver = NetworkDriver.Create();

            var endpoint = NetworkEndpoint.AnyIpv4.WithPort(GameConstants.DefaultConnectionPort);
            if (_networkDriver.Bind(endpoint) != 0)
            {
                Debug.LogError($"Failed to bind to port {GameConstants.DefaultConnectionPort}");
                return;
            }
            _networkDriver.Listen();

            SetupPlayerName();

            _connectionMenuStatus.text = $"Waiting for a player\nChosen name: {PlayerName}";

            _connectionMode = ConnectionMode.Server;

            Debug.Log("Started Server");
        }

        public void StartAsClient()
        {
            _networkDriver = NetworkDriver.Create();

            var endpoint = NetworkEndpoint.LoopbackIpv4.WithPort(GameConstants.DefaultConnectionPort);
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
                        Debug.Log("Disconnected");
                        _networkConnection = default;
                        SceneManager.LoadScene("ModeSelect");
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

        private void OnDestroy()
        {
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
