using System.Diagnostics.Contracts;
using System.Text;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

public class NetworkingUI : MonoBehaviour
{
    private NetworkManager nm;
    private UnityTransport utp;
    [SerializeField] private Canvas canvas;

    private void Awake()
    {
        nm = NetworkManager.Singleton;
        utp = nm.GetComponent<UnityTransport>();
    }

    public void StartServer()
    {
        utp.ConnectionData.ServerListenAddress = "0.0.0.0";
        utp.ConnectionData.Port = 6767;

        nm.StartServer();
        disableCanvas();
    }
    public void StartHost()
    {
        utp.ConnectionData.ServerListenAddress = "0.0.0.0";
        utp.ConnectionData.Port = 6767;

        nm.StartHost();
        disableCanvas();
    }

    public void ConnectClientLocal()
    {
        utp.ConnectionData.Address = "127.0.0.1";
        utp.ConnectionData.Port = 6767;

        Connect();
    }

    public void ConnectClientExternal()
    {
        utp.ConnectionData.Address = "12n.ddns.wtf";
        utp.ConnectionData.Port = 6767;

        Connect();
    }

    private void Connect()
    {
        nm.StartClient();
        disableCanvas();
    }
    private void disableCanvas()
    {
        canvas.enabled = false;
    }

    private void OnApplicationQuit()
    {
        if (nm != null && nm.IsListening)
        {
            nm.Shutdown();
            utp.Shutdown();
        }
    }
}
