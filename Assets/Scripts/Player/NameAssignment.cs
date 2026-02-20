using TMPro;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class NameAssignment : NetworkBehaviour
{
    [SerializeField]
    private TextMeshPro playerNameText;

    private NetworkVariable<FixedString64Bytes> playerName = new NetworkVariable<FixedString64Bytes>("Player", NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        playerName.OnValueChanged += OnNameChanged;
        if (IsOwner) SetNameTagServerRpc(NetworkManager.LocalClientId.ToString());
    }
    [ServerRpc]
    private void SetNameTagServerRpc(FixedString64Bytes name)
    {
        playerName.Value = name;
    }
    private void OnNameChanged(FixedString64Bytes pre, FixedString64Bytes post)
    {
        playerNameText.text = post.Value;
    }
}
