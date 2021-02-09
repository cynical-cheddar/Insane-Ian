using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using ExitGames.Client.Photon;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TimerBehaviour : MonoBehaviour
{

    public Text timerText;
    [SerializeField] public Timer timer = new Timer();

    [Serializable]
    public struct Timer {
        public float timeLeft;
        public Timer(float t) {
            timeLeft = t;
        }
    }

    // Time in seconds
    public void hostStartTimer(float time) {
        if (PhotonNetwork.IsMasterClient) {
            GetComponent<PhotonView>().RPC(nameof(setTimer), RpcTarget.AllBufferedViaServer, time);
            StartCoroutine(nameof(syncTime));
        }
    }

    // Time in seconds
    [PunRPC]
    void setTimer(float time) {
        timer.timeLeft = time;
    }

    private void Update() {
        timer.timeLeft -= Time.deltaTime;
        timerText.text = Mathf.RoundToInt(timer.timeLeft).ToString();
    }

    void updateTimersForClients() {
        if (PhotonNetwork.IsMasterClient) {
            GetComponent<PhotonView>().RPC(nameof(setTimer), RpcTarget.AllBufferedViaServer, timer.timeLeft);
        }
    }

    IEnumerator syncTime() {
        while (true) {
            yield return new WaitForSecondsRealtime(5);
            updateTimersForClients();
            Debug.Log("Timer updates to: " + timer.timeLeft);
        }
    }
}
