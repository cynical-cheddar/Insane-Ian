using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using Photon.Realtime;

public class NetworkPlayerVehicle : MonoBehaviourPunCallbacks
{
    // Start is called before the first frame update
    public List<MonoBehaviour> scriptsToDisable;
    void Start()
    {
        if (!photonView.IsMine)
        {
            foreach (MonoBehaviour m in (scriptsToDisable))
            {
                m.enabled = false;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
