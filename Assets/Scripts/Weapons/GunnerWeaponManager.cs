using System;
using System.Collections;
using System.Collections.Generic;
using Gamestate;
using Photon.Pun;
using UnityEngine;


public class GunnerWeaponManager : MonoBehaviour
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
        gunnerUltimateProgress = 0;
        foreach (WeaponControlGroup wcg in weaponControlGroups.weaponControlGroupList)
        {
            foreach (Weapon w in wcg.weapons)
            {
                w.ResetWeaponToDefaults();
            }
        }
    }
    
    public void AdjustGunnerUltimateProgress(float amt)
    {
        gunnerUltimateProgress += amt;
        if (gunnerUltimateProgress < 0) gunnerUltimateProgress = 0;
        if (gunnerUltimateProgress > maxGunnerUltimateProgress) gunnerUltimateProgress = 100;
        
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
        SelectFirst();
        myPhotonView = GetComponent<PhotonView>();
        StartCoroutine(nameof(LateStart));
        ultimateUiManager = FindObjectOfType<UltimateUiManager>();
    }

    IEnumerator LateStart()
    {
        yield return new WaitForSeconds(0.1f);
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

    public void SelectFirst()
    {
        // select the first control group
        SelectWeaponGroup(weaponControlGroups.weaponControlGroupList[0]);
    }

    public void SelectWeaponGroup(WeaponControlGroup group)
    {
        currentWeaponControlGroup = group;
        // foreach weapon in the group, activate the weapon
        foreach (Weapon w in currentWeaponControlGroup.weapons)
        {
            if(w!=null) w.ActivateWeapon();
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
                w.SetupWeapon();
            }
        }
    }
}
