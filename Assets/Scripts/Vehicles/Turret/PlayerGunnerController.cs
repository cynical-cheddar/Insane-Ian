using Cinemachine;
using Photon.Pun;
using System.Linq;
using UnityEngine;
using PhysX;

[VehicleScript(ScriptType.playerGunnerScript)]
public class PlayerGunnerController : MonoBehaviour {
    new public CinemachineVirtualCamera camera;
    public float cameraSensitivity = 1;
    private TurretController turretController;
    public GunnerWeaponManager gunnerWeaponManager;
    public Transform barrelTransform;
    public PhotonView gunnerPhotonView;
    public float maxZoomOut = 6f;
    Transform cam;
    PhysXCollider[] colliders;

    public PhysXCollider.CollisionLayer raycastLayers;

    uint rigidbodyVehicleId = 0;

    CinemachineTransposer cinemachineTransposer;

    // Start is called before the first frame update
    void Start() {
      //  Debug.LogWarning("Player Gunner Controller has not been ported to the new PhysX system");
      //  return;
        turretController = GetComponent<TurretController>();
        cam = Camera.main.transform;
        colliders = transform.root.gameObject.GetComponentsInChildren<PhysXCollider>();
        if (gunnerPhotonView == null) gunnerPhotonView = GetComponent<PhotonView>();

        rigidbodyVehicleId = transform.root.GetComponentInChildren<PhysXRigidBody>().vehicleId;

        cinemachineTransposer = camera.GetCinemachineComponent<CinemachineTransposer>();
    }

    void OnEnable() {
        if (FindObjectOfType<PlinthManager>() == null) Cursor.lockState = CursorLockMode.Locked;
        //  transform.parent = transform.parent.parent;
        camera.enabled = true;
    }

    void OnDisable() {
        Cursor.lockState = CursorLockMode.None;
        camera.enabled = false;
    }

    // Update is called once per frame
    void FixedUpdate() {

    }

    

    void Update() {
        // fire 1
         if (Input.GetButton("Fire1")) {
             if (gunnerPhotonView.IsMine) {
                 if (gunnerWeaponManager.CurrentWeaponGroupCanFire()) {
                     Vector3 targetHitpoint;
                     if (turretController.inDeadZone) targetHitpoint = CalculateTargetingHitpoint(cam);
                     else targetHitpoint = CalculateTargetingHitpoint(barrelTransform);

                     gunnerWeaponManager.FireCurrentWeaponGroup(targetHitpoint);
                 }
             }
         }

         if (Input.GetButtonUp("Fire1")) {
             if (gunnerPhotonView.IsMine) {
                 gunnerWeaponManager.CeaseFireCurrentWeaponGroup();
             }
         }

        // // relaod
         if (Input.GetButtonDown("Reload")) {
             if (gunnerPhotonView.IsMine) {
                 gunnerWeaponManager.ReloadCurrentWeaponGroup();
             }
         }

         if (Input.GetButtonDown("Ultimate"))
         {
             if (gunnerPhotonView.IsMine)
             {
                 gunnerWeaponManager.SelectUltimate();
             }
         }

        if (Input.GetKey(KeyCode.Space)) {
            camera.m_Lens.FieldOfView = Mathf.Lerp(camera.m_Lens.FieldOfView, 30, 0.1f);
        } else {
            camera.m_Lens.FieldOfView = Mathf.Lerp(camera.m_Lens.FieldOfView, 60, 0.1f);
        }

        turretController.ChangeTargetYaw(cameraSensitivity * Input.GetAxis("Mouse X") * Time.deltaTime);
        turretController.ChangeTargetPitch(-(cameraSensitivity * Input.GetAxis("Mouse Y") * Time.deltaTime));
        turretController.UpdateTargeterRotation();

       
        float newy = Mathf.Max(cinemachineTransposer.m_FollowOffset.y - Input.mouseScrollDelta.y * 0.2f, 1.5f);
        float newz = Mathf.Min(cinemachineTransposer.m_FollowOffset.z + Input.mouseScrollDelta.y * 0.6f, -3f);

        if (newy < maxZoomOut) {
            cinemachineTransposer.m_FollowOffset.y = newy;
            cinemachineTransposer.m_FollowOffset.z = newz;
        }


    }

    Vector3 CalculateTargetingHitpoint(Transform sourceTransform) {
        Ray ray = new Ray(sourceTransform.position, sourceTransform.forward);
     //   RaycastHit hit; //From camera to hitpoint, not as curent
        Transform hitTransform;
        Vector3 hitVector;
        hitTransform = FindClosestHitObject(ray, out hitVector);
       // Physics.Raycast(ray.origin, ray.direction, out hit, 999);
        Vector3 hp;

        if (hitTransform == null) {
            hp = cam.position + (cam.forward * 1500f);

        } else {
            hp = hitVector;

        }

        return hp;
    }
    protected Transform FindClosestHitObject(Ray ray, out Vector3 hitPoint) {

       // RaycastHit[] hits = Physics.RaycastAll(ray);
        colliders = transform.root.gameObject.GetComponentsInChildren<PhysXCollider>();
        Transform closestHit = null;
        float distance = 0;
        hitPoint = Vector3.zero;

        PhysXRaycastHit hitPhysX = PhysXRaycast.GetRaycastHit();
         if (PhysXRaycast.Fire(ray.origin, ray.direction, hitPhysX, 9999, raycastLayers, rigidbodyVehicleId)){
             closestHit = hitPhysX.transform;
             hitPoint = hitPhysX.point;
         }

         

          PhysXRaycast.ReleaseRaycastHit(hitPhysX);


        

        // closestHit is now either still null (i.e. we hit nothing) OR it contains the closest thing that is a valid thing to hit

        return closestHit;

    }

    
}
