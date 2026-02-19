using UnityEngine;
using Unity.Netcode;

public class PauseMenuManager : NetworkBehaviour {
    [SerializeField]
    private GameObject canvas;

    private GameObject player;
    private UserInput userInput;

    private bool isPaused = false;
    void Start()
    {
        player = transform.parent.gameObject;
        userInput = player.GetComponent<UserInput>();
    }

    void Update()
    {
        if(!IsLocalPlayer) return;
        if(UserInput.WasEscapePressed && !isPaused)
        {
            Pause();
        }else if(UserInput.WasEscapePauseMenuPressed && isPaused)
        {
            Unpause();
        }
    }
    void Pause()
    {
        isPaused = true;
        userInput.ToggleInput(false);
        Cursor.lockState = CursorLockMode.None;
    }
    void Unpause()
    {
        isPaused = false;
        userInput.ToggleInput(true);
        Cursor.lockState = CursorLockMode.Locked;
    }
}