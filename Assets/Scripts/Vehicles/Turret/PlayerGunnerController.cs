using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Cinemachine;
using Photon.Pun;

public class PlayerGunnerController : MonoBehaviour
{
    new public CinemachineVirtualCamera camera;
    public float cameraSensitivity = 1;
    private TurretController turretController;
    public GunnerWeaponManager gunnerWeaponManager;

    
    public PhotonView gunnerPhotonView;
    Transform cam;
    Collider[] colliders;
    
    
    // Start is called before the first frame update
    void Start()
    {
        turretController = GetComponent<TurretController>();
        cam = Camera.main.transform;
        colliders = transform.root.gameObject.GetComponentsInChildren<Collider>();
        if (gunnerPhotonView == null) gunnerPhotonView = GetComponent<PhotonView>();
    }

     void OnEnable()
    {
        Cursor.lockState = CursorLockMode.Locked;
      //  transform.parent = transform.parent.parent;
        camera.enabled = true;
    }

    void OnDisable()
    {
        Cursor.lockState = CursorLockMode.None;
        camera.enabled = false;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        turretController.ChangeTargetYaw(cameraSensitivity * Input.GetAxis("Mouse X") * Time.fixedDeltaTime);
        turretController.ChangeTargetPitch(-(cameraSensitivity * Input.GetAxis("Mouse Y") * Time.fixedDeltaTime));
    }

    void Update()
    {
        if (Input.GetMouseButton(0))
        {
            if (gunnerPhotonView.IsMine)
            {
                Vector3 targetHitpoint = CalculateTargetingHitpoint(cam);
                gunnerWeaponManager.FireCurrentWeaponGroup(targetHitpoint);
            }
            
        }
    }

    Vector3 CalculateTargetingHitpoint(Transform sourceTransform)
    {
        Ray ray = new Ray(cam.position, cam.forward); 
        RaycastHit hit; //From camera to hitpoint, not as curent
        Transform hitTransform;
        Vector3 hitVector;
        hitTransform = FindClosestHitObject(ray, out hitVector);
        Physics.Raycast(ray.origin, ray.direction, out hit, 999);
        Vector3 hp;
                
        if (hitTransform == null)
        {
            hp = cam.position + (cam.forward * 1500f);
            Debug.Log("Null hit on targeting");
        }
        else
        {
            hp = hitVector;
            Debug.Log("HitTransform:" + hitTransform + " hitVector: " + hitVector);
        }

        return hp;
    }
    protected Transform FindClosestHitObject(Ray ray, out Vector3 hitPoint)
    {
        
        RaycastHit[] hits = Physics.RaycastAll(ray);
        colliders = transform.root.gameObject.GetComponentsInChildren<Collider>();
        Transform closestHit = null;
        float distance = 0;
        hitPoint = Vector3.zero;

        foreach (RaycastHit hit in hits)
        {
            if (hit.transform.root != this.transform && (closestHit == null || hit.distance < distance) && !colliders.Contains(hit.collider))
            {
                // We have hit something that is:
                // a) not us
                // b) the first thing we hit (that is not us)
                // c) or, if not b, is at least closer than the previous closest thing

                closestHit = hit.transform;
                distance = hit.distance;
                hitPoint = hit.point;
                
            }
        }

        // closestHit is now either still null (i.e. we hit nothing) OR it contains the closest thing that is a valid thing to hit

        return closestHit;

    }
}
