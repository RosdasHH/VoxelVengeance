using TMPro;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerHealth : NetworkBehaviour
{
    public NetworkVariable<int> health = new NetworkVariable<int>(50, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    [SerializeField]
    private TextMeshPro damageNumber;

    public override void OnNetworkSpawn()
    {
        if (IsServer) health.Value = 50;
    }

    public void decreaseHealth(int amount, ulong shooterId, ulong hitPlayerId)
    {
        if (!IsServer) return;
        health.Value -= amount;
        hitMessageClientRpc(amount, shooterId, hitPlayerId, health.Value);
        if (health.Value <= 0)
        {
            Vector3 randomSpawnPos = GameObject.FindWithTag("GameManager").GetComponent<Spawn>().getRandomSpawnPosition();
            gameObject.GetComponent<PlayerNetworkMovement>().TeleportTo(randomSpawnPos);
            health.Value = 50;
            sendKillMessageClientRpc(shooterId, hitPlayerId);
        }
    }

    [ClientRpc]
    private void sendKillMessageClientRpc(ulong shooterId, ulong victimId)
    {
        GetComponent<AudioPlayer>().Play("kill");
        if (shooterId == NetworkManager.LocalClientId || victimId == NetworkManager.LocalClientId)
        {
            NetworkManager.Singleton.ConnectedClients.TryGetValue(shooterId, out var shooter);
            NetworkManager.Singleton.ConnectedClients.TryGetValue(victimId, out var victim);
            string shooterName = shooter.PlayerObject.GetComponent<NameAssignment>().playerName.Value.ToString();
            string victimName = victim.PlayerObject.GetComponent<NameAssignment>().playerName.Value.ToString();
            if (NetworkManager.LocalClientId == shooterId)
            {
                GameObject.FindWithTag("HudManager").GetComponent<HUD_Manager>().setKillText("You Killed " + victimName, Color.green);

            }
            else if (NetworkManager.LocalClientId == victimId)
            {
                GameObject.FindWithTag("HudManager").GetComponent<HUD_Manager>().setKillText("Killed by " + shooterName, Color.red);
            }
        }
    }

    [ClientRpc]
    private void hitMessageClientRpc(int amount, ulong shooterId, ulong hitPlayerId, int health)
    {
        if (NetworkManager.LocalClientId == shooterId)
        {
            spawnDamageNumber(amount, Color.white);
            GetComponent<AudioPlayer>().Play("hit");
        } else if(NetworkManager.LocalClientId == hitPlayerId)
        {
            spawnDamageNumber(amount, Color.red);
            GetComponent<AudioPlayer>().Play("hurt");
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
