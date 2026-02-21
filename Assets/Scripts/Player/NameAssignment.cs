using TMPro;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

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
            SetNameTagServerRpc("Player " + NetworkManager.LocalClientId.ToString());
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
