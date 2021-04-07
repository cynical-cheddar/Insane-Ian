using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;
using UnityEngine.UI;
using System.Web;

public class VoiceChatTest : MonoBehaviour
{
    public InputField idInputField;
    public InputField messageInputField;
    public Text messages;

    [DllImport("__Internal")]
    private static extern void initialize();

    [DllImport("__Internal")]
    private static extern void join(string recvID);

    [DllImport("__Internal")]
    private static extern void signal(string sigName);

    [DllImport("__Internal")]
    private static extern void sendMessage(string msg);

    void Start() {
        initialize();
    }

    public void Connect() {
        string id = idInputField.text;
        Debug.Log($"ID: {id}");
        join(id);
    }

    public void Reset() {
        signal("Reset");
    }
    public void Go() {
        signal("Go");
    }

    public void Fade() {
        signal("Fade");
    }

    public void Off() {
        signal("Off");
    }

    public void Send() {
        string text = messageInputField.text;
        messages.text += $"\n{text}";
        sendMessage(text);
    }

    public void Clear() {
        messages.text = "Messages:\nMessages cleared";
    }
}
