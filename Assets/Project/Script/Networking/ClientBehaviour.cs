using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Networking.Transport;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Gazeus.DesafioMatch3
{
    //Client based on Unity's documentation at:
    //https://docs.unity3d.com/Packages/com.unity.transport@2.5/manual/client-server-simple.html

    public class ClientBehaviour : MonoBehaviour
    {
        NetworkDriver _networkDriver;
        NetworkConnection _networkConnection;
        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }

        public void StartClient()
        {
            _networkDriver = NetworkDriver.Create();

            var endpoint = NetworkEndpoint.LoopbackIpv4.WithPort(GameConstants.DefaultConnectionPort);
            _networkConnection = _networkDriver.Connect(endpoint);

            Debug.Log("Started Client");
        }

        public void StopClient()
        {
            
            _networkDriver.Dispose();
            Debug.Log("Stopped Client");


        }

        private void OnDestroy()
        {
            StopClient();
        }

        void Update()
        {
            if (!_networkDriver.IsCreated)
            {
                return;
            }
            _networkDriver.ScheduleUpdate().Complete();
            
            DataStreamReader streamReader;
            NetworkEvent.Type command;
            while ((command = _networkConnection.PopEvent(_networkDriver, out streamReader)) != NetworkEvent.Type.Empty)
            {
                if (command == NetworkEvent.Type.Connect)
                {
                    Debug.Log("Connected to server");

                    SceneManager.LoadScene("Gameplay");
                    
                } else if (command == NetworkEvent.Type.Data)
                {
                    FixedString128Bytes value = streamReader.ReadFixedString128();
                    
                } else if(command == NetworkEvent.Type.Disconnect) {
                    Debug.Log("Client got disconnected from server");
                    _networkConnection = default;
                }
            }
        }

    }
}
