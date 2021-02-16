using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;

public class TimerBehaviour : MonoBehaviour
{
    public float initialTime = 300;
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
    [PunRPC]
    void SetTimer(float time) {
        timer.timeLeft = time;
    }

    // Time in seconds
    public void HostStartTimer() {
        if (PhotonNetwork.IsMasterClient) {
            GetComponent<PhotonView>().RPC(nameof(SetTimer), RpcTarget.AllBufferedViaServer, initialTime);
            StartCoroutine(SyncTime());
        }
    }



    private void Update() {
        timer.timeLeft -= Time.deltaTime;
        timerText.text = Mathf.RoundToInt(timer.timeLeft).ToString();
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
}
