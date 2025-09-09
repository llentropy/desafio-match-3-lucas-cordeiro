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

    //Server based on Unity's documentation at:
    //https://docs.unity3d.com/Packages/com.unity.transport@2.5/manual/client-server-simple.html
    public class ServerBehaviour : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _nameTextArea;
        [SerializeField] private TextMeshProUGUI _connectionMenuStatus;

        private ConnectionMode _connectionMode = ConnectionMode.Disconnected;

        NetworkDriver _networkDriver;
        NativeList<NetworkConnection> _networkConnections;
        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }

        public void StartServer()
        {
            _networkDriver = NetworkDriver.Create();
            _networkConnections = new NativeList<NetworkConnection>(16, Allocator.Persistent);

            var endpoint = NetworkEndpoint.AnyIpv4.WithPort(GameConstants.DefaultConnectionPort);
            if (_networkDriver.Bind(endpoint) != 0)
            {
                Debug.LogError($"Failed to bind to port {GameConstants.DefaultConnectionPort}");
                return;
            }
            _networkDriver.Listen();

            _connectionMenuStatus.text = $"Waiting for a player\nChosen name: {_nameTextArea.text}";
            Debug.Log("Started Server");
        }

        public void StopServer()
        {
            if (_networkDriver.IsCreated)
            {
                _networkDriver.Dispose();
                _networkConnections.Dispose();
            }
            Debug.Log("Stopped Server");

        }

        void Update()
        {
            if(!_networkDriver.IsCreated)
            {
                return;
            }
            _networkDriver.ScheduleUpdate().Complete();

            for(int i = 0; i < _networkConnections.Length; i++)
            {
                if (!_networkConnections[i].IsCreated)
                {
                    _networkConnections.RemoveAtSwapBack(i);
                    i--;
                }
            }

            NetworkConnection connection;
            while((connection = _networkDriver.Accept()) != default)
            {
                _networkConnections.Add(connection);
                Debug.Log("Accepted connection");
            }

            for(int i = 0; i < _networkConnections.Length; i++)
            {
                DataStreamReader streamReader;
                NetworkEvent.Type command;
                while((command = _networkDriver.PopEventForConnection(_networkConnections[i], out streamReader)) != NetworkEvent.Type.Empty)
                {
                    if (command == NetworkEvent.Type.Connect)
                    {
                        Debug.Log("Connected to client");

                        SceneManager.LoadScene("Gameplay");

                    }else if (command == NetworkEvent.Type.Data)
                    {
                        uint number = streamReader.ReadUInt();
                        Debug.Log($"Received number {number} from a client. We will multiply it by 2");
                        number *= 2;

                        _networkDriver.BeginSend(NetworkPipeline.Null, _networkConnections[i], out var writer);
                        writer.WriteUInt(number);
                        _networkDriver.EndSend(writer);

                    } else if (command == NetworkEvent.Type.Disconnect)
                    {
                        Debug.Log("Client disconnected from the server");
                        _networkConnections[i] = default;
                        break;
                    }
                }
            }
        }

        private void OnDestroy()
        {
            StopServer();
        }
    }
}
