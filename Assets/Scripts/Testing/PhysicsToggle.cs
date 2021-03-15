using System;
using System.Runtime.InteropServices;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.LowLevel;

public class PhysicsToggle : MonoBehaviour
{
    private bool doingPhysics = false;
    private IntPtr physicsFunction = IntPtr.Zero;

    #if UNITY_WEBGL
    [DllImport("__Internal")]
    #else
    [DllImport("CppTest")]
    #endif
    public static extern int AddNumbers(int x, int y);

    // Start is called before the first frame update
    void Start()
    {
        DontDestroyOnLoad(gameObject);

        PlayerLoopSystem loopSystemRoot = PlayerLoop.GetCurrentPlayerLoop();
        
        EditPhysics(ref loopSystemRoot, false);

        //physicsLoopSystem.

        //physicsLoopSystem.updateDelegate

        PlayerLoop.SetPlayerLoop(loopSystemRoot);

        Debug.Log(AddNumbers(7, 12));
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
                    //Debug.Log("YEET!");
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
        if (Input.GetKeyDown(KeyCode.P)) {
            doingPhysics = !doingPhysics;

            PlayerLoopSystem loopSystemRoot = PlayerLoop.GetCurrentPlayerLoop();
            EditPhysics(ref loopSystemRoot, doingPhysics);
            PlayerLoop.SetPlayerLoop(loopSystemRoot);
        }
    }
}
