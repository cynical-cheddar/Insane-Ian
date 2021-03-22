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
            if(collider.gameObject.GetComponent<HealthManager>()!=null){
                collider.gameObject.GetComponent<HealthManager>().TakeDamage(maxExplosionDamage);
            }
        }
    }

    protected override void Die() {
        
        health = 0;
        isDead = true;        
        explode();
        myPhotonView.RPC(nameof(PlayDeathEffects_RPC), RpcTarget.All);
    }
    
    [PunRPC]
    protected override void PlayDeathEffects_RPC()
    {
        if(wreckPrefab!=null){
            Instantiate(wreckPrefab, transform.position, transform.rotation);
        }
        PhotonNetwork.Destroy(gameObject);
    }

}
