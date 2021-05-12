using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class PropHealthManager : CollidableHealthManager
{
    
    public GameObject wreckPrefab;
    
    protected override void Die() {
        Debug.Log("Dead prop");
        health = 0;
        isDead = true;
        myPhotonView.RPC(nameof(PlayDeathEffects_RPC), RpcTarget.AllViaServer);
    }
    
    [PunRPC]
    protected override void PlayDeathEffects_RPC()
    {
        Debug.Log("Playing death effects");
        if (wreckPrefab != null) {
            Debug.Log("Found wreck prafab");
            Instantiate(wreckPrefab, transform.position, transform.rotation);  
        }
        Destroy(gameObject);

    }
}
