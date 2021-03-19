using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialBehaviour : MonoBehaviour
{
    public KeyCode dismissKey;

    // Update is called once per frame
    void Update() {
        if (Input.GetKeyDown(dismissKey)) {
            Invoke(nameof(Deactivate), 1.75f);
        }
    }

    void Deactivate() {
        gameObject.SetActive(false);
    }
}
