using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;
using UnityEngine.UI;
using System.Web;

public class VoiceChatTestSender : MonoBehaviour
{
    public InputField idInputField;
    public InputField messageInputField;
    public Text messages;

    [DllImport("__Internal")]
    private static extern void initializeVCTestSender();

    [DllImport("__Internal")]
    private static extern void joinVCTestSender(string recvID);

    [DllImport("__Internal")]
    private static extern void signalVCTestSender(string sigName);

    [DllImport("__Internal")]
    private static extern void sendMessageVCTestSender(string msg);

    void Start() {
        initializeVCTestSender();
    }

    public void Connect() {
        string id = idInputField.text;
        Debug.Log($"ID: {id}");
        joinVCTestSender(id);
    }

    public void Reset() {
        signalVCTestSender("Reset");
    }
    public void Go() {
        signalVCTestSender("Go");
    }

    public void Fade() {
        signalVCTestSender("Fade");
    }

    public void Off() {
        signalVCTestSender("Off");
    }

    public void Send() {
        string text = messageInputField.text;
        messages.text += $"\nYou: {text}";
        sendMessageVCTestSender(text);
    }

    public void Clear() {
        messages.text = "Messages:\nMessages cleared";
    }

    public void RecieveMessage(string msg) {
        messages.text += $"\nPeer: {msg}";
    }
}
