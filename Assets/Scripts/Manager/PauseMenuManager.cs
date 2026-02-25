using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenuManager : NetworkBehaviour
{
    [SerializeField]
    private GameObject canvas;

    [SerializeField]
    private GameObject OptionsMenu;

    [SerializeField]
    private TMP_InputField playerNameInputField;

    [SerializeField]
    TMP_Dropdown weaponSelection1;

    [SerializeField]
    TMP_Dropdown weaponSelection2;

    public Weapon[] selectedWeapons = new Weapon[2];

    EquipWeapon ew;

    [SerializeField]
    public Weapon[] Weapons;

    [System.Serializable]
    public class Weapon
    {
        public string name;
        public GameObject Prefab;
    };

    private GameObject player;
    private UserInput userInput;

    private bool curMenuActive = false;

    private void Start()
    {
        foreach (Weapon t in Weapons)
        {
            weaponSelection1.options.Add(new TMP_Dropdown.OptionData() { text = t.name });
            weaponSelection2.options.Add(new TMP_Dropdown.OptionData() { text = t.name });
        }
        //Theres probably a better way to set the dropdown1 to pistol at start..
        weaponSelection1.value = 0;
        weaponSelection1.value = 1;
        weaponSelection1.value = 0;
        weaponSelection2.value = 1;
    }

    public void OnChangeDropdown()
    {
        int w0 = weaponSelection1.value;
        int w1 = weaponSelection2.value;

        selectedWeapons[0] = Weapons[w0];
        selectedWeapons[1] = Weapons[w1];

        var localPlayer = NetworkManager.Singleton?.LocalClient?.PlayerObject;
        if (localPlayer == null)
        {
            return;
        }

        var equip = localPlayer.GetComponent<EquipWeapon>();
        if (equip == null)
        {
            return;
        }

        if (!equip.IsOwner || !equip.IsSpawned)
        {
            return;
        }

        equip.SetLoadout(w0, w1);

        equip.reloadWeaponLocalOnly();
    }

    public override void OnNetworkSpawn()
    {
        canvas.SetActive(false);
        OptionsMenu.SetActive(false);
        if (IsOwner && IsClient)
            ew =
                NetworkManager.Singleton.LocalClient.PlayerObject.gameObject.GetComponent<EquipWeapon>();
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
        Cursor.visible = true;
        UserInput.playerInput.SwitchCurrentActionMap("UI");
        NameAssignment nameAss =
            NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<NameAssignment>();
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
        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = false;
        UserInput.playerInput.SwitchCurrentActionMap("Player");
        NameAssignment nameAss =
            NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<NameAssignment>();
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
        NetworkManager.Singleton.Shutdown();
        SceneManager.LoadScene("MainMenu");
    }
}
