using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialBehaviour : MonoBehaviour
{
    public KeyCode dismissKey;
    public int tutorialNumber;
    public GameObject effect;

    TutorialManager tutorialManager;
    

    private void Start() {
        tutorialManager = FindObjectOfType<TutorialManager>();
    }

    // Update is called once per frame
    void Update() {
        if (tutorialManager.tutorials[tutorialNumber]) {
            effect.SetActive(true);
            if (Input.GetKeyDown(dismissKey)) {
                tutorialManager.tutorials[tutorialNumber] = false;
                Invoke(nameof(Deactivate), 1.75f);
            }
        } 
    }

    void Deactivate() {
        effect.SetActive(false);
    }
}
