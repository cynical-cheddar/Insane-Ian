using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Profiling;
using PhysX;
using AOT;

public class PhysXSceneManager : MonoBehaviour
{
    static readonly ProfilerMarker collisionCallbackMarker = new ProfilerMarker("CollisionCallbacks");

    private static bool sceneManagerExists = false;

    private static IntPtr scene = IntPtr.Zero;

    private static Dictionary<long, PhysXBody> bodies = new Dictionary<long, PhysXBody>();
    private List<PhysXBody> preRegisteredBodies = new List<PhysXBody>();

    private static List<PhysXCollision> ongoingCollisions = new List<PhysXCollision>();
    private static List<PhysXTrigger> ongoingTriggers = new List<PhysXTrigger>();

    public PhysicMaterial defaultMaterial;

    public bool doPhysics = true;

    void Awake() {
        if (sceneManagerExists) {
            Debug.Log("PhysX Scene Manager already exists");
            Destroy(gameObject);
            return;
        }

        sceneManagerExists = true;

        GetComponent<PhysicsToggle>().Setup();

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
        PhysXLib.DestroyScene(scene);
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
            bodies.Add(body.physXBody.ToInt64(), body);
            PhysXLib.AddActorToScene(scene, body.physXBody);
            body.PostSceneInsertionSetup();
        }
    }

    public void RemoveActor(PhysXBody body) {
        bodies.Remove(body.physXBody.ToInt64());
    }

    public static PhysXBody GetBodyFromPointer(IntPtr pointer) {
        return bodies[pointer.ToInt64()];
    }

    public void Simulate() {
        if (doPhysics) {
            PhysXLib.StepPhysics(scene, Time.fixedDeltaTime);

            foreach (PhysXBody body in bodies.Values) {
                body.UpdatePositionAndVelocity();
            }

            collisionCallbackMarker.Begin();      
            foreach (PhysXCollision collision in PhysXSceneManager.ongoingCollisions) {
                collision.PopulateWithUnityObjects(bodies);
                PhysXBody body = null;
                if (bodies.TryGetValue(collision.self.ToInt64(), out body)) {
                    try {
                        body.FireCollisionEvents(collision);
                    }
                    catch (Exception e) {
                        Debug.LogError("Exception: " + e.Message + "\n" + e.StackTrace);
                    }
                }
                PhysXCollision.ReleaseCollision(collision);
            }
            PhysXSceneManager.ongoingCollisions.Clear();

            foreach (PhysXTrigger trigger in PhysXSceneManager.ongoingTriggers) {
                trigger.PopulateWithUnityObjects(bodies);
                PhysXBody body = null;
                if (bodies.TryGetValue(trigger.self.ToInt64(), out body)) {
                    try {
                        body.FireTriggerEvents(trigger);
                    }
                    catch (Exception e) {
                        Debug.LogError("Exception: " + e.Message + "\n" + e.StackTrace);
                    }
                }
                PhysXTrigger.ReleaseTrigger(trigger);
            }
            PhysXSceneManager.ongoingTriggers.Clear();
            collisionCallbackMarker.End();

            PhysXLib.StepGhostPhysics(scene, Time.fixedDeltaTime);
        }
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

    public static bool FireRaycastFiltered(PhysXVec3 origin, PhysXVec3 direction, float distance, IntPtr raycastHit, uint w0, uint w1, uint w2, uint w3) {
        return PhysXLib.FireRaycastFiltered(scene, origin, direction, distance, raycastHit, w0, w1, w2, w3);
    }
}
