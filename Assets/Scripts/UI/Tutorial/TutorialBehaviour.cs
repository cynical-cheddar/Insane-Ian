using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Gamestate;
using Photon.Pun;

public class TutorialBehaviour : MonoBehaviour
{
    public KeyCode dismissKey;
    public int tutorialNumber;
    public GameObject effect;
    public bool requireDismissal = true;

    TutorialManager tutorialManager;
    NetworkPlayerVehicle npv;

    private void Start() {
        tutorialManager = FindObjectOfType<TutorialManager>();
        npv = GetComponentInParent<NetworkPlayerVehicle>();
        StartCoroutine(nameof(LateStart));
    }

    IEnumerator LateStart() {
        yield return new WaitForEndOfFrame();
        if (tutorialNumber == 1) { // Gunner tutorials
            if (npv.botGunner) gameObject.SetActive(false);
        } else if (tutorialNumber == 0 || tutorialNumber == 2) { // Driver tutorials
            if (npv.botDriver) gameObject.SetActive(false);
        } else if (tutorialNumber == 3) { // Shared tutorials
            if (npv.botDriver && npv.botGunner) gameObject.SetActive(false);
            else if (npv.GetGunnerID() != PhotonNetwork.LocalPlayer.ActorNumber && npv.GetDriverID() != PhotonNetwork.LocalPlayer.ActorNumber) gameObject.SetActive(false);
        }
    }

    // Update is called once per frame
    void Update() {
        if (tutorialManager != null) {
            if (tutorialManager.tutorials[tutorialNumber]) {
                effect.SetActive(true);
                if (Input.GetKeyDown(dismissKey)) {
                    tutorialManager.tutorials[tutorialNumber] = false;
                    Invoke(nameof(Deactivate), 1.75f);
                }
                if (!requireDismissal) {
                    tutorialManager.tutorials[tutorialNumber] = false;
                    Invoke(nameof(Deactivate), 6f);
                }
            }
        }
    }

    void Deactivate() {
        effect.SetActive(false);
    }
}
