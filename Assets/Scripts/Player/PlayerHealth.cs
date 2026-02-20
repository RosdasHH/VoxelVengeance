using TMPro;
using Unity.Netcode;
using UnityEngine;

public class PlayerHealth : NetworkBehaviour
{
    public NetworkVariable<int> health = new NetworkVariable<int>(50, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    [SerializeField]
    private TextMeshPro damageNumber;

    public override void OnNetworkSpawn()
    {
        if (IsServer) health.Value = 50;
        health.OnValueChanged += OnHealthChanged;
    }

    public override void OnNetworkDespawn()
    {
        health.OnValueChanged -= OnHealthChanged;
    }

    private void OnHealthChanged(int previousValue, int newValue)
    {
        if (!IsServer) return;
        if (newValue <= 0)
        {
            Vector3 randomSpawnPos = GameObject.FindWithTag("GameManager").GetComponent<Spawn>().getRandomSpawnPosition();
            gameObject.GetComponent<PlayerNetworkMovement>().TeleportTo(randomSpawnPos);
            health.Value = 50;
        }
    }

    public void decreaseHealth(int amount, ulong shooterId, ulong hitPlayerId)
    {
        if (!IsServer) return;
        health.Value -= amount;
        hitMessageClientRpc(amount, shooterId, hitPlayerId);
    }

    [ClientRpc]
    private void hitMessageClientRpc(int amount, ulong shooterId, ulong hitPlayerId)
    {
        if (NetworkManager.LocalClientId == shooterId)
        {
            spawnDamageNumber(amount, Color.green);
        } else if(NetworkManager.LocalClientId == hitPlayerId)
        {
            spawnDamageNumber(amount, Color.red);
        } else
        {
            spawnDamageNumber(amount, Color.blue);
        }
    }
    private void spawnDamageNumber(float amount, Color color)
    {
        TextMeshPro inst = Instantiate(damageNumber, generateRandomPositionAround(gameObject.transform.position, 0.5f), gameObject.transform.rotation);
        inst.text = amount.ToString();
        inst.color = color;
        Destroy(inst.gameObject, 1f);
    }
    private Vector3 generateRandomPositionAround(Vector3 pos, float variation)
    {
        float x = pos.x;
        float y = pos.y;
        float z = pos.z;

        float newX = Random.Range(x - variation, x + variation);
        float newY = Random.Range(y - variation, y + variation);
        float newZ = Random.Range(z - variation, z + variation);

        return new Vector3(newX, newY, newZ);
    }
}
