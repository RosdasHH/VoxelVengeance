using System.Globalization;
using Unity.Netcode;
using UnityEngine;

public class PlayerMovement : NetworkBehaviour
{
    public override void OnNetworkSpawn()
    {
        if (!IsOwner) return;
        Camera cam = Camera.main;
        cam.GetComponent<Cam>().player = gameObject;
    }

}
