using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class WeaponUi : MonoBehaviour
{
    public TextMeshProUGUI salvoText;
    public TextMeshProUGUI reserveAmmoText;
    public TextMeshProUGUI weaponNameText;
    public Text crosshair;

    // Start is called before the first frame update
    public void UpdateAmmo(int currentSalvo, int salvoSize, int reserveAmmo)
    {
        salvoText.text = currentSalvo.ToString() + "/" + salvoSize;
        reserveAmmoText.text = reserveAmmo.ToString();
    }

    private void Start()
    {
        SetCanvasVisibility(false);
    }

    public void SetCanvasVisibility(bool set)
    {
        if (crosshair != null) crosshair.gameObject.SetActive(set);
        CanvasGroup[] canvasRenderers = GetComponentsInChildren<CanvasGroup>();
        foreach (CanvasGroup cv in canvasRenderers)
        {
            
            if(set) cv.alpha = 1;
            if (!set) cv.alpha = 0;
        }
    }

    public void SetWeaponNameText(string weaponName)
    {
        weaponNameText.text = weaponName;
    }
}
