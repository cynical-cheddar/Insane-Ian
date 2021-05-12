using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using PhysX;
public class ExplodingPropHealthManager : PropHealthManager, ICollisionEnterEvent
{
    public float maxExplosionDamage = 50;
    public GameObject temporaryDeathExplosion;
    
    public float crashShakeIntensity = 3f;

   // public bool requiresData { get { return true; } }

    public new void CollisionEnter() {}


    void explode(){
        //Debug.LogWarning("Exploding Prop Health Manager has not been ported to the new PhysX system");
        //Debug.LogWarning("I need to sort out overlap sphere stuff");
        GameObject temporaryDeathExplosionInstance = Instantiate(temporaryDeathExplosion, transform.position, transform.rotation);

        //Collider[] hitColliders = Physics.OverlapSphere(transform.position, 5);
        //foreach (Collider collider in hitColliders){
        //    if(collider.gameObject.GetComponent<HealthManager>()!=null){
        //        collider.gameObject.GetComponent<HealthManager>().TakeDamage(maxExplosionDamage);
        //    }
        //}
    }


    public new void CollisionEnter(PhysXCollision col){
        if (col.rigidBody != null && col.rigidBody.velocity.magnitude > 3) {
            
            Die();
            DriverCinematicCam cam = col.gameObject.transform.root.GetComponentInChildren<DriverCinematicCam>();
                
            if(cam != null){
                Debug.Log("shake");
                cam.ShakeCams(crashShakeIntensity,1f);
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
