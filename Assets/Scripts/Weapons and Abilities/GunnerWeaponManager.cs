using System;
using System.Collections;
using System.Collections.Generic;
using Gamestate;
using Photon.Pun;
using UnityEngine;


public class GunnerWeaponManager : MonoBehaviourPun, IPunObservable
{
    private UltimateUiManager ultimateUiManager;
    
    // this script is essentially an interface to fire the weapons
    // weapons have been assigned into groups, in case we want to bind multiple weapons to a single fire command
    public float gunnerUltimateProgress = 0f;
    public float maxGunnerUltimateProgress = 100f;
    private PhotonView myPhotonView;
    private int driverId = 0;
    private int gunnerId = 0;
    private bool driverBot = false;
    private bool gunnerBot = false;

    private bool hasSelectedWeaponGroup = false;

    public int ultimateGroupWeaponIndex = 1;

    public float gunnerDamageDealt = 0;
    
    public void UpdateDamageDealt(Weapon.WeaponDamageDetails hitDetails)
    {
        // only let the gunner update this 

        if (hitDetails.sourcePlayerId == gunnerId)
        {
            gunnerDamageDealt += hitDetails.damage;
        }
    }
    
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(gunnerDamageDealt);
        }
        else
        {
            gunnerDamageDealt = (float)stream.ReceiveNext();
        }
    }
    
    
    
    
    [Serializable]
    public struct WeaponControlGroups
    {
        public List<WeaponControlGroup> weaponControlGroupList;

        public WeaponControlGroups(List<WeaponControlGroup> wcgs)
        {
            weaponControlGroupList = wcgs;
        }
    }
    [Serializable]

    public struct WeaponControlGroup
    {
        public bool isUltimate;
        public List<Weapon> weapons;

        public WeaponControlGroup(bool setUltimate, List<Weapon> weaponGroupList)
        {
            isUltimate = setUltimate;
            weapons = weaponGroupList;
        }
    }

    [Header("Link fire command to all these scripts")]
    [SerializeField]public WeaponControlGroups weaponControlGroups;
    WeaponControlGroup currentWeaponControlGroup;

    public void Reset()
    {
        myPhotonView.RPC(nameof(Reset_RPC), RpcTarget.All);
    }

    [PunRPC]
    void Reset_RPC()
    {
        gunnerUltimateProgress = 0;
        SetGunnerUltimateProgress(0);
        SelectFirst();
        
        
        foreach (WeaponControlGroup wcg in weaponControlGroups.weaponControlGroupList)
        {
            foreach (Weapon w in wcg.weapons)
            {
                w.ResetWeaponToDefaults();
            }
        }
    }

    void ResetWeaponGroup(WeaponControlGroup wcg)
    {
        foreach (Weapon w in wcg.weapons)
        {
            w.ResetWeaponToDefaults();
        }
    }
    
    public void AdjustGunnerUltimateProgress(float amt)
    {
        gunnerUltimateProgress += amt;
        if (gunnerUltimateProgress < 0) gunnerUltimateProgress = 0;
        if (gunnerUltimateProgress > maxGunnerUltimateProgress) gunnerUltimateProgress = maxGunnerUltimateProgress;
        
        myPhotonView.RPC(nameof(SetUltimateProgress_RPC), RpcTarget.All, gunnerUltimateProgress);
    }

    public void SetGunnerUltimateProgress(float amt)
    {
        gunnerUltimateProgress = amt;
        if (gunnerUltimateProgress < 0) gunnerUltimateProgress = 0;
        if (gunnerUltimateProgress > maxGunnerUltimateProgress) gunnerUltimateProgress = maxGunnerUltimateProgress;
        
        myPhotonView.RPC(nameof(SetUltimateProgress_RPC), RpcTarget.All, gunnerUltimateProgress);
    }
   

    [PunRPC]
    void SetUltimateProgress_RPC(float amt)
    {
        gunnerUltimateProgress = amt;
        
        // now update the current hud with ultimate progress, if we are a real player
        if (driverId == PhotonNetwork.LocalPlayer.ActorNumber || gunnerId == PhotonNetwork.LocalPlayer.ActorNumber)
        {
            if(ultimateUiManager!=null) ultimateUiManager.UpdateGunnerBar(gunnerUltimateProgress, maxGunnerUltimateProgress);
           
        }
    }

    private void Start()
    {
        Invoke(nameof(StartWeaponManager), 0.5f);
    }

    public void StartWeaponManager()
    {
        myPhotonView = GetComponent<PhotonView>();
        
        ultimateUiManager = FindObjectOfType<UltimateUiManager>();
        if(ultimateUiManager!=null) ultimateUiManager.CacheRole();
        
        SelectFirst();
        SetupWeaponOwnerships();
        
        
        
        
        Invoke(nameof(SetupGunnerWeaponManager), 0.2f);
        
        
    }





    void SetupGunnerWeaponManager()
    {
        NetworkPlayerVehicle npv = GetComponentInParent<NetworkPlayerVehicle>();
        if (npv != null)
        {
            driverId = npv.GetDriverID();
            gunnerId = npv.GetGunnerID();
            if (driverId < 0) driverBot = true;
            if (gunnerId < 0) gunnerBot = true;
            AdjustGunnerUltimateProgress(0);
        }
    }

    [PunRPC]
    void SetUsingUltimate_RPC(bool set)
    {
        usingUltimate = set;
    }

    public void SelectFirst()
    {
        
        
        // select the first control group
        SelectWeaponGroup(weaponControlGroups.weaponControlGroupList[0]);
    }

    public void SelectUltimate()
    {
        if (gunnerUltimateProgress >= maxGunnerUltimateProgress)
        {
            usingUltimate = true;
            ResetWeaponGroup(weaponControlGroups.weaponControlGroupList[ultimateGroupWeaponIndex]);
            SelectWeaponGroup(weaponControlGroups.weaponControlGroupList[ultimateGroupWeaponIndex]);
        }
    }

    public bool usingUltimate = false;

    public void SelectWeaponGroup(WeaponControlGroup group)
    {
        
        // deselect current weapon group
       if(hasSelectedWeaponGroup) DeselectWeaponGroup(group);
        
        currentWeaponControlGroup = group;
        // foreach weapon in the group, activate the weapon
        if (group.isUltimate) myPhotonView.RPC(nameof(SetUsingUltimate_RPC), RpcTarget.All, true);
        else myPhotonView.RPC(nameof(SetUsingUltimate_RPC), RpcTarget.All, false);
        
        foreach (Weapon w in currentWeaponControlGroup.weapons)
        {
            if (w != null)
            {
                w.ActivateWeapon();
            }
            
        }

        hasSelectedWeaponGroup = true;
    }

    public void DeselectWeaponGroup(WeaponControlGroup group)
    {
        foreach (Weapon w in currentWeaponControlGroup.weapons)
        {
            if (w != null)
            {
                w.DeselectWeapon();
            }
            
        }
    }

    public bool CurrentWeaponGroupCanFire() {
        foreach (Weapon w in currentWeaponControlGroup.weapons) {
            if (w.CanFire()) return true;
        }
        return false;
    }

    public void FireCurrentWeaponGroup(Vector3 targetPos)
    {
        // get all weapons in the current weapon group and fire them at the targetPos
        foreach (Weapon w in currentWeaponControlGroup.weapons)
        {
            w.Fire(targetPos);
        }
    }

    public void ReloadCurrentWeaponGroup()
    {
        foreach (Weapon w in currentWeaponControlGroup.weapons)
        {
            w.ReloadSalvo();
        }
    }

    public void CeaseFireCurrentWeaponGroup()
    {
        foreach (Weapon w in currentWeaponControlGroup.weapons)
        {
            w.CeaseFire();
        }
    }

    public void SetupWeaponOwnerships()
    {
        foreach (WeaponControlGroup wcg in weaponControlGroups.weaponControlGroupList)
        {
            foreach (Weapon w in wcg.weapons)
            {
                if(w!=null) w.SetupWeapon();
            }
        }
    }
}
