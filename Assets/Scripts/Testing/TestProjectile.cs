using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestProjectile : MonoBehaviour
{
    // Start is called before the first frame update
    void Start() {
        Invoke("kill", 4f);
    }

    // Update is called once per frame
    void Update() {
        transform.position += transform.forward * 0.3f;
    }

    void kill() {
        Destroy(gameObject);
    }
}
