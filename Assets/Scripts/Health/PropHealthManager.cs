using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class PropHealthManager : HealthManager
{
    
    public GameObject wreckPrefab;
    public float environmentCollisionResistance = 500;
    
    protected override void Die() {
        Debug.Log("Dead prop");
        health = 0;
        isDead = true;
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
    void OnCollisionEnter(Collision collision) {
        if (PhotonNetwork.IsMasterClient) {
            Vector3 collisionNormal = collision.GetContact(0).normal;
            Vector3 collisionForce = collision.impulse;
            if (Vector3.Dot(collisionForce, collisionNormal) < 0) collisionForce = -collisionForce;
            collisionForce /= Time.fixedDeltaTime;
            collisionForce = transform.InverseTransformDirection(collisionForce);

            VehicleManager otherVehicleManager = collision.gameObject.GetComponent<VehicleManager>();

            Vector3 collisionPoint = Vector3.zero;
            for (int i = 0; i < collision.contactCount; i++) {
                collisionPoint += collision.GetContact(i).point;
            }
            collisionPoint /= collision.contactCount;

            Vector3 contactDirection = transform.InverseTransformPoint(collisionPoint);
            float damage = CalculateCollisionDamage(collisionForce, contactDirection, otherVehicleManager != null);
            Debug.Log(damage);
            
            if (otherVehicleManager != null) {
                Weapon.WeaponDamageDetails rammingDetails = otherVehicleManager.rammingDetails;
                rammingDetails.damage = damage;
                TakeDamage(rammingDetails);
            }
            else {
                TakeDamage(damage);
            }
        }
    }

    private float CalculateCollisionDamage(Vector3 collisionForce, Vector3 collisionDirection, bool hitVehicle) {
        float collisionResistance = 1;

        foreach (CollisionArea collisionArea in collisionAreas) {
            Vector3 verticalComponent = Vector3.ProjectOnPlane(collisionDirection, collisionArea.rotation * Vector3.right).normalized;
            Vector3 horizontalComponent = Vector3.ProjectOnPlane(collisionDirection, collisionArea.rotation * Vector3.up).normalized;
            Vector3 areaCentre = collisionArea.rotation * Vector3.forward;

            if (Vector3.Dot(areaCentre, verticalComponent) > Mathf.Cos(collisionArea.height / 2) &&
                Vector3.Dot(areaCentre, horizontalComponent) > Mathf.Cos(collisionArea.width / 2)) {

                collisionResistance = collisionArea.collisionResistance;
                break;
            }
        }

        float reducedForce = collisionForce.magnitude / baseCollisionResistance;
        if (!hitVehicle) reducedForce /= environmentCollisionResistance;
        reducedForce /= collisionResistance;

        return reducedForce;
    }

}
