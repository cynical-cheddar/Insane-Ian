using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayIntro : MonoBehaviour
{
    public static bool hasplayed = false;
    // Start is called before the first frame update
    void Start()
    {
        if(!hasplayed){
            GetComponent<AudioSource>().Play();
            hasplayed = true;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
