using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Random = UnityEngine.Random;



    [VehicleScript(ScriptType.aiDriverScript)]
    public class CarAIControl : MonoBehaviour
    {
        public enum BrakeCondition
        {
            NeverBrake,                 // the car simply accelerates at full throttle all the time.
            TargetDirectionDifference,  // the car will brake according to the upcoming change in direction of the target. Useful for route-based AI, slowing for corners.
            TargetDistance,             // the car will brake as it approaches its target, regardless of the target's direction. Useful if you want the car to
                                        // head for a stationary target and come to rest when it arrives there.
        }

        // This script provides input to the car controller in the same way that the user control script does.
        // As such, it is really 'driving' the car, with no special physics or animation tricks to make the car behave properly.

        // "wandering" is used to give the cars a more human, less robotic feel. They can waver slightly
        // in speed and direction while driving towards their target.

        public LayerMask sensorLayerMask;
        
        [SerializeField] [Range(0, 1)] private float m_CautiousSpeedFactor = 0.05f;               // percentage of max speed to use when being maximally cautious
        [SerializeField] [Range(0, 180)] private float m_CautiousMaxAngle = 50f;                  // angle of approaching corner to treat as warranting maximum caution
        [SerializeField] private float m_CautiousMaxDistance = 100f;                              // distance at which distance-based cautiousness begins
        [SerializeField] private float m_CautiousAngularVelocityFactor = 30f;                     // how cautious the AI should be when considering its own current angular velocity (i.e. easing off acceleration if spinning!)
        [SerializeField] private float m_SteerSensitivity = 0.05f;                                // how sensitively the AI uses steering input to turn to the desired direction
        [SerializeField] private float m_AccelSensitivity = 0.04f;                                // How sensitively the AI uses the accelerator to reach the current desired speed
        [SerializeField] private float m_BrakeSensitivity = 1f;                                   // How sensitively the AI uses the brake to reach the current desired speed
        [SerializeField] private float m_LateralWanderDistance = 3f;                              // how far the car will wander laterally towards its target
        [SerializeField] private float m_LateralWanderSpeed = 0.1f;                               // how fast the lateral wandering will fluctuate
        [SerializeField] [Range(0, 1)] private float m_AccelWanderAmount = 0.1f;                  // how much the cars acceleration will wander
        [SerializeField] private float m_AccelWanderSpeed = 0.1f;                                 // how fast the cars acceleration wandering will fluctuate
        [SerializeField] private BrakeCondition m_BrakeCondition = BrakeCondition.TargetDistance; // what should the AI consider when accelerating/braking?
        [SerializeField] private bool m_Driving;                                                  // whether the AI is currently actively driving or stopped.
        [SerializeField] private Transform m_Target;                                              // 'target' the target object to aim for.
        [SerializeField] private bool m_StopWhenTargetReached;                                    // should we stop driving when we reach the target?
        [SerializeField] private float m_ReachTargetThreshold = 2;                                // proximity to target to consider we 'reached' it, and stop driving.

        private float m_RandomPerlin;             // A random value for the car to base its wander on (so that AI cars don't all wander in the same pattern)
        private InterfaceCarDrive4W interfaceCarDrive;    // Reference to actual car controller we are controlling
        private float m_AvoidOtherCarTime;        // time until which to avoid the car we recently collided with
        private float m_AvoidOtherCarSlowdown;    // how much to slow down due to colliding with another car, whilst avoiding
        private float m_AvoidPathOffset;          // direction (-1 or 1) in which to offset path to avoid other car, whilst avoiding
        private Rigidbody m_Rigidbody;

        private IDrivable CarDriver;

        public float maxSpeed = 30f;


        public List<string> terrainTags = new List<string>();
     //   public string terraintag = "DustGround";
        
        [Header("Sensors")]
        public float sensorLength = 3f;
        public Vector3 frontSensorPosition = new Vector3(0f, 0.2f, 0.5f);
        public float frontSideSensorPosition = 0.2f;
        public float frontSensorAngle = 30f;

        public bool yeaaahhhhh = true;

        private bool inThreePointTurn = false;

        protected bool circuitFound = false;
        private void Awake()
        {
            // get the car controller reference
            interfaceCarDrive = GetComponent<InterfaceCarDrive4W>();

            // give the random perlin a random value
            m_RandomPerlin = Random.value*100;

            m_Rigidbody = GetComponent<Rigidbody>();
            
            CarDriver = interfaceCarDrive.GetComponent<IDrivable>();
            m_Driving = true;
            WaypointCircuit wpc = FindObjectOfType<WaypointCircuit>();
            if (wpc != null) circuitFound = true;
        }

        IEnumerator ThreePointTurn()
        {
            Debug.Log("ThreePointTurn");
            inThreePointTurn = true;
            float elapsedTime = 0;
            float maxTime = 1.5f;

            Vector3 targetPos = m_Target.position;
            Vector3 targetDir = targetPos - transform.position;
            
            float dotResult = Vector3.Dot(transform.forward, targetDir);
            float turnDir = 0;
            if (dotResult > 0.0) {
                turnDir = 1.0f;
            } else if (dotResult < 0.0) {
                turnDir = -1.0f;
            } else {
                turnDir = 0.0f;
            }
            
            while (elapsedTime<=maxTime)
            {
                elapsedTime += Time.deltaTime;
                CarDriver.StopAccellerate();
                CarDriver.StopBrake();
                CarDriver.Reverse();
                CarDriver.Steer(turnDir);
                yield return new WaitForFixedUpdate();
            }

            inThreePointTurn = false;
        }
        IEnumerator ThreePointTurn2()
        {
            inThreePointTurn = true;
            float elapsedTime = 0;
            float maxTime = 1.5f;

            Vector3 targetPos = m_Target.position;
            Vector3 targetDir = targetPos - transform.position;
            
            float dotResult = Vector3.Dot(transform.forward, targetDir);
            float turnDir = 0;
            if (dotResult > 0.0) {
                turnDir = 1.0f;
            } else if (dotResult < 0.0) {
                turnDir = -1.0f;
            } else {
                turnDir = 0.0f;
            }
            
            while (elapsedTime<=maxTime)
            {
                elapsedTime += Time.deltaTime;
  
                CarDriver.StopBrake();
                CarDriver.Accellerate();
                CarDriver.Steer(turnDir);
                yield return new WaitForFixedUpdate();
            }

            inThreePointTurn = false;
        }

        private float stuckTimer = 0;

        private void FixedUpdate()
        {
            Debug.LogWarning("AI control needs porting to new PhysX system.");
            return;

            if (circuitFound)
            {
                if (m_Rigidbody.velocity.magnitude < 1)
                {
                    stuckTimer += Time.deltaTime;
                }
                else
                {
                    stuckTimer = 0;
                }

                if (stuckTimer > 1 && !inThreePointTurn)
                {
                    int rand = Random.Range(0, 1);
                    StartCoroutine(ThreePointTurn());
                    
                   // else if(rand==2)StartCoroutine(ThreePointTurn2());
                }

               

                if (!inThreePointTurn)
                {
                    if (m_Target == null || !m_Driving)
                    {
                        // Car should not be moving,
                        // use handbrake to stop
                        //m_CarController.Move(0, 0, -1f, 1f);
                        CarDriver.Brake();
                        CarDriver.StopAccellerate();
                    }
                    // do sensor stuff
                    else if (SensorsManouvre())
                    {

                    }
                    else
                    {
                        NormalDriving();
                    }
                }
            }
        }

        bool SensorsManouvre()
        {
            bool avoiding = false;
            RaycastHit hit;
            Vector3 sensorStartPos = transform.position;
            sensorStartPos += transform.forward * frontSensorPosition.z;
            sensorStartPos += transform.up * frontSensorPosition.y;
            float avoidMultiplier = 0;
            avoiding = false;

            //front right sensor
            sensorStartPos += transform.right * frontSideSensorPosition;
            if (Physics.Raycast(sensorStartPos, transform.forward, out hit, sensorLength, sensorLayerMask)) {
                Debug.DrawLine(sensorStartPos, hit.point);
                    avoiding = true;
                    avoidMultiplier -= 1f;
                }

            //front right angle sensor
            else if (Physics.Raycast(sensorStartPos, Quaternion.AngleAxis(frontSensorAngle, transform.up) * transform.forward, out hit, sensorLength, sensorLayerMask)) {
                Debug.DrawLine(sensorStartPos, hit.point);
                    avoiding = true;
                    avoidMultiplier -= 0.5f;
                }

            //front left sensor
            sensorStartPos -= transform.right * (frontSideSensorPosition * 2);
            if (Physics.Raycast(sensorStartPos, transform.forward, out hit, sensorLength, sensorLayerMask)) {
                Debug.DrawLine(sensorStartPos, hit.point);
                    avoiding = true;
                    avoidMultiplier += 1f;
            }

            //front left angle sensor
            else if (Physics.Raycast(sensorStartPos, Quaternion.AngleAxis(-frontSensorAngle, transform.up) * transform.forward, out hit, sensorLength, sensorLayerMask)) {
                Debug.DrawLine(sensorStartPos, hit.point);
                    avoiding = true;
                    avoidMultiplier += 0.5f;
                }

            //front center sensor
            if (avoidMultiplier == 0) {
                if (Physics.Raycast(sensorStartPos, transform.forward, out hit, sensorLength, sensorLayerMask)) {
                    if (!terrainTags.Contains(hit.collider.tag)) {
                        Debug.DrawLine(sensorStartPos, hit.point);
                        avoiding = true;
                        if (hit.normal.x < 0) {
                            avoidMultiplier = -1;
                        } else {
                            avoidMultiplier = 1;
                        }
                    }
                }
            }

            if (avoiding) {
                CarDriver.Steer(avoidMultiplier);
                if(avoidMultiplier >= 1 && m_Rigidbody.velocity.magnitude > maxSpeed/2) CarDriver.Reverse();
                else if(avoidMultiplier <= -1 && m_Rigidbody.velocity.magnitude > maxSpeed/2) CarDriver.Reverse();
            }

            return avoiding;
        }

        void NormalDriving()
        {
            Vector3 fwd = transform.forward;
                    if (m_Rigidbody.velocity.magnitude > 20f)
                    {
                        fwd = m_Rigidbody.velocity;
                    }

                    float desiredSpeed = maxSpeed;

                    // now it's time to decide if we should be slowing down...
                    switch (m_BrakeCondition)
                    {
                        case BrakeCondition.TargetDirectionDifference:
                        {
                            // the car will brake according to the upcoming change in direction of the target. Useful for route-based AI, slowing for corners.

                            // check out the angle of our target compared to the current direction of the car
                            float approachingCornerAngle = Vector3.Angle(m_Target.forward, fwd);

                            // also consider the current amount we're turning, multiplied up and then compared in the same way as an upcoming corner angle
                            float spinningAngle =
                                m_Rigidbody.angularVelocity.magnitude * m_CautiousAngularVelocityFactor;

                            // if it's different to our current angle, we need to be cautious (i.e. slow down) a certain amount
                            float cautiousnessRequired = Mathf.InverseLerp(0, m_CautiousMaxAngle,
                                Mathf.Max(spinningAngle,
                                    approachingCornerAngle));
                            desiredSpeed = Mathf.Lerp(maxSpeed, maxSpeed * m_CautiousSpeedFactor,
                                cautiousnessRequired);
                            break;
                        }

                        case BrakeCondition.TargetDistance:
                        {
                            // the car will brake as it approaches its target, regardless of the target's direction. Useful if you want the car to
                            // head for a stationary target and come to rest when it arrives there.

                            // check out the distance to target
                            Vector3 delta = m_Target.position - transform.position;
                            float distanceCautiousFactor = Mathf.InverseLerp(m_CautiousMaxDistance, 0, delta.magnitude);

                            // also consider the current amount we're turning, multiplied up and then compared in the same way as an upcoming corner angle
                            float spinningAngle =
                                m_Rigidbody.angularVelocity.magnitude * m_CautiousAngularVelocityFactor;

                            // if it's different to our current angle, we need to be cautious (i.e. slow down) a certain amount
                            float cautiousnessRequired = Mathf.Max(
                                Mathf.InverseLerp(0, m_CautiousMaxAngle, spinningAngle), distanceCautiousFactor);
                            desiredSpeed = Mathf.Lerp(maxSpeed, maxSpeed * m_CautiousSpeedFactor,
                                cautiousnessRequired);
                            break;
                        }

                        case BrakeCondition.NeverBrake:
                            break;
                    }

                    // Evasive action due to collision with other objects:

                    // our target position starts off as the 'real' target position
                    Vector3 offsetTargetPos = m_Target.position;

                    // if are we currently taking evasive action to prevent being stuck against another car:
                    if (Time.time < m_AvoidOtherCarTime)
                    {
                        // slow down if necessary (if we were behind the other car when collision occured)
                        desiredSpeed *= m_AvoidOtherCarSlowdown;

                        // and veer towards the side of our path-to-target that is away from the other car
                        offsetTargetPos += m_Target.right * m_AvoidPathOffset;
                    }
                    else
                    {
                        // no need for evasive action, we can just wander across the path-to-target in a random way,
                        // which can help prevent AI from seeming too uniform and robotic in their driving
                        offsetTargetPos += m_Target.right *
                                           (Mathf.PerlinNoise(Time.time * m_LateralWanderSpeed, m_RandomPerlin) * 2 -
                                            1) *
                                           m_LateralWanderDistance;
                    }

                    // use different sensitivity depending on whether accelerating or braking:
                    float accelBrakeSensitivity = (desiredSpeed < m_Rigidbody.velocity.magnitude)
                        ? m_BrakeSensitivity
                        : m_AccelSensitivity;

                    // decide the actual amount of accel/brake input to achieve desired speed.
                    float accel = Mathf.Clamp((desiredSpeed - m_Rigidbody.velocity.magnitude) * accelBrakeSensitivity,
                        -1, 1);

                    // add acceleration 'wander', which also prevents AI from seeming too uniform and robotic in their driving
                    // i.e. increasing the accel wander amount can introduce jostling and bumps between AI cars in a race
                    accel *= (1 - m_AccelWanderAmount) +
                             (Mathf.PerlinNoise(Time.time * m_AccelWanderSpeed, m_RandomPerlin) * m_AccelWanderAmount);

                    // calculate the local-relative position of the target, to steer towards
                    Vector3 localTarget = transform.InverseTransformPoint(offsetTargetPos);

                    // work out the local angle towards the target
                    float targetAngle = Mathf.Atan2(localTarget.x, localTarget.z) * Mathf.Rad2Deg;

                    // get the amount of steering needed to aim the car towards the target
                    float steer = Mathf.Clamp(targetAngle * m_SteerSensitivity, -1, 1) *
                                  Mathf.Sign(m_Rigidbody.velocity.magnitude);

                    // feed input to the car controller.
                    //  m_CarController.Move(steer, accel, accel, 0f);

                    CarDriver.Steer((int) steer);

                    if (!yeaaahhhhh)
                    {
                        if (desiredSpeed > m_Rigidbody.velocity.magnitude)
                        {
                            CarDriver.Accellerate();
                            CarDriver.StopBrake();
                        }
                        else CarDriver.Brake();
                    }
                    else
                    {
                        CarDriver.Accellerate();
                    }


                    // if appropriate, stop driving when we're close enough to the target.
                    if (m_StopWhenTargetReached && localTarget.magnitude < m_ReachTargetThreshold)
                    {
                        m_Driving = false;
                    }
                }


        private void OnCollisionStay(Collision col)
        {
            // detect collision against other cars, so that we can take evasive action
            if (col.gameObject.GetComponentInParent<Obstacle>() != null)
            {
                var obstacle = col.gameObject.GetComponentInParent<Obstacle>();
                if (obstacle != null)
                {
                    // we'll take evasive action for 1 second
                    m_AvoidOtherCarTime = Time.time + 1;

                    // but who's in front?...
                    if (Vector3.Angle(transform.forward, obstacle.transform.position - transform.position) < 90)
                    {
                        // the other ai is in front, so it is only good manners that we ought to brake...
                        m_AvoidOtherCarSlowdown = 0.5f;
                    }
                    else
                    {
                        // we're in front! ain't slowing down for anybody...
                        m_AvoidOtherCarSlowdown = 1;
                    }

                    // both cars should take evasive action by driving along an offset from the path centre,
                    // away from the other car
                    var otherCarLocalDelta = transform.InverseTransformPoint(obstacle.transform.position);
                    float otherCarAngle = Mathf.Atan2(otherCarLocalDelta.x, otherCarLocalDelta.z);
                    m_AvoidPathOffset = m_LateralWanderDistance*-Mathf.Sign(otherCarAngle);
                }
            }
        }


        public void SetTarget(Transform target)
        {
            m_Target = target;
            m_Driving = true;
        }
    }
