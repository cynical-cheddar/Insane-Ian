using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Cinemachine;

[VehicleScript(ScriptType.playerDriverScript)]

public class DriverCrashDetector : MonoBehaviour
{





    public float crashAngleThreshold = 50;


    
    public float playerSensorMultiplier = 3f;

    public int playerLayer = 8;

    private float currentSpeed;



    
    

    
    [Header("Sensors")]
    public LayerMask sensorLayerMask;
    float sensorLength = 5f;
    
    
    [Header("Default: 15 and 10")]
    public SpeedSensorLengthPair slowRange;
    [Header("Default: 35 and 20")]
    public SpeedSensorLengthPair fastRange;
    
    public Vector3 frontSensorPosition = new Vector3(0f, 0.2f, 2.5f);
    public float frontSideSensorPosition = 0.2f;
    public float frontSensorAngle = 30f;
    private Rigidbody myRb;
    
    
    [Serializable]
    public struct CurrentSensorReportStruct
    {
        public float speed;
        public float crashValue;
        public float telecastCrashValue;
        public bool playerAhead;
        
        public bool crashed;
        public Transform lastCrashedPlayer;
        public float leftRightCoefficient;
        public float estimatedTimeToHit;
        public float estimatedDistanceToHit;
        public CurrentSensorReportStruct(float speedLocal, float crashValueLocal, float telecastCrashValueLocal,
            bool playerAheadLocal, bool crashedLocal, Transform lastCrashedPlayerLocal, float leftRightCoefficientLocal, float estimatedTimeToHitLocal, float estimatedDistanceToHitLocal)
        {
            speed = speedLocal;
            crashValue = crashValueLocal;
            telecastCrashValue = telecastCrashValueLocal;
            playerAhead = playerAheadLocal;
            crashed = crashedLocal;
            lastCrashedPlayer = lastCrashedPlayerLocal;
            leftRightCoefficient = leftRightCoefficientLocal;
            estimatedTimeToHit = estimatedTimeToHitLocal;
            estimatedDistanceToHit = estimatedDistanceToHitLocal;
        }
    }

    private List<float> distList = new List<float>();
    private List<float> timeList = new List<float>();
   // [SerializeField]
    public CurrentSensorReportStruct currentSensorReport;
    
    
    
    
    [Serializable]
    public struct SpeedSensorLengthPair
    {
        public float speed;
        public float sensorLength;

        public SpeedSensorLengthPair(float s, float l)
        {
            speed = s;
            sensorLength = l;
        }
    }

    private void Start()
    {
        myRb = GetComponent<Rigidbody>();
        currentSensorReport = new CurrentSensorReportStruct();

    }


    private void OnCollisionEnter(Collision other)
    {
        currentSensorReport.crashed = true;
    }

    private Vector3 vel = Vector3.zero;
    private Vector3 localVel = Vector3.zero;
    private void FixedUpdate()
    {

        vel = myRb.velocity;
        localVel = transform.InverseTransformDirection(vel);
        
        // interpolate speed;
        currentSpeed = localVel.z;
        if (currentSpeed < 0) currentSpeed = 0;
        
        if (currentSpeed < slowRange.speed)
        {
            sensorLength = 0;
        }
        else if (currentSpeed > fastRange.speed)
        {
            sensorLength = fastRange.sensorLength;
        }
        else
        {
            // interpolate length
            sensorLength = slowRange.sensorLength + (currentSpeed - slowRange.speed) *
                ((fastRange.sensorLength - slowRange.sensorLength) / (fastRange.speed - slowRange.speed));
        }

        currentSensorReport.speed = currentSpeed;
        CalculateSensors();
    }

    void CalculateTimeToHit(Rigidbody otherPlayer)
    {
        float answer = Mathf.Infinity;
        Vector3 otherVel = transform.InverseTransformDirection(otherPlayer.velocity);

        Vector3 velocityDifference =  localVel - otherVel;
        
        

        if (velocityDifference.z <= 1)
        {
            float relativeDistance = Vector3.Distance(otherPlayer.position, transform.position + transform.forward * frontSensorPosition.z);





            if (relativeDistance < 2)
            {
                distList.Add(relativeDistance);
                timeList.Add(0.05f);
            }
          
        }
        else if (velocityDifference.z > 1)
        {
            float relativeSpeed = currentSpeed = velocityDifference.z;
            float relativeDistance = Vector3.Distance(otherPlayer.position , transform.position + transform.forward * frontSensorPosition.z);

            if (distList.Count > 0)
            {
                // if distance is more that 50% further than all other, don't add
                if (relativeDistance > distList.Max() * 1.5f) return;

                // if distance is more that 30 percent closer than other distances, invalidate list and add this
                if (relativeDistance < distList.Min() * 0.70f)
                {
                    distList.Clear();
                    timeList.Clear();
                }
            }

            answer = relativeDistance / relativeSpeed;
            if (relativeDistance < 2) answer = 0;
            
          //  Debug.Log("Velocity difference player" + velocityDifference);
          //  Debug.Log("Velocity difference estimated time player" + answer + " and velocity differnce was " + velocityDifference);
            
            distList.Add(relativeDistance);
            timeList.Add(answer);
        }
        
        
    }
    void CalculateTimeToHit(Vector3 hitpoint)
    {
        float answer = Mathf.Infinity;
        Vector3 otherVel = Vector3.zero;

        Vector3 velocityDifference =  localVel - otherVel;
        
        

        if (velocityDifference.z <= 1)
        {
          //  Debug.Log("Velocity difference not gonna hit" + velocityDifference);
          float relativeDistance = Vector3.Distance(hitpoint, transform.position + transform.forward * frontSensorPosition.z);






        }
        else if (velocityDifference.z > 1)
        {
            float relativeSpeed = currentSpeed = velocityDifference.z;
            float relativeDistance = Vector3.Distance(hitpoint, transform.position + transform.forward * frontSensorPosition.z);
            
            // if distance is more that 50% further than all other, don't add
            if (distList.Count > 0)
            {
                if (relativeDistance > distList.Max() * 1.5f) return;
                if (relativeDistance < distList.Min() * 0.70f)
                {
                    distList.Clear();
                    timeList.Clear();
                }
            }

            // if distance is more that 30 percent closer than other distances, invalidate list and add this
            
            
            
            answer = relativeDistance / relativeSpeed;
            if (relativeDistance < 2) answer = 0;
            // Debug.Log("Velocity difference" + velocityDifference);
            // Debug.Log("Velocity difference estimated time " + answer);
            distList.Add(relativeDistance);
            timeList.Add(answer);
        }
    }

    void CalculateSensors()
    {
        Debug.Log("player layer: " + playerLayer);
            float leftRightCoefficient = 0;
            
            bool avoiding = false;
            bool playerAhead = false;
            RaycastHit hit;
            Vector3 sensorStartPos = transform.position;
            sensorStartPos += transform.forward * frontSensorPosition.z;
            sensorStartPos += transform.up * frontSensorPosition.y;
            float crashSensorValue = 0;
            float telecastCrashSensorValue = 0;
            
            distList.Clear();
            timeList.Clear();
            // calculate a value of crashing
         //   Vector3 targetVel = Vector3.zero;
            //front right sensor
            sensorStartPos += transform.right * frontSideSensorPosition;
            if (Physics.Raycast(sensorStartPos, transform.forward, out hit, sensorLength, sensorLayerMask)) {
                Debug.DrawLine(sensorStartPos, hit.point);
                    avoiding = true;
                    float increase = 1f;
                    float telecastIncrease = 1f;
                    // if normal is > angle
                    if (Vector3.Angle(hit.normal, transform.up) > crashAngleThreshold)
                    {
                        if (hit.collider.gameObject.layer == playerLayer)
                        {
                            telecastIncrease *= playerSensorMultiplier;
                            playerAhead = true;
                            CalculateTimeToHit(hit.collider.attachedRigidbody);
                            
                        }
                        else CalculateTimeToHit(hit.point);

                        

                        crashSensorValue += increase;
                        telecastCrashSensorValue += telecastIncrease;
                        leftRightCoefficient += 0.75f;
                        
                    }
            }

            //front right angle sensor
            else if (Physics.Raycast(sensorStartPos,
                Quaternion.AngleAxis(frontSensorAngle, transform.up) * transform.forward, out hit, sensorLength,
                sensorLayerMask))
            {
                Debug.DrawLine(sensorStartPos, hit.point);
                float increase = 0.5f;
                float telecastIncrease = 0.5f;
                if (Vector3.Angle(hit.normal, transform.up) > crashAngleThreshold)
                {
                    if (hit.collider.gameObject.layer == playerLayer)
                    {
                        telecastIncrease *= playerSensorMultiplier;
                        playerAhead = true;
                        CalculateTimeToHit(hit.collider.attachedRigidbody);
                    }
                    else CalculateTimeToHit(hit.point);

                    crashSensorValue += increase;
                    telecastCrashSensorValue += telecastIncrease;
                    leftRightCoefficient += 0.25f;
                }
            }

            //front left sensor
            sensorStartPos -= transform.right * (frontSideSensorPosition * 2);
            if (Physics.Raycast(sensorStartPos, transform.forward, out hit, sensorLength, sensorLayerMask)) {
                Debug.DrawLine(sensorStartPos, hit.point);
                float increase = 1f;
                float telecastIncrease = 1f;
                if (Vector3.Angle(hit.normal, transform.up) > crashAngleThreshold)
                {
                    if (hit.collider.gameObject.layer == playerLayer)
                    {
                        telecastIncrease *= playerSensorMultiplier;
                        playerAhead = true;
                        CalculateTimeToHit(hit.collider.attachedRigidbody);
                    }
                    else CalculateTimeToHit(hit.point);

                    crashSensorValue += increase;
                    telecastCrashSensorValue += telecastIncrease;
                    leftRightCoefficient -= 0.75f;
                }
            }

            //front left angle sensor
            else if (Physics.Raycast(sensorStartPos,
                Quaternion.AngleAxis(-frontSensorAngle, transform.up) * transform.forward, out hit, sensorLength,
                sensorLayerMask))
            {
                Debug.DrawLine(sensorStartPos, hit.point);
                float increase = 0.5f;
                float telecastIncrease = 0.5f;
                if (Vector3.Angle(hit.normal, transform.up) > crashAngleThreshold)
                {
                    if (hit.collider.gameObject.layer == playerLayer)
                    {
                        telecastIncrease *= playerSensorMultiplier;
                        playerAhead = true;
                        CalculateTimeToHit(hit.collider.attachedRigidbody);
                    }
                    else CalculateTimeToHit(hit.point);

                    crashSensorValue += increase;
                    telecastCrashSensorValue += telecastIncrease;
                    leftRightCoefficient -= 0.25f;
                }
            }

            //front center sensor
            if (Physics.Raycast(sensorStartPos, transform.forward, out hit, sensorLength, sensorLayerMask))
                {
                    Debug.DrawLine(sensorStartPos, hit.point);
                    float increase = 1.5f;
                    float telecastIncrease = 1.5f;
                    if (Vector3.Angle(hit.normal, transform.up) > crashAngleThreshold)
                    {
                        if (hit.collider.gameObject.layer == playerLayer)
                        {
                            telecastIncrease *= playerSensorMultiplier;
                            playerAhead = true;
                            CalculateTimeToHit(hit.collider.attachedRigidbody);
                        }
                        else CalculateTimeToHit(hit.point);

                        crashSensorValue += increase;
                        telecastCrashSensorValue += telecastIncrease;
                    }
                }

            float meanDist = Mathf.Infinity;
            float meanCrashTime = Mathf.Infinity;;

            if (distList.Count > 0) meanDist = distList.Min();
            if (timeList.Count > 0) meanCrashTime = timeList.Min();

            
            Debug.Log(meanCrashTime);
            
            currentSensorReport.estimatedDistanceToHit = meanDist;
            currentSensorReport.estimatedTimeToHit = meanCrashTime;
            currentSensorReport.crashValue = crashSensorValue;
            currentSensorReport.telecastCrashValue = telecastCrashSensorValue;
            currentSensorReport.playerAhead = playerAhead;
            currentSensorReport.leftRightCoefficient = leftRightCoefficient;

    }
}
