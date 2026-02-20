using UnityEngine;

public class DamageNumber : MonoBehaviour
{
    private void LateUpdate()
    {
        if(Camera.main != null) transform.forward = Camera.main.transform.forward;
    }
}
