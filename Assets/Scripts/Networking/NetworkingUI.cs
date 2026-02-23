using System.Diagnostics.Contracts;
using System.Text;
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

    private void Start()
    {
        GameObject GameManager = GameObject.FindGameObjectWithTag("GameManager");
        MainMenuManager MainMenuManager = GameManager.GetComponent<MainMenuManager>();
        if (MainMenuManager.networkStart == "JoinServer")
        {
            ConnectClientExternal();
        } else 
        if (MainMenuManager.networkStart == "StartServer")
        {
            StartServer();
        } else 
        if (MainMenuManager.networkStart == "Host")
        {
            StartHost();
        } else 
        if (MainMenuManager.networkStart == "JoinLocal")
        {
            ConnectClientLocal();
        }
    }

    public void StartServer()
    {
        utp.ConnectionData.ServerListenAddress = "0.0.0.0";
        utp.ConnectionData.Port = 6767;

        nm.StartServer();
    }
    public void StartHost()
    {
        utp.ConnectionData.ServerListenAddress = "0.0.0.0";
        utp.ConnectionData.Port = 6767;

        nm.StartHost();
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
