using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurretBody : MonoBehaviour
{
    private Transform turretTransform;

    void Start() {

    }

    public void SetupParents() {
       // turretTransform = transform.parent;
      //  transform.parent = transform.parent.parent;
    }
    
    public void UpdateAngle() {
        transform.localRotation = Quaternion.Euler(0, turretTransform.localEulerAngles.y, turretTransform.localEulerAngles.z);
    }
}
