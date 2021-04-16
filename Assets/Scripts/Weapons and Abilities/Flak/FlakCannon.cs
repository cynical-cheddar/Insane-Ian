using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class FlakCannon : ProjectileWeapon
{

    public Transform[] overrideBarrelTransforms;
    
    
    public override void Fire(Vector3 targetPoint)
    {
        if (CanFire() && gunnerPhotonView.IsMine)
        {
            int i = 0;
            foreach (Transform barrel in overrideBarrelTransforms)
            {
                targetPoint =
                    CalculateFireDeviation(targetPoint, projectileDeviationDegrees);
                currentCooldown = fireRate;
                UseAmmo(ammoPerShot);
                //     float distanceMultiplier = CalculateDamageMultiplierCurve(Vector3.Distance(barrelTransform.position, targetPoint));
                // define weapon damage details
                WeaponDamageDetails weaponDamageDetails = new WeaponDamageDetails(myNickName, myPlayerId, myTeamId,
                    damageType, baseDamage, Vector3.zero);
                string weaponDamageDetailsJson = JsonUtility.ToJson(weaponDamageDetails);
                int j = 0;
                if (i >= 2) j = 1;
                StartCoroutine(delayedFire(j * 0.1f, targetPoint, weaponDamageDetailsJson, i));
                // do the rest in subclass
                i++;
            }
        }
    }

    IEnumerator delayedFire(float t, Vector3 targetPoint, string weaponDamageDetailsJson, int i)
    {
        yield return new WaitForSeconds(t);
        weaponPhotonView.RPC(nameof(FireRPC_FlakCannon), RpcTarget.All, targetPoint,
            weaponDamageDetailsJson, i);
    }
    
    
        
        
    [PunRPC]
    protected virtual void FireRPC_FlakCannon(Vector3 targetPoint, string serializedDamageDetails, int i)
    {
        Debug.LogWarning("Flak Cannon has not been ported to the new PhysX system");
        return;
        WeaponDamageDetails weaponDamageDetails = JsonUtility.FromJson<WeaponDamageDetails>(serializedDamageDetails);
        parentRigidbody = transform.root.GetComponent<Rigidbody>();
        // debug function to fire weapon
        weaponAnimator.SetTrigger(primaryFireAnimatorTriggerName);
      //  Debug.Log("ProjectileWeapon class object has fired");

        // if we are the owner of the photonview, then fire the real projectile

        
        /*
        PooledObject pooledProjectile =
            Pool.Instance.Spawn(projectilePrefab, barrelTransform.position, barrelTransform.rotation);


        GameObject projectile = pooledProjectile.gameObject;
        */

        GameObject obj = projectilePrefab;
        StopProjectileCollisionsWithSelf(obj);
        
            GameObject projectile = Instantiate(obj, overrideBarrelTransforms[i].position, overrideBarrelTransforms[i].rotation);
            StopProjectileCollisionsWithSelf(projectile);

            ProjectileScript projScript = projectile.GetComponent<ProjectileScript>();

            // set projscript stuff
            projScript.SetWeaponDamageDetails(weaponDamageDetails);
            projScript.ActivateProjectile(imapactParticle, missImpactParticle, projectileParticleEffectPrefab,
                impactParticleSound, impactParticleSoundMiss, imapactParticleVolume, missImpactParticleVolume);


            DoMuzzleFlashEffect();
            projectile.transform.LookAt(targetPoint);

            PlayAudioClipOneShot(weaponFireSound);
            projectile.GetComponent<Rigidbody>().mass = projectileMass;
            // FIRE REAL PROJECTILE
            if (gunnerPhotonView.IsMine)
            {
                projScript.SetTrueProjectile(true);
                projectile.GetComponent<Rigidbody>().AddForce(projectileSpeed * (projectile.transform.forward),
                    ForceMode.VelocityChange);
                if (inheritVelocityFromVehicle)
                    projectile.GetComponent<Rigidbody>().AddForce(parentRigidbody.velocity, ForceMode.VelocityChange);
            }
            // add projectile settings 
            // otherwise fire a lag compensated dummy projectile with no damage scripts enabled
            else
            {
                projScript.SetTrueProjectile(false);
                float ping = (PhotonNetwork.GetPing() * 1.0f) / 2;
                // update position by ping
                Vector3 newPos = projectile.transform.position +
                                 (projectile.transform.forward * (ping * 0.001f) * projectileSpeed);
                if (inheritVelocityFromVehicle)
                {
                    newPos += parentRigidbody.velocity * ping * 0.001f;
                }

                projectile.transform.position = newPos;
                projectile.GetComponent<Rigidbody>().AddForce(projectileSpeed * (projectile.transform.forward),
                    ForceMode.VelocityChange);
            }

            Destroy(projectile, 4f);

            //pooledProjectile.Finish(4f);
    }
}
