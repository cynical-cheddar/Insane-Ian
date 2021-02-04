using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDrivable {
    void Accellerate();
    void StopAccellerate();
    void Brake();
    void StopBrake();
    void Steer(int targetDirection);
    void StopSteer();

}