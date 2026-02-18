using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

public class NetworkingUI : MonoBehaviour
{
    private NetworkManager nm;
    private UnityTransport utp;

    private void Awake()
    {
        nm = NetworkManager.Singleton;
        utp = nm.GetComponent<UnityTransport>();
    }

    public void ServerOnly()
    {
        utp.ConnectionData.ServerListenAddress = "0.0.0.0";
        utp.ConnectionData.Port = 6767;

        nm.StartServer();
    }

    public void ConnectClient()
    {
        utp.ConnectionData.Address = "12n.ddns.wtf";
        utp.ConnectionData.Port = 6767;

        nm.StartClient();
    }

    public void Stop()
    {
        nm.Shutdown();
    }
}
