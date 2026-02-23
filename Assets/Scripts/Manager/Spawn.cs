using System.Text;
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
        int spawnPointLength = gameObject.transform.childCount;
        int randomIndex = Random.Range(0, spawnPointLength);
        Transform spawnpoint = gameObject.transform.GetChild(randomIndex);
        return spawnpoint.position;
    }
}
