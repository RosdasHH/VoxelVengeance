using Unity.Netcode;
using UnityEngine;

public class PauseMenuManager : NetworkBehaviour
{
    [SerializeField]
    private GameObject canvas;

    private GameObject player;
    private UserInput userInput;

    private bool isPaused = false;

    void Start()
    {
        player = transform.parent.gameObject;
        canvas.SetActive(false);
        userInput = player.GetComponent<UserInput>();
    }

    void Update()
    {
        if (!IsLocalPlayer)
            return;
        if (UserInput.WasEscapePressed && !isPaused)
        {
            Pause();
        }
        else if (UserInput.WasEscapePauseMenuPressed && isPaused)
        {
            Unpause();
        }
    }

    void Pause()
    {
        Debug.Log("Pause");
        isPaused = true;
        userInput.ToggleInput(false);
        canvas.SetActive(true);
        Cursor.lockState = CursorLockMode.None;
    }

    void Unpause()
    {
        isPaused = false;
        userInput.ToggleInput(true);
        canvas.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
    }
    public void ContinueBtn()
    {
        Unpause();
    }
    public void OptionsBtn()
    {
        Unpause();
    }
    public void LeaveMatchBtn()
    {
        Unpause();
    }
}
