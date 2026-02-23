using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    [System.NonSerialized] public string networkStart;
    [System.NonSerialized] public string playerName;

    [SerializeField] TMP_InputField playerNameInput;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }
    public void ButtonPressed(string action)
    {
        networkStart = action;
        SceneManager.LoadScene("Game");
    }
    public void SetPlayerName(string name)
    {
        playerName = name;
    }
}
