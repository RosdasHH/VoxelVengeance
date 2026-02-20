using Unity.Netcode;
using UnityEngine;

public class PauseMenuManager : NetworkBehaviour
{
    [SerializeField]
    private GameObject canvas;
    [SerializeField]
    private GameObject OptionsMenu;

    private GameObject player;
    private UserInput userInput;

    private bool curMenuActive = false;

    void Start()
    {
        player = NetworkManager.LocalClient.PlayerObject.gameObject;
        canvas.SetActive(false);
        OptionsMenu.SetActive(false);
        userInput = player.GetComponent<UserInput>();
    }

    void Update()
    {
        if (!IsLocalPlayer)
            return;
        if (UserInput.WasEscapePressed && !curMenuActive)
        {
            Pause();
        }
        else if (UserInput.WasEscapePauseMenuPressed && curMenuActive)
        {
            Unpause();
        }
    }

    void Pause()
    {
        Debug.Log("Pause");
        curMenuActive = true;
        userInput.ToggleInput(false);
        canvas.SetActive(true);
        Cursor.lockState = CursorLockMode.None;
        UserInput.playerInput.SwitchCurrentActionMap("UI");
    }

    void Unpause()
    {
        curMenuActive = false;
        userInput.ToggleInput(true);
        canvas.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        UserInput.playerInput.SwitchCurrentActionMap("Player");
    }
    public void ContinueBtn()
    {
        Unpause();
    }
    public void OptionsBtn()
    {
        canvas.SetActive(false);
        OptionsMenu.SetActive(true);
    }
    public void LeaveMatchBtn()
    {
        Unpause();
    }
}
