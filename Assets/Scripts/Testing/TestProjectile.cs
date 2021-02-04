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
        transform.position += transform.forward * 0.1f;
    }

    void kill() {
        Destroy(gameObject);
    }

    private void OnTriggerEnter(Collider other) {
        other.gameObject.GetComponent<Squishing>().ExplodeMeshAt(transform.position - transform.forward * 0.3f, 0.3f, true);
        Destroy(gameObject);
    }
}
