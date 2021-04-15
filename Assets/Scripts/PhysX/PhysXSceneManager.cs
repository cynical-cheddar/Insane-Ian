using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using PhysX;
using AOT;

public class PhysXSceneManager : MonoBehaviour
{
    private static IntPtr scene = IntPtr.Zero;

    private static Dictionary<IntPtr, PhysXBody> bodies = new Dictionary<IntPtr, PhysXBody>();
    private List<PhysXBody> preRegisteredBodies = new List<PhysXBody>();

    private static List<PhysXCollision> ongoingCollisions = new List<PhysXCollision>();
    private static List<PhysXTrigger> ongoingTriggers = new List<PhysXTrigger>();

    public PhysicMaterial defaultMaterial;

    void Awake() {
        if (scene != IntPtr.Zero) Debug.LogError("PhysX already set up. There may be multiple scene managers.");

        PhysXLib.SetupPhysX();
        PhysXLib.RegisterCollisionCallback(AddCollision);
        PhysXLib.RegisterTriggerCallback(AddTrigger);
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
        bodies.Clear();
    }

    void Setup() {
        scene = PhysXLib.CreateScene(new PhysXVec3(Physics.gravity));
        foreach (PhysXBody body in preRegisteredBodies) {
            AddActor(body);
        }
        preRegisteredBodies.Clear();
    }

    public void AddActor(PhysXBody body) {
        if (scene == IntPtr.Zero) {
            preRegisteredBodies.Add(body);
        }
        else {
            body.Setup();
            bodies.Add(body.physXBody, body);
            PhysXLib.AddActorToScene(scene, body.physXBody);
            body.PostSceneInsertionSetup();
        }
    }

    public void RemoveActor(PhysXBody body) {
        bodies.Remove(body.physXBody);
    }

    public static PhysXBody GetBodyFromPointer(IntPtr pointer) {
        return bodies[pointer];
    }

    public void Simulate() {
        PhysXLib.StepPhysics(scene, Time.fixedDeltaTime);

        foreach (PhysXBody body in bodies.Values) {
            body.UpdatePositionAndVelocity();
        }

        foreach (PhysXCollision collision in PhysXSceneManager.ongoingCollisions) {
            collision.PopulateWithUnityObjects(bodies);
            PhysXBody body = null;
            if (bodies.TryGetValue(collision.self, out body)) body.FireCollisionEvents(collision);
            PhysXCollision.ReleaseCollision(collision);
        }
        PhysXSceneManager.ongoingCollisions.Clear();

        foreach (PhysXTrigger trigger in PhysXSceneManager.ongoingTriggers) {
            Debug.Log("triggered");
            trigger.PopulateWithUnityObjects(bodies);
            PhysXBody body = null;
            if (bodies.TryGetValue(trigger.self, out body)) body.FireTriggerEvents(trigger);
            PhysXTrigger.ReleaseTrigger(trigger);
        }
        PhysXSceneManager.ongoingTriggers.Clear();
    }

    [MonoPInvokeCallback(typeof(PhysXLib.CollisionCallback))]
    public static void AddCollision(IntPtr pairHeader, IntPtr pairs, int pairCount, IntPtr self, bool isEnter, bool isStay, bool isExit) {
        PhysXCollision collision = PhysXCollision.GetCollision();
        collision.FromPhysXInternalCollision(pairHeader, pairs, pairCount, self, isEnter, isStay, isExit);
        ongoingCollisions.Add(collision);
    }

    [MonoPInvokeCallback(typeof(PhysXLib.TriggerCallback))]
    public static void AddTrigger(IntPtr other, IntPtr otherShape, IntPtr self, bool isEnter, bool isExit) {
        PhysXTrigger trigger = PhysXTrigger.GetTrigger();
        trigger.FromPhysXInternalTrigger(other, otherShape, self, isEnter, isExit);
        ongoingTriggers.Add(trigger);
    }

    public static bool FireRaycast(PhysXVec3 origin, PhysXVec3 direction, float distance, IntPtr raycastHit) {
        return PhysXLib.FireRaycast(scene, origin, direction, distance, raycastHit);
    }
}
