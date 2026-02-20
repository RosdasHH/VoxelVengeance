using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class OptionsMenu : NetworkBehaviour
{
    GameObject player;
    PlayerMovement playerMovement;

    void Start() { }

    void Update()
    {
        if (UserInput.WasEscapePauseMenuPressed)
        {
            gameObject.SetActive(false);
        }
    }

    public void SensiSliderChange(Slider slider)
    {
        if (!player || !playerMovement)
        {
            player = NetworkManager.LocalClient.PlayerObject.gameObject;
            playerMovement = player.GetComponent<PlayerMovement>();
        }
        playerMovement.rotationSpeed = slider.value;
    }
}
