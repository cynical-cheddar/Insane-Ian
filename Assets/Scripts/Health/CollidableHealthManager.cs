using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using Random = UnityEngine.Random;
using System;
using PhysX;

public class CollidableHealthManager : HealthManager, ICollisionEnterEvent
{
    public bool requiresData { get { return true; } }

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
    public GameObject audioSourcePrefab = null;
    public float crashSoundsSmallDamageThreshold = 5f;
    public float crashSoundsLargeDamageThreshold = 40f;
    public List<AudioClip> crashSoundsSmall = new List<AudioClip>();
    public List<AudioClip> crashSoundsLarge = new List<AudioClip>();
    public float crashMasterVolume = 1f;

    protected float deathForce = Mathf.Pow(10, 6.65f);
    protected float baseCollisionResistance = 1;
    public float environmentCollisionResistance = 1;

    [Header("Collision area 0 should be front")]
    public List<CollisionArea> collisionAreas = new List<CollisionArea>();

    protected bool resetting = false;
    
    public float rammingDamageMultiplier = 1f;
    
    protected float timeSinceLastRam = 0f;
    protected new void Start(){
        baseCollisionResistance = deathForce / maxHealth;
        base.Start();
    }
    protected void Update(){
        timeSinceLastRam += Time.deltaTime;
    }

    public void CollisionEnter() {}

    public void CollisionEnter(PhysXCollision collision) {
        if (myPhotonView.IsMine && collision.contactCount > 0) {
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
            if(otherVehicleManager!=null)damage  *= otherVehicleManager.rammingDamageMultiplier;
            //Debug.Log(damage);
    
            // instantiate damage sound over network
            if(damage > crashSoundsSmallDamageThreshold && timeSinceLastRam > 0.15f) myPhotonView.RPC(nameof(PlayDamageSoundNetwork), RpcTarget.All, damage);

            damage = damage / rammingDamageResistance;
            
            
            if (GetComponent<COMDropper>() != null && !resetting) {
                Debug.LogWarning("Whatever this is has not been ported to the new PhysX system");
                // Might not be needed anymore as COM is no longer dropped?
                // resetting = true;
                // Rigidbody rb = GetComponent<Rigidbody>();
                // StartCoroutine(ResetPreviousCOM(rb.centerOfMass, 1f));
                // rb.centerOfMass = Vector3.zero;
            }
            
            if (otherVehicleManager != null) {
                Weapon.WeaponDamageDetails rammingDetails = otherVehicleManager.rammingDetails;
                rammingDetails.damage = damage;
                
                TakeDamage(rammingDetails);
            }
            else {
                TakeDamage(damage);
            }
            timeSinceLastRam= 0f;
        }
    }

    protected IEnumerator ResetPreviousCOM(Vector3 com, float t)
    {
        yield return new WaitForSeconds(t);
        GetComponent<Rigidbody>().centerOfMass = com;
        resetting = false;
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


    [PunRPC]
    protected void PlayDamageSoundNetwork(float damage)
    {
        if (audioSourcePrefab != null) {
            GameObject crashSound = Instantiate(audioSourcePrefab, transform.position, Quaternion.identity);
            AudioSource a = crashSound.GetComponent<AudioSource>();
            if (damage > crashSoundsLargeDamageThreshold && crashSoundsLarge.Count > 0)
            {
                int randInt = Random.Range(0, crashSoundsLarge.Count - 1);
                a.clip = crashSoundsLarge[randInt];
            }
            else if(crashSoundsSmall.Count > 0)
            {
                int randInt = Random.Range(0, crashSoundsSmall.Count - 1);
                a.clip = crashSoundsLarge[randInt];
            }

            if (a.clip != null)
            {
                a.Play();
                Destroy(crashSound, a.clip.length);
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }
}
