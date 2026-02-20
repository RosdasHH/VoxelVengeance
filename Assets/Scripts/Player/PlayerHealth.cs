using Unity.Netcode;
using UnityEngine;

public class PlayerHealth : NetworkBehaviour
{
    public NetworkVariable<int> health = new NetworkVariable<int>(50, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    public override void OnNetworkSpawn()
    {
        if(IsServer) health.Value = 50;
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

    public void decreaseHealth(int amount)
    {
        health.Value -= amount;
    }
}
