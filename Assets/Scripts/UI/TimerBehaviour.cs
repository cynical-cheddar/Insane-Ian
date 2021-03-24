using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;
using TMPro;

public class TimerBehaviour : MonoBehaviour
{
    public float defaultTimeLimit = 300f;
    public TextMeshProUGUI timerText;
    public Timer timer = new Timer();
    bool gameOverLoading = false;

    [Serializable]
    public struct Timer {
        public float timeLeft;
        public Timer(float t) {
            timeLeft = t;
        }
    }

    // Time in seconds
    [PunRPC]
    void SetTimer(float time) {
        timer.timeLeft = time;
    }

    private void Awake() {
        timer.timeLeft = defaultTimeLimit;
    }

    // Time in seconds
    public void HostStartTimer(float timeLimit) {
        if (PhotonNetwork.IsMasterClient) {
            if (timeLimit == 0) {
                GetComponent<PhotonView>().RPC(nameof(SetTimer), RpcTarget.AllBufferedViaServer, defaultTimeLimit);
            } else {
                GetComponent<PhotonView>().RPC(nameof(SetTimer), RpcTarget.AllBufferedViaServer, timeLimit);
            }
            StartCoroutine(SyncTime());
        }
    }



    private void Update() {
        timer.timeLeft -= Time.deltaTime;
        int minutes = Mathf.FloorToInt(timer.timeLeft / 60f);
        int seconds = Mathf.FloorToInt(timer.timeLeft - minutes * 60f);
        if (seconds < 10) {
            timerText.text = $"{minutes}:0{seconds}";
        } else {
            timerText.text = $"{minutes}:{seconds}";
        }

        if (Mathf.CeilToInt(timer.timeLeft) <= 30) timerText.color = new Color(255f, 16f, 0f);
        if (Mathf.CeilToInt(timer.timeLeft) <= 10) timerText.color = new Color(118f, 0f, 0f);
        
        if (timer.timeLeft <= 0) {
            EndGame();
        }
    }

    void UpdateTimersForClients() {
        if (PhotonNetwork.IsMasterClient) {
            GetComponent<PhotonView>().RPC(nameof(SetTimer), RpcTarget.AllBufferedViaServer, timer.timeLeft);
        }
    }

    IEnumerator SyncTime() {
        while (true) {
            yield return new WaitForSecondsRealtime(5);
            UpdateTimersForClients();
        }
    }

    public void EndGame() {
        if (!gameOverLoading && PhotonNetwork.IsMasterClient) {
            gameOverLoading = true;
            PhotonNetwork.LoadLevel("GameOver");
        }
    }
}
