using System;
using System.Runtime.InteropServices;

using UnityEngine;

namespace PhysX {
    [StructLayout (LayoutKind.Sequential)]
    public class PhysXQuat {

        public float x, y, z, w;

        public PhysXQuat(float x, float y, float z, float w) {
            this.x = x;
            this.y = y;
            this.z = z;
            this.w = w;
        }

        public PhysXQuat(Quaternion quaternion) {
            x = quaternion.x;
            y = quaternion.y;
            z = quaternion.z;
            w = quaternion.w;
        }

        public void FromQuaternion(Quaternion quaternion) {
            x = quaternion.x;
            y = quaternion.y;
            z = quaternion.z;
            w = quaternion.w;
        }

        public Quaternion ToQuaternion() {
            return new Quaternion(x, y, z, w);
        }

        public void ToQuaternion(ref Quaternion quaternion) {
            quaternion.Set(x, y, z, w);
        }
    }
}