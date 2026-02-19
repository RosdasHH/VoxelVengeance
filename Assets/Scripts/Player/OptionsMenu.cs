using UnityEngine;
using UnityEngine.UI;

public class OptionsMenu : MonoBehaviour
{
    GameObject player;
    PlayerMovement playerMovement;


    void Start()
    {
        player = transform.parent.gameObject;
        playerMovement = player.GetComponent<PlayerMovement>();
    }

    void Update()
    {
        if (UserInput.WasEscapePauseMenuPressed)
        {
            gameObject.SetActive(false);
        }
    }
    public void SensiSliderChange(Slider slider)
    {
        playerMovement.rotationSpeed = slider.value;
    }
}
