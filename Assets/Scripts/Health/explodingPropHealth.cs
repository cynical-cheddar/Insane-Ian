using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class explodingPropHealth : PropHealthManager
{
    public float maxExplosionDamage = 50;
    

    void explode(){
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, 5);
        foreach (Collider collider in hitColliders){
            collider.gameObject.GetComponent<HealthManager>().TakeDamage(maxExplosionDamage);
        }
    }

    protected override void Die() {
        health = 0;
        isDead = true;
        myPhotonView.RPC(nameof(PlayDeathEffects_RPC), RpcTarget.All);
        explode();
        
    }
    
    [PunRPC]
    protected override void PlayDeathEffects_RPC()
    {
        Instantiate(wreckPrefab, transform.position, transform.rotation);
        PhotonNetwork.Destroy(gameObject);
    }

}
