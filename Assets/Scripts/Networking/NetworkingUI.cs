using Newtonsoft.Json.Bson;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

public class NetworkingUI : MonoBehaviour
{
    NetworkManager networkManager;
    UnityTransport unityTransport;
    private void Start()
    {
        networkManager = GetComponent<NetworkManager>();
        unityTransport = GetComponent<UnityTransport>();
    }
    public void hostServer()
    {
        unityTransport.SetConnectionData("127.0.0.1", (ushort)6767);
        networkManager.StartServer();
    }

   public void connectClient()
    {
        GetComponent<NetworkManager>().StartClient();
    }
}
