using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PhysX;

public class MapSelfRighter : MonoBehaviour, ICollisionStayEvent
{

    public bool requiresData { get { return true; } }

    public void CollisionStay() {

    }
    
    float timer = 0f;

    public void CollisionStay(PhysXCollision col) {
     //   Debug.LogError("resetting pos up " + col.gameObject + " transform is: " + col.transform.root);
        timer += Time.unscaledDeltaTime;


        if(col.gameObject.CompareTag("Player") && timer > 0.8f){
            timer = 0f;
            Transform car = col.transform.root;
            car.transform.position = car.transform.position + Vector3.up * 3;
            car.GetComponent<PhysXBody>().position = car.transform.position + Vector3.up * 3;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
