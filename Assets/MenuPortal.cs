using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuPortal : MonoBehaviour
{
    // Start is called before the first frame update
    public string lobbyScene;

    public void loadLobbyScene()
    {
        SceneManager.LoadScene(lobbyScene);
    }
}
