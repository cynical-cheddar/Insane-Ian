using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class VCTestHandler : MonoBehaviour
{
    public void LoadSender() {
        SceneManager.LoadScene("VoiceChatTestSender");
    }

    public void LoadReciever() {
        SceneManager.LoadScene("VoiceChatTestReciever");
    }
}
