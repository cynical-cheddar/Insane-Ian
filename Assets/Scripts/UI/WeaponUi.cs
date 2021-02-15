using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WeaponUi : MonoBehaviour
{
    public Text salvoText;
    public Text reserveAmmoText;
    public Text weaponNameText;
    // Start is called before the first frame update
    public void UpdateAmmo(int currentSalvo, int salvoSize, int reserveAmmo)
    {
        salvoText.text = currentSalvo.ToString() + "/" + salvoSize;
        reserveAmmoText.text = reserveAmmo.ToString();
    }

    public void SetWeaponNameText(string weaponName)
    {
        weaponNameText.text = weaponName;
    }
}
