using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class TeamNameBehaviour : MonoBehaviour
{
    public Transform textTransform;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        textTransform.rotation = Camera.main.transform.rotation;
    }
}
