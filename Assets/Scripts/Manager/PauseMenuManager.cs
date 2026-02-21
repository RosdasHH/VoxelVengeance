using TMPro;
using Unity.Netcode;
using UnityEngine;

public class PauseMenuManager : NetworkBehaviour
{
    [SerializeField]
    private GameObject canvas;

    [SerializeField]
    private GameObject OptionsMenu;

    [SerializeField]
    private TMP_InputField playerNameInputField;

    private GameObject player;
    private UserInput userInput;

    private bool curMenuActive = false;

    public override void OnNetworkSpawn()
    {
        canvas.SetActive(false);
        OptionsMenu.SetActive(false);
    }

    void Update()
    {
        // if (!IsLocalPlayer)
        //     return;
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
        if (!player || !userInput)
        {
            player = NetworkManager.LocalClient.PlayerObject.gameObject;
            userInput = player.GetComponent<UserInput>();
            userInput.enabled = true;
        }

        Debug.Log("Pause");
        curMenuActive = true;
        userInput.ToggleInput(false);
        canvas.SetActive(true);
        Cursor.lockState = CursorLockMode.None;
        UserInput.playerInput.SwitchCurrentActionMap("UI");
        NameAssignment nameAss = NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<NameAssignment>();
        playerNameInputField.text = nameAss.playerName.Value.ToString();
    }

    void Unpause()
    {
        Debug.Log("unpause");
        if (!player || !userInput)
        {
            player = NetworkManager.LocalClient.PlayerObject.gameObject;
            userInput = player.GetComponent<UserInput>();
            userInput.enabled = true;
        }

        curMenuActive = false;
        userInput.ToggleInput(true);
        canvas.SetActive(false);
        OptionsMenu.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        UserInput.playerInput.SwitchCurrentActionMap("Player");
        NameAssignment nameAss = NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<NameAssignment>();
        nameAss.changeName(playerNameInputField.text);
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
        Application.Quit();
    }
}
