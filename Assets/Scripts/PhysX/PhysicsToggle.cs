using System;
using System.Runtime.InteropServices;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.LowLevel;

using PhysX;

public class PhysicsToggle : MonoBehaviour
{
    private bool doingPhysics = false;
    private IntPtr physicsFunction = IntPtr.Zero;

    // Start is called before the first frame update
    public void Setup()
    {
        if (Application.IsPlaying(gameObject)) {
            DontDestroyOnLoad(gameObject);

            PlayerLoopSystem loopSystemRoot = PlayerLoop.GetCurrentPlayerLoop();

            EditPhysics(ref loopSystemRoot, false);

            //physicsLoopSystem.

            //physicsLoopSystem.updateDelegate

            PlayerLoop.SetPlayerLoop(loopSystemRoot);

            //Debug.Log(PhysXLib.AddNumberses(12, 7));
        }
    }

    void OnApplicationQuit() {
        PlayerLoopSystem loopSystemRoot = PlayerLoop.GetCurrentPlayerLoop();
        EditPhysics(ref loopSystemRoot, true);
        PlayerLoop.SetPlayerLoop(loopSystemRoot);
    }

    private bool EditPhysics(ref PlayerLoopSystem loopSystem, bool doPhysics) {
        if (loopSystem.type == typeof(FixedUpdate.PhysicsFixedUpdate)) {
            if (physicsFunction == IntPtr.Zero) physicsFunction = loopSystem.updateFunction;

            if (doPhysics) {
                loopSystem.updateFunction = physicsFunction;
                loopSystem.updateDelegate = null;
            }
            else {
                loopSystem.updateFunction = IntPtr.Zero;
                loopSystem.updateDelegate = () => {
                    PhysXSceneSimulator.Simulate();
                };
            }

            return true;
        }
        else {
            if (loopSystem.subSystemList != null) {
                for (int i = 0; i < loopSystem.subSystemList.Length; i++) {
                    if (EditPhysics(ref loopSystem.subSystemList[i], doPhysics)) return true;
                }
            }
        }

        return false;
    }

    // Update is called once per frame
    void Update()
    {
        if (Application.IsPlaying(gameObject)) {
            if (Input.GetKeyDown(KeyCode.P)) {
                doingPhysics = !doingPhysics;

                PlayerLoopSystem loopSystemRoot = PlayerLoop.GetCurrentPlayerLoop();
                EditPhysics(ref loopSystemRoot, doingPhysics);
                PlayerLoop.SetPlayerLoop(loopSystemRoot);
            }
        }
    }
}
