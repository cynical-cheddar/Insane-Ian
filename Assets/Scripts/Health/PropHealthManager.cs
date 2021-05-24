//Prop health manager manages props which have health and then break into anothyer prefab upon death.

using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using PhysX;
public class PropHealthManager : MonoBehaviour, ICollisionEnterEvent
{

    public bool requiresData { get { return true; } }
    public GameObject wreckPrefab; //The prefab an object will break into

   // public bool requiresData { get { return true; } }

    public void CollisionEnter() {}

    public void CollisionEnter(PhysXCollision col){ //When hit at a certain velocity, the object will call Die()
        if (col.rigidBody != null && col.rigidBody.velocity.magnitude > 3) {
            Die();
        }
    }

    protected void Die() { //Is called when an object breaks, insstantiates new prefab and destroys old gameObject
        if(wreckPrefab!=null){
            Instantiate(wreckPrefab, transform.position, transform.rotation);
        }
        Destroy(gameObject);
    }
}
