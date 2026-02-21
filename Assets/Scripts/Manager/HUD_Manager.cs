using System.Collections;
using TMPro;
using UnityEngine;

public class HUD_Manager : MonoBehaviour
{
    [SerializeField] private TMP_Text killText;
    private Coroutine clearRoutine;
    void Start()
    {
        
    }
    
    public void setKillText(string message, Color color)
    {
        killText.color = color;
        killText.text = message;
        if(clearRoutine != null) StopCoroutine(clearRoutine);
        clearRoutine = StartCoroutine(ClearKillMessage(3f));
    }

    private IEnumerator ClearKillMessage(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        killText.text = "";
    }
}
