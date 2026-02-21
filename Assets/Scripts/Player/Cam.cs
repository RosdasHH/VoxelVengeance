using UnityEngine;

public class Cam : MonoBehaviour
{
    [System.NonSerialized]
    public GameObject player;

    void LateUpdate()
    {
        if (player == null)
            return;
    }
}
