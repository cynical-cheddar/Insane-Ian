using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class PropHealthManager : HealthManager
{


    public GameObject wreckPrefab;
    
    protected override void Die() {
        health = 0;
        isDead = true;
        myPhotonView.RPC(nameof(PlayDeathEffects_RPC), RpcTarget.All);
    }
    
    [PunRPC]
    protected override void PlayDeathEffects_RPC()
    {
        Instantiate(wreckPrefab, transform.position, transform.rotation);
        PhotonNetwork.Destroy(gameObject);
    }
}
