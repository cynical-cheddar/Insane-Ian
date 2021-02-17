using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Photon.Pun;
using UnityEngine;

public class WeaponTestFireController : MonoBehaviour
{
    public PhotonView gunnerPhotonView;
    Transform cam;
    public Weapon weapon;
    public Transform barrelTransform;

    public Collider[] _colliders;
    private void Start()
    {
        cam = Camera.main.transform;
        _colliders = transform.root.gameObject.GetComponentsInChildren<Collider>();
    }

    protected Transform FindClosestHitObject(Ray ray, out Vector3 hitPoint)
    {
        
        RaycastHit[] hits = Physics.RaycastAll(ray);
        _colliders = transform.root.gameObject.GetComponentsInChildren<Collider>();
        Transform closestHit = null;
        float distance = 0;
        hitPoint = Vector3.zero;

        foreach (RaycastHit hit in hits)
        {
            if (hit.transform.root != this.transform && (closestHit == null || hit.distance < distance) && !_colliders.Contains(hit.collider))
            {
                // We have hit something that is:
                // a) not us
                // b) the first thing we hit (that is not us)
                // c) or, if not b, is at least closer than the previous closest thing

                closestHit = hit.transform;
                distance = hit.distance;
                hitPoint = hit.point;
                
            }
        }

        // closestHit is now either still null (i.e. we hit nothing) OR it contains the closest thing that is a valid thing to hit

        return closestHit;

    }
    protected void LateUpdate()
    {
        if (Input.GetMouseButton(0))
        {
            if (gunnerPhotonView.IsMine)
            {
                Ray ray = new Ray(cam.position, cam.forward); 
                RaycastHit hit; //From camera to hitpoint, not as curent
                Transform hitTransform;
                Vector3 hitVector;
                hitTransform = FindClosestHitObject(ray, out hitVector);
                Physics.Raycast(ray.origin, ray.direction, out hit, 999);
                Vector3 hp;
                
                if (hitTransform == null)
                {
                    hp = cam.position + (cam.forward * 1500f);
                    Debug.Log("Null hit on targeting");
                }
                else
                {
                    hp = hitVector;
                    Debug.Log("HitTransform:" + hitTransform + " hitVector: " + hitVector);
                }
                
                weapon.Fire(hp);
                
            }
        }
    }
}
