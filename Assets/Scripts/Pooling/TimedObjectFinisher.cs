using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimedObjectFinisher : MonoBehaviour
{
    private float time = 5f;
    // Start is called before the first frame update
    private void OnEnable()
    {
        GetComponent<PooledObject>().Finish(time);
    }
}
