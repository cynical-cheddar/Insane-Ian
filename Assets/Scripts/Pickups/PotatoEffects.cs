using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class PotatoEffects : MonoBehaviour
{
    // Start is called before the first frame update

    public AnimationClip activateAnimation;
    public AnimationClip deactivateAnimation;

    private Animation animation;

    public AudioClip activationAudio;
    public AudioClip deactivationAudio;
    public AudioClip activatedLoop;

    public AudioSource activationSource;
    public AudioSource activatedLoopSource;

    public LightningBoltScript lightning1;
    public LightningBoltScript lightning2;

    public GameObject teslaWreck;

    public List<Transform> teslaCoils;

    private int myDriverId = 0;
    private int myGunnerId = 0;
    private Shader regShader;
    private Shader hpShader;
    public Renderer rend;
    private List<Material> mats = new List<Material>();

    public void Setup()
    {
        animation = GetComponent<Animation>();
        // Invoke(nameof(DelayedStart), 1f);
        regShader = Shader.Find("Shader No Border");
        Debug.Log("regShader.name: " + regShader.name);
        hpShader = Shader.Find("Unlit/Hot Potato Shader");
        Debug.Log("hpShader.name: " + hpShader.name);
        rend.GetMaterials(mats);
        NetworkPlayerVehicle npv = GetComponentInParent<NetworkPlayerVehicle>();
        myDriverId = npv.GetDriverID();
        myGunnerId = npv.GetGunnerID();
    }

    void DelayedStart()
    {
        NetworkPlayerVehicle npv = GetComponentInParent<NetworkPlayerVehicle>();
        myDriverId = npv.GetDriverID();
        myGunnerId = npv.GetGunnerID();
    }
    
    

    public void ActivatePotatoEffects(int driverId, int gunnerId)
    {
        animation.clip = activateAnimation;
        animation.Play();
        activationSource.PlayOneShot(activationAudio);
        activatedLoopSource.clip = activatedLoop;
        activatedLoopSource.Play();
        lightning1.enabled = true;
        lightning1.GetComponent<LineRenderer>().enabled = true;
        lightning2.enabled = true;
        lightning2.GetComponent<LineRenderer>().enabled = true;


        if (driverId == PhotonNetwork.LocalPlayer.ActorNumber || gunnerId == PhotonNetwork.LocalPlayer.ActorNumber)
        {
            PotatoUi pui = FindObjectOfType<PotatoUi>();
            pui.SetText(true);
        } else
        {
            foreach (var mat in mats)
            {
                mat.shader = hpShader;
            }
            

        }
    }

    public void DeactivatePotatoEffects(int driverId, int gunnerId)
    {
        foreach (Transform t in teslaCoils)
        {
            GameObject wreck = teslaWreck;
            wreck.transform.localScale = t.lossyScale;
            GameObject a = Instantiate(wreck, t.position, t.rotation);
            
        }
        animation.clip = deactivateAnimation;
        animation.Play();
        activatedLoopSource.Stop();
        activationSource.PlayOneShot(deactivationAudio);
        
        lightning1.enabled = false;
        lightning1.GetComponent<LineRenderer>().enabled = false;
        
        lightning2.enabled = false;
        lightning2.GetComponent<LineRenderer>().enabled = false;



        if (driverId == PhotonNetwork.LocalPlayer.ActorNumber || gunnerId == PhotonNetwork.LocalPlayer.ActorNumber)
        {
            PotatoUi pui = FindObjectOfType<PotatoUi>();
            pui.SetText(false);
        } else
        {
            foreach (var mat in mats)
            {
                mat.shader = regShader;
            }
        }
    }
}
