using System;
using System.Runtime.InteropServices;

using UnityEngine;

namespace PhysX {
    [StructLayout (LayoutKind.Sequential)]
    public class PhysXVec3 {

        public float x, y, z;

        public PhysXVec3() {
            
        }

        public PhysXVec3(float x, float y, float z) {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public PhysXVec3(Vector3 vector3) {
            x = vector3.x;
            y = vector3.y;
            z = vector3.z;
        }

        public void FromVector(Vector3 vector3) {
            x = vector3.x;
            y = vector3.y;
            z = vector3.z;
        }

        public Vector3 ToVector() {
            return new Vector3(x, y, z);
        }

        public void ToVector(ref Vector3 vector3) {
            vector3.Set(x, y, z);
        }
    }
}