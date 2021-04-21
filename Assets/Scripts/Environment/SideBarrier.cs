using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SideBarrier : MonoBehaviour
{
    private Collider lastCollider;

    public float speed = 10f;

    // private void Start()
    // {
    //     lastCollider = GetComponent<Collider>();
    // }

    // private void OnTriggerStay(Collider other)
    // {

    //         if (other.transform.root.CompareTag("Player"))
    //         {
    //             // see if the forward dot product is closer to left or right, lerp em towards that
    //             float leftAngle = Vector3.Angle(-transform.right, other.transform.forward);
    //             float rightAngle = Vector3.Angle(transform.right, other.transform.forward);
    //             if (leftAngle < rightAngle)
    //             {

    //                 Vector3 eulers = other.transform.root.eulerAngles;
    //                 Vector3 myEulers = transform.root.eulerAngles;
    //                 Quaternion targetRotation = Quaternion.Euler(myEulers.x+90, eulers.y, eulers.z);
                    
    //                 var step = speed * Time.deltaTime;

    //                 // Rotate our transform a step closer to the target's.
    //                 other.transform.root.rotation = Quaternion.RotateTowards(other.transform.rotation, targetRotation , step);
    //             }
    //             if (rightAngle < leftAngle)
    //             {

    //                 Vector3 eulers = other.transform.root.eulerAngles;
    //                 Vector3 myEulers = transform.root.eulerAngles;
                    
    //                 Quaternion targetRotation = Quaternion.Euler(myEulers.x-90, eulers.y, eulers.z);
                    
    //                 var step = speed * Time.deltaTime;

    //                 // Rotate our transform a step closer to the target's.
    //                 other.transform.root.rotation = Quaternion.RotateTowards(other.transform.rotation, targetRotation , step);
    //             }
    //         }
            
    // }
}
