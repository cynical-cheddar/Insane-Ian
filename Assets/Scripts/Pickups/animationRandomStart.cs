using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class animationRandomStart : MonoBehaviour
{
    // Start is called before the first frame update
    public float range = 0.2f;
    void Start()
    {
        float time = Random.Range(0, range);
        Invoke(nameof(PlayAni), time);
    }

    void PlayAni()
    {
        GetComponent<Animation>().Play();
    }


}
