using UnityEngine;

public class CharacterSelection : MonoBehaviour
{
    // in future set event to trigger character selection as clicking on the character
    [SerializeField]
    GameObject characterMenu;

    MainMenuManager GameManager;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GameManager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<MainMenuManager>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void SelectCharacter(GameObject character)
    {
        GameManager.SetCharacter(character);
    }
    public void OpenMenu()
    {
        characterMenu.SetActive(true);
        Cursor.visible = true;
    }
    public void CloseMenu()
    {
        characterMenu.SetActive(false);
    }
}
