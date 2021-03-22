using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UiCanvasBehaviour : MonoBehaviour
{
    public GameObject ControlNotifier;

    // Start is called before the first frame update
    void Start() {
        ControlNotifier.SetActive(false); // Not a good fix, but I think that TMPro has a bug.
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
