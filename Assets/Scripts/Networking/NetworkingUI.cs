using Newtonsoft.Json.Bson;
using Unity.Netcode;
using UnityEngine;

public class NetworkingUI : MonoBehaviour
{
    public void hostServer()
    {
        GetComponent<NetworkManager>().StartServer();
    }

   public void connectClient()
    {
        GetComponent<NetworkManager>().StartClient();
    }
}
