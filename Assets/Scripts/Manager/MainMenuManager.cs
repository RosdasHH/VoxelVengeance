using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    [System.NonSerialized] public string networkStart;
    [System.NonSerialized] public string playerName;
    [System.NonSerialized] public GameObject character;

    [SerializeField] TMP_InputField playerNameInput;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }
    public void ButtonPressed(string action)
    {
        // if(!character) return;
        networkStart = action;
        SceneManager.LoadScene("Game");
    }
    public void SetPlayerName(string name)
    {
        playerName = name;
    }
    public void SetCharacter(GameObject _character)
    {
        character = _character;
    }
}
