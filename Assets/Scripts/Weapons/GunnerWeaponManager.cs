using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunnerWeaponManager : MonoBehaviour
{

    
    // this script is essentially an interface to fire the weapons
    // weapons have been assigned into groups, in case we want to bind multiple weapons to a single fire command


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

    void Start()
    {
        Invoke(nameof(SelectFirst), 0.4f);
        //SelectFirst();
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
            w.ActivateWeapon();
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
}
