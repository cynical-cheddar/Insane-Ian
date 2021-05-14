using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using PhysX;
public class PropHealthManager : MonoBehaviour, ICollisionEnterEvent
{

    public bool requiresData { get { return true; } }
    public GameObject wreckPrefab;

   // public bool requiresData { get { return true; } }

    public void CollisionEnter() {}

    public void CollisionEnter(PhysXCollision col){
        if (col.rigidBody != null && col.rigidBody.velocity.magnitude > 3) {
            Die();
        }
    }

    protected void Die() {
        if(wreckPrefab!=null){
            Instantiate(wreckPrefab, transform.position, transform.rotation);
        }
        Destroy(gameObject);
    }
}
