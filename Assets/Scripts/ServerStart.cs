#if UNITY_SERVER
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ServerStart : MonoBehaviour
{
    void Start()
    {
        if (System.Environment.GetCommandLineArgs().Contains("-server"))
        {
            GetComponent<MainMenuManager>().networkStart = "StartServer";
            SceneManager.LoadScene("Game");
        }
    }
}
#endif