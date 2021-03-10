using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDrivable {
    void Accellerate();
    void StopAccellerate();
    void Brake();
    void StopBrake();
    void Steer(float targetDirection);
    void StopSteer();
    void Reverse();
    void Drift();
    void StopDrift();
    void UpdateWheelPoses();
}