using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MakeTheMap : MonoBehaviour
{
   
   public string mapObjectColliderPrefabName;
   GameObject map;

   public void MakeMap(){
       GameObject go = Resources.Load(mapObjectColliderPrefabName) as GameObject;
       map = Instantiate(go, transform.position, transform.rotation);
   }
    
}
