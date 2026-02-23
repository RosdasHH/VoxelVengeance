using TMPro;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using static UnityEngine.LowLevelPhysics2D.PhysicsLayers;

public class NameAssignment : NetworkBehaviour
{
    [SerializeField]
    private TextMeshPro playerNameText;

    [SerializeField]
    private TextMeshPro NameInput;

    public NetworkVariable<FixedString64Bytes> playerName = new NetworkVariable<FixedString64Bytes>(
        "Player",
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        playerName.OnValueChanged += OnNameChanged;
        playerNameText.text = playerName.Value.ToString();
        if (IsOwner)
        {
            string mainmenuPlayername = GameObject.FindGameObjectWithTag("GameManager").GetComponent<MainMenuManager>().playerName;
            Debug.Log(mainmenuPlayername);
            if(mainmenuPlayername == null || mainmenuPlayername == "")
            {
                string[] randomName = {"Würzige_Gurke", "Voxxler", "Kaktusbombe", "Noobinator", "HeadshotHugo", "GlitchGurke", "RespawnRon", "LagLegende"};
                mainmenuPlayername = randomName[Random.Range(0, randomName.Length)];
            }
            SetNameTagServerRpc(mainmenuPlayername);
        }
    }

    [ServerRpc]
    private void SetNameTagServerRpc(FixedString64Bytes name)
    {
        playerName.Value = name;
    }

    private void OnNameChanged(FixedString64Bytes pre, FixedString64Bytes post)
    {
        playerNameText.text = post.ToString();
    }

    public void changeName(string value)
    {
        if (!IsOwner)
            return;
        SetNameTagServerRpc(value);
    }
}
