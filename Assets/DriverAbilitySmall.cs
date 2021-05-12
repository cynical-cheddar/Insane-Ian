using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Photon.Pun;

public class DriverAbilitySmall : Equipment
{

    
        protected float currentCooldown = 0f;
    public float cooldown = 1f;
    public float timeSinceLastFire = 0f;
    public bool isLockOnAbility = false;
    protected PhotonView driverPhotonView;
    protected PhotonView abilityPhotonView;
    protected NetworkPlayerVehicle _networkPlayerVehicle;
    protected VehicleHealthManager myVehicleManager;
    protected DriverAbilityManager driverAbilityManager;
        
    protected int myPlayerId;
    protected int myTeamId;
    protected string myNickName = "";

    protected bool allowLockOn = true;
    
    protected bool isSetup = false;

    public bool abilityActivated = false;

    public AudioClip activateAudioClip;


    
    
    // lock on stuff
    
    protected Canvas uiCanvas;
    protected float updateCooldown = 1f;
    protected float upd = 0f;

    protected int listLength = 0;

    protected GameObject targetOverlay;

    protected List<Transform> enemyList = new List<Transform>();
    public GameObject targetOverlayPrefab;

    public float maxTargetAngle = 60f;
    public float maxDist = 30f;

    protected Transform target;
    protected Transform lastTarget;
    
    public AudioClip targetChangeAudioclip;
    public void SetLockOn(bool set)
    {
        allowLockOn = set;

        if(isSetup){
            if (set == false)
            {
                targetOverlay.SetActive(false);
            }
            else
            {
                targetOverlay.SetActive(true);
            }
        }
    }

    public virtual void JustCollided()
    {
        
    }
    
    protected virtual void Update()
    {

        if(abilityActivated){
            duration += Time.deltaTime;
        }
        


        timeSinceLastFire += Time.deltaTime;

        currentActivationCooldown -= Time.deltaTime;

        if (currentCooldown >= 0)
        {
            currentCooldown -= Time.deltaTime;
        }

        if (allowLockOn && isLockOnAbility && CanUseAbility()&& isSetup && _networkPlayerVehicle.GetDriverID() == PhotonNetwork.LocalPlayer.ActorNumber)
        {
            LockOnTargetSelection();
        }

    }



    protected void LockOnTargetSelection()
    {
        // display ui over all vehicles
        target = GetBestTarget();
        if (target != lastTarget && target != transform.root)
        {
            GetComponent<AudioSource>().PlayOneShot(targetChangeAudioclip);
        }

        lastTarget = target;
        if (target != transform.root) {

            Vector3 aimPos = target.position;
            Vector3 screenPos = Camera.main.WorldToScreenPoint(aimPos);
            Vector2 movePos;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(uiCanvas.transform as RectTransform, screenPos, uiCanvas.worldCamera, out movePos);
            
            targetOverlay.GetComponent<RectTransform>().anchoredPosition = movePos;

            if (Mathf.Abs(Vector3.SignedAngle(Camera.main.transform.forward,  aimPos - Camera.main.transform.position, transform.up)) > 90f){
                targetOverlay.SetActive(false);
            }
            else {
                targetOverlay.SetActive(true);
            }
        }
        else
        {
   
            targetOverlay.SetActive(false);
        }
        
    }
    
    protected Transform GetBestTarget()
    {
        
        float bestAngle = maxTargetAngle;
        Transform bestTarget = transform.root;
        // get the enemy with smallest angle difference
        foreach (Transform t in enemyList)
        {
            if (t != transform.root)
            {
                if (Mathf.Abs(Vector3.Angle(Camera.main.transform.forward,
                    t.position - Camera.main.transform.position)) < bestAngle && Vector3.Distance(transform.position, t.position) < maxDist)
                {
                    bestTarget = t;
                }
            }
        }
        

        return bestTarget;
    }
    

    protected float activationCooldown = 2f;
    protected float currentActivationCooldown = 2f;

    public float duration = 2f;
    protected float progress = 0f;


    public bool CanUseAbility()
    {
        if(timeSinceLastFire > cooldown) return true;
        else return false;
    }



    protected void InvokeDeactivate()
    {
        driverAbilityManager.DeactivatePrimaryAbility();
    }
   

    public virtual void SetupAbility()
    {
        driverPhotonView = transform.root.GetComponent<PhotonView>();
        // assign photon view to the driver
        abilityPhotonView = GetComponent<PhotonView>();
        abilityPhotonView.TransferOwnership(driverPhotonView.Owner);
        
        
        //Player gunnerPlayer = gunnerPhotonView.Owner;

        _networkPlayerVehicle = driverPhotonView.GetComponent<NetworkPlayerVehicle>();
        myVehicleManager = driverPhotonView.GetComponent<VehicleHealthManager>();
        driverAbilityManager = driverPhotonView.GetComponent<DriverAbilityManager>();

        if (_networkPlayerVehicle != null)
        {
            myNickName = _networkPlayerVehicle.GetDriverNickName();
            myPlayerId = _networkPlayerVehicle.GetDriverID();
            myTeamId = _networkPlayerVehicle.teamId;
        }
        else
        {
            Debug.LogError("Ability does not belong to a valid vehicle!! Assigning owner to null");
        }

        uiCanvas = FindObjectOfType<UiCanvasBehaviour>().GetComponent<Canvas>();
        if(targetOverlay !=null)Destroy(targetOverlay);
        targetOverlay = Instantiate(targetOverlayPrefab, uiCanvas.transform);
        targetOverlay.SetActive(false);
        lastTarget = transform;
        isSetup = true;
        Invoke(nameof(GetCarList), 12f);
    }

    void GetCarList()
    {
        NetworkPlayerVehicle[] npvs = FindObjectsOfType<NetworkPlayerVehicle>();
        foreach (NetworkPlayerVehicle npv in npvs)
        {
            if(npv.transform.root != transform.root)enemyList.Add(npv.transform);
            
        }
    }

    public virtual void ResetAbility()
    {
        
    }

    public virtual void ActivateAbility()
    {
        
        if (!isSetup)
        {
            SetupAbility();
        }

        currentActivationCooldown = activationCooldown;

        abilityActivated = true;
       // Debug.Log("Activate driver ability");
        abilityPhotonView.RPC(nameof(ActivationEffects_RPC), RpcTarget.All);
        targetOverlay.SetActive(false);
    }

    public virtual void DeactivateAbility()
    {
        abilityActivated = false;
        Debug.Log("Deactivate mini driver ability");
    }

    [PunRPC]
    protected virtual void ActivationEffects_RPC()
    {
        GetComponent<AudioSource>().PlayOneShot(activateAudioClip);
    }





    public override void Fire()
    {
        if (CanUseAbility())
        {
            currentCooldown = cooldown;
            timeSinceLastFire = 0;
            Debug.Log("Base driver ability fire");
        }
    }

    public override void CeaseFire()
    {
        base.CeaseFire();
    }
}
