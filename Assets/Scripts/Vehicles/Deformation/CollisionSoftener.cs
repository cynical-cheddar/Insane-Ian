using UnityEngine;
using PhysX;

public class CollisionSoftener : MonoBehaviour, ICollisionStayEvent
{
    public bool requiresData { get { return true; } }

    public float penForUnitImpulseScale = 0.5f;
    public float maxImpulseScale = 1.5f;
    // public float penForMaxImpulseScale = 0.8f;
    //public int penExp = 3;
    public float velocityScale = 5;
    public float velocityOffset = 5;
    public float maxPenExp = 3;

    private PhysXRigidBody rigidBody;
    private new PhysXCollider collider;

    private float velocity = 0;

    void Start() {
        rigidBody = GetComponentInParent<PhysXRigidBody>();
        collider = GetComponent<PhysXCollider>();
    }

    void FixedUpdate() {
        velocity = rigidBody.velocity.magnitude;
        velocity -= velocityOffset;
        if (velocity < 0) velocity = 0;
    }

    public void CollisionStay() {}

    public void CollisionStay(PhysXCollision collision) {
        // float penCoefficientA = (penForMaxImpulseScale - penForUnitImpulseScale * maxImpulseScale) / (penForUnitImpulseScale * penForUnitImpulseScale * penForMaxImpulseScale - penForUnitImpulseScale * penForMaxImpulseScale * penForMaxImpulseScale);
        // float penCoefficientB = maxImpulseScale / penForMaxImpulseScale - penCoefficientA * penForMaxImpulseScale;
        
        float penExp = velocityScale / velocity;
        if (penExp > maxPenExp) penExp = maxPenExp;

        float penCoefficient = 1 / Mathf.Pow(penForUnitImpulseScale, penExp);

        for (int i = 0; i < collision.contactCount; i++) {
            PhysXContactPoint contactPoint = collision.GetContact(i);

            if (contactPoint.ownShape == collider.shape) {
                float penetration = -contactPoint.separation;
                if (penetration < 0) penetration = 0;

                //Debug.Log("penetration: " + penetration);

                Vector3 impulse = contactPoint.impulse;
                // float impulseScale = penCoefficientA * penetration * penetration + penCoefficientB * penetration;
                float impulseScale = penCoefficient * Mathf.Pow(penetration, penExp);
                if (impulseScale > maxImpulseScale) impulseScale = maxImpulseScale;
                impulse *= impulseScale;

                rigidBody.AddGhostImpulse(impulse, contactPoint.point);
            }
        }
    }
}