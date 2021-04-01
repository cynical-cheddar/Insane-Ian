using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using PhysX;

public class PhysXSceneManager : MonoBehaviour
{
    private IntPtr scene = IntPtr.Zero;

    private List<PhysXRigidBody> rigidBodies = new List<PhysXRigidBody>();
    private List<PhysXRigidBody> preRegisteredRigidBodies = new List<PhysXRigidBody>();

    public PhysicMaterial defaultMaterial;

    void Awake() {
        PhysXLib.SetupPhysX();
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.sceneUnloaded += OnSceneUnloaded;
        PhysXSceneSimulator.AddScene(this);
    }

    void OnSceneLoaded(Scene s, LoadSceneMode mode) {
        Debug.Log("loaded");
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
            rigidBodies.Add(rigidBody);
            PhysXLib.AddActorToScene(scene, rigidBody.physXDynamicRigidBody);
        }
    }

    public void Simulate() {
        PhysXLib.StepPhysics(scene, Time.fixedDeltaTime);

        foreach (PhysXRigidBody rigidBody in rigidBodies) {
            rigidBody.UpdatePositionAndVelocity();
        }
    }
}
