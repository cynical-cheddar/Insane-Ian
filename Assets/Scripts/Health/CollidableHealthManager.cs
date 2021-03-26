using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using System;

public class CollidableHealthManager : HealthManager
{
    [Serializable]
    public struct CollisionArea {
        public bool show;
        public Vector3 rotationEuler;

        [HideInInspector]
        public Quaternion rotation;
        public float width;
        public float height;
        public float collisionResistance;
    }

    public float defaultCollisionResistance = 1;
    protected float deathForce = Mathf.Pow(10, 6.65f);
    protected float baseCollisionResistance = 1;
    public float environmentCollisionResistance = 1;

    public List<CollisionArea> collisionAreas;

    protected new void Start(){
        baseCollisionResistance = deathForce / maxHealth;
        base.Start();
    }
    
    protected void OnCollisionEnter(Collision collision) {
        if (PhotonNetwork.IsMasterClient) {
            Vector3 collisionNormal = collision.GetContact(0).normal;
            Vector3 collisionForce = collision.impulse;
            if (Vector3.Dot(collisionForce, collisionNormal) < 0) collisionForce = -collisionForce;
            collisionForce /= Time.fixedDeltaTime;
            collisionForce = transform.InverseTransformDirection(collisionForce);

            VehicleHealthManager otherVehicleManager = collision.gameObject.GetComponent<VehicleHealthManager>();

            Vector3 collisionPoint = Vector3.zero;
            for (int i = 0; i < collision.contactCount; i++) {
                collisionPoint += collision.GetContact(i).point;
            }
            collisionPoint /= collision.contactCount;

            Vector3 contactDirection = transform.InverseTransformPoint(collisionPoint);
            float damage = CalculateCollisionDamage(collisionForce, contactDirection, otherVehicleManager != null);
            //Debug.Log(damage);
            
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

    protected float CalculateCollisionDamage(Vector3 collisionForce, Vector3 collisionDirection, bool hitVehicle) {
        float collisionResistance = defaultCollisionResistance;

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
