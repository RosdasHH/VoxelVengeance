using System.Text;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

public class Spawn : MonoBehaviour
{
    private UnityTransport transport;

    private void Awake()
    {
        if (NetworkManager.Singleton == null) return;
        transport = NetworkManager.Singleton.GetComponent<UnityTransport>();

        NetworkManager.Singleton.ConnectionApprovalCallback += ApprovalCheck;
        NetworkManager.Singleton.OnClientDisconnectCallback += ClientDisconnection;
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
    }
    
    private void OnDestroy()
    {
        if (NetworkManager.Singleton == null) return;
        NetworkManager.Singleton.ConnectionApprovalCallback -= ApprovalCheck;
        NetworkManager.Singleton.OnClientDisconnectCallback -= ClientDisconnection;
        NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
    }

    private void OnClientConnected(ulong clientid)
    {
        if (!NetworkManager.Singleton.IsServer) return;
        var ip = transport.ConnectionData.Address;
        Debug.Log(ip + " has connected");
    }

    private void ClientDisconnection(ulong user)
    {
        if (!NetworkManager.Singleton.IsServer) return;
        Debug.Log(user + " left the game");
    }

    private void ApprovalCheck(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
    {
        if (!NetworkManager.Singleton.IsServer) return;
        Debug.Log(request.ClientNetworkId + " is trying to join.");
        response.Approved = true;
        response.CreatePlayerObject = true;
        response.Position = getRandomSpawnPosition();
    }

    public Vector3 getRandomSpawnPosition()
    {
        int spawnPointLength = gameObject.transform.childCount;
        int randomIndex = Random.Range(0, spawnPointLength);
        Transform spawnpoint = gameObject.transform.GetChild(randomIndex);
        return spawnpoint.position;
    }
}
