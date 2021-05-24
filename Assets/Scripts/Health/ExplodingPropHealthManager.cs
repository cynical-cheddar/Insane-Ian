//Health manager for props which explode on death

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using PhysX;
public class ExplodingPropHealthManager : MonoBehaviour, ICollisionEnterEvent
{

    public bool requiresData { get { return true; } }
    public GameObject wreckPrefab; //Prefab to be instantiated upon death

    public void CollisionEnter() {}

    public void CollisionEnter(PhysXCollision col){ //Dies if collision of great enough velocity occurs
        if (col.rigidBody != null && col.rigidBody.velocity.magnitude > 3) {
            Die();
        }
    }

    protected void Die() {
        if(wreckPrefab!=null){
            Instantiate(wreckPrefab, transform.position, transform.rotation); //Explosive prefab
        }
        Destroy(gameObject); //Destroys dead game object
    }
}
