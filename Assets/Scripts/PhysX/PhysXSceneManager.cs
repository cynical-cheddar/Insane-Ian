using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using PhysX;
using AOT;

public class PhysXSceneManager : MonoBehaviour
{
    private IntPtr scene = IntPtr.Zero;

    private Dictionary<IntPtr, PhysXRigidBody> rigidBodies = new Dictionary<IntPtr, PhysXRigidBody>();
    private List<PhysXRigidBody> preRegisteredRigidBodies = new List<PhysXRigidBody>();

    private static List<PhysXCollision> ongoingCollisions = new List<PhysXCollision>();

    public PhysicMaterial defaultMaterial;

    void Awake() {
        PhysXLib.SetupPhysX();
        PhysXLib.RegisterCollisionCallback(AddCollision);
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.sceneUnloaded += OnSceneUnloaded;
        PhysXSceneSimulator.AddScene(this);
    }

    void OnSceneLoaded(Scene s, LoadSceneMode mode) {
        if (scene == IntPtr.Zero) Setup();
    }

    void OnSceneUnloaded(Scene s) {
        Debug.LogWarning("unloaded     TODO: cleanup physx on unload");
        scene = IntPtr.Zero;
        rigidBodies.Clear();
    }

    void Setup() {
        scene = PhysXLib.CreateScene(new PhysXVec3(Physics.gravity));
        foreach (PhysXRigidBody rigidBody in preRegisteredRigidBodies) {
            AddActor(rigidBody);
        }
        preRegisteredRigidBodies.Clear();
    }

    public void AddActor(PhysXRigidBody rigidBody) {
        if (scene == IntPtr.Zero) {
            preRegisteredRigidBodies.Add(rigidBody);
        }
        else {
            rigidBody.Setup();
            rigidBodies.Add(rigidBody.physXDynamicRigidBody, rigidBody);
            PhysXLib.AddActorToScene(scene, rigidBody.physXDynamicRigidBody);
        }
    }

    public void Simulate() {
        PhysXLib.StepPhysics(scene, Time.fixedDeltaTime);

        foreach (PhysXRigidBody rigidBody in rigidBodies.Values) {
            rigidBody.UpdatePositionAndVelocity();
        }

        foreach (PhysXCollision collision in PhysXSceneManager.ongoingCollisions) {
            collision.PopulateWithUnityObjects(rigidBodies);
            rigidBodies[collision.self].FireCollisionEvents(collision);
            PhysXCollision.ReleaseCollision(collision);
        }
        PhysXSceneManager.ongoingCollisions.Clear();
    }

    [MonoPInvokeCallback(typeof(PhysXLib.CollisionCallback))]
    public static void AddCollision(IntPtr pairHeader, IntPtr pairs, int pairCount, IntPtr self, bool isEnter, bool isStay, bool isExit) {
        PhysXCollision collision = PhysXCollision.GetCollision();
        collision.FromPhysXInternalCollision(pairHeader, pairs, pairCount, self, isEnter, isStay, isExit);
        ongoingCollisions.Add(collision);
    } 
}
