using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurretController : MonoBehaviour
{
    private TurretTarget turretTarget;
    private TurretFollowTarget turretFollowTarget;
    private GameObject defaultTarget;
    private GameObject targeter;

    void Start() {
        turretTarget = GetComponentInChildren<TurretTarget>();
        turretFollowTarget = GetComponent<TurretFollowTarget>();
        defaultTarget = turretFollowTarget.target;
    }

    public float ChangeTargetYaw(float change) {
        turretTarget.yaw += change;
        return turretTarget.yaw;
    }

    public void SetTargetYaw(float yaw) {
        turretTarget.yaw = yaw;
    }

    public float ChangeTargetPitch(float change) {
        turretTarget.pitch += change;
        return turretTarget.pitch;
    }

    public void SetTargetPitch(float pitch) {
        turretTarget.pitch = pitch;
    }

    public void SetTarget(GameObject target) {
        turretFollowTarget.target = target;
    }

    public void ResetTarget() {
        turretFollowTarget.target = defaultTarget;
    }
}
