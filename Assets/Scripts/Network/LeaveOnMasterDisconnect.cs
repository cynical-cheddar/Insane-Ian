using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.SceneManagement;
public class LeaveOnMasterDisconnect : MonoBehaviourPunCallbacks
{
    private string quitScene = "menu";
    // Start is called before the first frame update
    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        PhotonNetwork.Disconnect();
        SceneManager.LoadScene(quitScene);
    }
}
