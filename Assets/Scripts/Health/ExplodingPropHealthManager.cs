using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class ExplodingPropHealthManager : PropHealthManager
{
    public float maxExplosionDamage = 50;
    public GameObject temporaryDeathExplosion;
    

    void explode(){
        GameObject temporaryDeathExplosionInstance = Instantiate(temporaryDeathExplosion, transform.position, transform.rotation);
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, 5);
        foreach (Collider collider in hitColliders){
            if(collider.gameObject.GetComponent<HealthManager>()!=null){
                collider.gameObject.GetComponent<HealthManager>().TakeDamage(maxExplosionDamage);
            }
        }
    }

    protected override void Die() {
        //Debug.Log("Dead exploding prop");
        health = 0;
        isDead = true;        
        
        myPhotonView.RPC(nameof(PlayDeathEffects_RPC), RpcTarget.All);
    }
    
    [PunRPC]
    protected override void PlayDeathEffects_RPC()
    {
        explode();
        if(wreckPrefab!=null){
            Instantiate(wreckPrefab, transform.position, transform.rotation);
        }
        PhotonNetwork.Destroy(gameObject);
    }

}
