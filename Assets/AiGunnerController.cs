using Cinemachine;
using Photon.Pun;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PhysX;

[VehicleScript(ScriptType.aiGunnerScript)]

public class AiGunnerController : MonoBehaviour
{

    private TurretController turretController;
    private GunnerWeaponManager gunnerWeaponManager;

    Transform target;
    List<Transform> enemies = new List<Transform>();
    bool shouldFire = true;

    public PhysXCollider.CollisionLayer raycastLayers;
    uint rigidbodyVehicleId = 0;


    public void Start() {
        gunnerWeaponManager = GetComponent<GunnerWeaponManager>();
        turretController = GetComponent<TurretController>();
        // Invoke(nameof(GetAllTargets), 1f);
        GetAllTargets();
        if (enemies.Count == 0) {
            Debug.Log("Gunner AI for " + transform.root.gameObject.name + " could not find any targets.");
            return;
        }
        rigidbodyVehicleId = transform.root.GetComponentInChildren<PhysXRigidBody>().vehicleId;
        StartCoroutine(GetBestTarget());
        StartCoroutine(YesOrNoFire());
        StartCoroutine(FireCoroutine());
    }

    void GetAllTargets() {
        NetworkPlayerVehicle[] npvs = FindObjectsOfType<NetworkPlayerVehicle>();
        enemies.Clear();
        foreach (NetworkPlayerVehicle npv in npvs) {
            if (npv != transform.GetComponentInParent<NetworkPlayerVehicle>()) {
                enemies.Add(npv.transform);
            }
        }
    }

    IEnumerator YesOrNoFire() {
        while (true) {
            int randNumber = Random.Range(0,7);

            if(randNumber < 4){
                shouldFire = false;
            }
            else if(randNumber >= 4){
                shouldFire = true;
            }

            yield return new WaitForSeconds(3f);
        }
    }

    IEnumerator GetBestTarget() {
        yield return new WaitForSeconds(1.5f);
        while (true) {
            target = GetClosestEnemy();
            turretController.SetTarget(target.gameObject);
            yield return new WaitForSeconds(3f);
        }
    }

    Transform GetClosestEnemy() {
        Transform tMin = null;
        float minDist = Mathf.Infinity;
        Vector3 currentPos = transform.position;
        foreach (Transform t in enemies)
        {
            float dist = Vector3.Distance(t.position, currentPos);
            if (dist < minDist)
            {
                tMin = t;
                minDist = dist;
            }
        }
        return tMin;
    }

    // Update is called once per frame
    IEnumerator FireCoroutine() {
        while (true) {
            if (target !=null && shouldFire) {
                if (gunnerWeaponManager.CurrentWeaponGroupCanFire()) {
                    Vector3 hitpoint = CalculateTargetingHitpoint(transform.position, target.position);
                    
                    if (hitpoint != Vector3.zero) {
                        gunnerWeaponManager.FireCurrentWeaponGroup(CalculateFireDeviation(target.position, 10f));
                        yield return new WaitForSeconds(gunnerWeaponManager.currentWeaponControlGroup.weapons[0].fireRate);
                    }
                    else yield return new WaitForSeconds(0.5f);
                }
            }
            yield return new WaitForEndOfFrame();
        }
    }

    protected Vector3 CalculateFireDeviation(Vector3 oldTargetPoint, float maxDegrees) {
        if (maxDegrees == 0) return oldTargetPoint;
        float deviationDegreesTraverse = Random.Range(0, maxDegrees);
        float deviationDegreesElevation = Random.Range(0, maxDegrees);
        // get vector distance from barrel to hitpoint
        float range = Vector3.Distance(oldTargetPoint, transform.position);

        float max = Mathf.Tan(Mathf.Deg2Rad * maxDegrees) * range;


        Vector3 deviation3D = Random.insideUnitSphere * max;



        Vector3 newTargetPoint = oldTargetPoint + deviation3D;
        
        
        return newTargetPoint;
    }


    Vector3 CalculateTargetingHitpoint(Vector3 start, Vector3 end) {
        Ray ray = new Ray(start, end - start);
     //   RaycastHit hit; //From camera to hitpoint, not as curent
        Transform hitTransform;
        Vector3 hitVector;
        hitTransform = FindClosestHitObject(ray, out hitVector);
       // Physics.Raycast(ray.origin, ray.direction, out hit, 999);
        Vector3 hp;

        if (hitTransform == null) {
            hp = Vector3.zero;
        }
        else if(!hitTransform.CompareTag("Player")){
            hp = Vector3.zero;
        }
        else {
            hp = hitVector;
        }

        return hp;
    }

    protected Transform FindClosestHitObject(Ray ray, out Vector3 hitPoint) {
        Transform closestHit = null;
        hitPoint = Vector3.zero;

        PhysXRaycastHit hitPhysX = PhysXRaycast.GetRaycastHit();
        if (PhysXRaycast.Fire(ray.origin, ray.direction, hitPhysX, raycastLayers, rigidbodyVehicleId)) {
            closestHit = hitPhysX.transform;
            hitPoint = hitPhysX.point;
        }

        PhysXRaycast.ReleaseRaycastHit(hitPhysX);

        return closestHit;
    }
}
