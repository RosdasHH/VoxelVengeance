using UnityEngine;

public class Cam : MonoBehaviour
{
    [System.NonSerialized]
    public GameObject player;
    void LateUpdate()
    {
        if (player == null) return;

        transform.position = new Vector3(player.transform.position.x, player.transform.position.y+2, player.transform.position.z-4);
    }
}
