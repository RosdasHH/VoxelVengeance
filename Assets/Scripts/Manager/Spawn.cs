using Unity.Netcode;
using UnityEngine;

public class Spawn : MonoBehaviour
{
    private void Start()
    {
        NetworkManager.Singleton.ConnectionApprovalCallback += ApprovalCheck;
    }

    private void ApprovalCheck(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
    {
        response.Approved = true;
        response.CreatePlayerObject = true;
        response.Position = getRandomSpawnPosition();
    }
    public Vector3 getRandomSpawnPosition()
    {
        Transform spawnPoints = transform.Find("SpawnPoints");
        int spawnPointLength = spawnPoints.childCount;
        int randomIndex = Random.Range(1, spawnPointLength);
        Transform spawnpoint = spawnPoints.GetChild(randomIndex);
        return spawnpoint.position;
    }
}
