using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;
using UnityEngine.UI;
using System.Web;

public class VoiceChatTestReciever : MonoBehaviour
{
    public List<Text> notifiers;
    public InputField messageInputField;
    public Text messages;

    [DllImport("__Internal")]
    private static extern void initializeVCTestReciever();

    [DllImport("__Internal")]
    private static extern void sendMessageVCTestReciever(string msg);

    void Start() {
        initializeVCTestReciever();
    }

    public void RecieveSignal(int sigID) {
        for (int i = 0; i < notifiers.Count; i++) {
            if (i == sigID) notifiers[i].color = Color.red;
            else notifiers[i].color = Color.black;
        }
    }

    public void Send() {
        string text = messageInputField.text;
        messages.text += $"\nYou: {text}";
        sendMessageVCTestReciever(text);
    }

    public void Clear() {
        messages.text = "Messages:\nMessages cleared";
    }

    public void RecieveMessage(string msg) {
        messages.text += $"\nPeer: {msg}";
    }
}
