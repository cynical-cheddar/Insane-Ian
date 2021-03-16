using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Equipment : MonoBehaviour
{
    // Start is called before the first frame update
    public virtual void Fire(Vector3 targetPoint){
      //  Debug.Log("Base equipment fire - don't use this class");
    }
    
    public virtual void Fire(){
      //  Debug.Log("Base equipment fire - don't use this class");
    }

    public virtual void CeaseFire()
    {
      //  Debug.Log("Base equipment CeaseFire - don't use this class");
    }
    
}
