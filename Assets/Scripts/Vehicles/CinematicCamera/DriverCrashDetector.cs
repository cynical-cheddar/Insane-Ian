using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Cinemachine;
using PhysX;

[VehicleScript(ScriptType.playerDriverScript)]

public class DriverCrashDetector : MonoBehaviour, ICollisionEnterEvent
{
    public float crashAngleThreshold = 50;

    public float playerSensorMultiplier = 3f;

    public int playerLayer = 8;

    private float currentSpeed;

    public bool drawDebugLines = false;
    
    [Header("Sensors")]
    public PhysXCollider.CollisionLayer sensorLayerMask;
    float sensorLength = 5f;
    
    
    [Header("Default: 15 and 15")]
    public SpeedSensorLengthPair slowRange;
    [Header("Default: 35 and 35")]
    public SpeedSensorLengthPair fastRange;
    
    public Vector3 frontSensorPosition = new Vector3(0f, 0.2f, 2.5f);
    public float frontSideSensorPosition = 0.2f;
    public float frontSensorAngle = 30f;
    private PhysXRigidBody myRb;
    
    
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
        public float forwardBackValue;
        public CurrentSensorReportStruct(float speedLocal, float crashValueLocal, float telecastCrashValueLocal,
            bool playerAheadLocal, bool crashedLocal, Transform lastCrashedPlayerLocal, float leftRightCoefficientLocal, float estimatedTimeToHitLocal, float estimatedDistanceToHitLocal, float forwardBackValueLocal)
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
            forwardBackValue = forwardBackValueLocal;
        }
    }

    private List<float> distList = new List<float>();
    private List<float> timeList = new List<float>();
   // [SerializeField]
    public CurrentSensorReportStruct currentSensorReport;
    private NetworkPlayerVehicle npv;
    
    
    
    
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

        public void CollisionEnter() {}
    public bool requiresData { get { return true; } }

    private void Start()
    {
        //  Debug.LogWarning("Driver Crash Detector has not been ported to the new PhysX system");
        // return;

        npv = GetComponent<NetworkPlayerVehicle>();
        myRb = GetComponent<PhysXRigidBody>();
        currentSensorReport = new CurrentSensorReportStruct();
        currentSensorReport.lastCrashedPlayer = transform.root;
        
    }


    public void CollisionEnter(PhysXCollision other)
    {

    }

    public void CrashCollisionCamera(PhysXCollision other){

            if (other.transform.root.CompareTag("Player"))
            {
            currentSensorReport.crashed = true;
            // get left/right 
            PhysXContactPoint[] contactPoints = other.GetContacts();
            Vector3 cpSum = Vector3.zero;
            foreach (PhysXContactPoint c in contactPoints)
            {
                cpSum += c.point;
            }

            cpSum /= contactPoints.Length;

            // get dir
            float signedDir = Vector3.SignedAngle(transform.forward, cpSum - transform.position, Vector3.up);
            if (signedDir < 0) currentSensorReport.leftRightCoefficient = -1;
            else currentSensorReport.leftRightCoefficient = 1;

            float forwardBackValue = 0;
            if (Mathf.Abs(signedDir) < 90) forwardBackValue = 1;
            else forwardBackValue = -1;

            currentSensorReport.forwardBackValue = forwardBackValue;
            
            
            
            currentSensorReport.lastCrashedPlayer = other.transform.root;

            
            if (!npv.botDriver) {
                GetComponentInChildren<DriverCinematicCam>().SetCam(DriverCinematicCam.Cams.carCrashFrontLeftEnum);
            }
             //   if(lastTargetPoint!=null) Destroy(lastTargetPoint);
              //  Vector3 point = contactPoints[0].point;
             //   lastTargetPoint = Instantiate(new GameObject(), point, Quaternion.identity);
             //   lastTargetPoint.transform.parent = transform.root;
              //  currentSensorReport.lastCrashedPlayer = lastTargetPoint.transform;

             // StartCoroutine(SetCrashedFalse());
                crashTimer = 0f;
            }

            
        
    }

    private GameObject lastTargetPoint;
    
    float crashTimer = 0;

    IEnumerator SetCrashedFalse()
    {
        yield return new WaitForSeconds(0.3f);
        
        currentSensorReport.crashed = false;
        //currentSensorReport.lastCrashedPlayer = transform.root;
      //  GetComponentInChildren<DriverCinematicCam>().SetCam(DriverCinematicCam.Cams.defaultCamEnum);
    }

    private Vector3 vel = Vector3.zero;
    private Vector3 localVel = Vector3.zero;
    private void FixedUpdate()
    {
        //return;
        crashTimer += Time.deltaTime;
        if(crashTimer > 0.6){
            currentSensorReport.crashed = false;
           // GetComponentInChildren<DriverCinematicCam>().SetCam(DriverCinematicCam.Cams.defaultCamEnum);
        }
        vel = myRb.velocity;
        localVel = transform.InverseTransformDirection(vel);
        
        // interpolate speed;
        currentSpeed = localVel.z;
        if (currentSpeed < 0) currentSpeed = 0;
        
        if (currentSpeed < slowRange.speed)
        {
            sensorLength = 0.01f;
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

    void CalculateTimeToHit(PhysXRigidBody otherPlayer)
    {
        // Debug.Log("Start calculateTimetoHit physx");
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



            answer = relativeDistance / relativeSpeed;
            if (relativeDistance < 2) answer = 0;
            
          //  Debug.Log("Velocity difference player" + velocityDifference);
          //  Debug.Log("Velocity difference estimated time player" + answer + " and velocity differnce was " + velocityDifference);
            
            distList.Add(relativeDistance);
            timeList.Add(answer);
            
            // Debug.Log("End calculateTimetoHit physx velocityDifference.z > 1 ");
        }
        
        // Debug.Log("End calculateTimetoHit physx");
        
        
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
            


            // if distance is more that 30 percent closer than other distances, invalidate list and add this
            
            
            
            answer = relativeDistance / relativeSpeed;
            if (relativeDistance < 2) answer = 0;
            // Debug.Log("Velocity difference" + velocityDifference);
            // Debug.Log("Velocity difference estimated time " + answer);
            distList.Add(relativeDistance);
            timeList.Add(answer);
        }
    }

    private float dotThreshold = 0.93f;

    void CalculateSensors()
    {
        //Debug.Log("player layer: " + playerLayer);
            float leftRightCoefficient = 0;
            
            bool avoiding = false;
            bool playerAhead = false;

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
            PhysXRaycastHit hit = PhysXRaycast.GetRaycastHit();
            if (PhysXRaycast.Fire(sensorStartPos, transform.forward, hit, sensorLength, sensorLayerMask, myRb.vehicleId)) {
                 if (drawDebugLines) Debug.DrawLine(sensorStartPos, hit.point);
                    avoiding = true;
                    float increase = 1f;
                    float telecastIncrease = 1f;
                    // if normal is > angle
                    if (Vector3.Angle(hit.normal, transform.up) > crashAngleThreshold &&  Vector3.Dot(hit.normal, Vector3.up) < dotThreshold)
                    {
                        if (hit.collider.gameObject.layer == playerLayer)
                        {
                            telecastIncrease *= playerSensorMultiplier;
                            playerAhead = true;
                            CalculateTimeToHit(hit.collider.attachedRigidBody);
                        }
                        else CalculateTimeToHit(hit.point);

                        

                        crashSensorValue += increase;
                        telecastCrashSensorValue += telecastIncrease;
                        leftRightCoefficient += 0.75f;
                        
                    }
                    else
                    {
                        distList.Add(myRb.velocity.magnitude * sensorLength);
                        timeList.Add(1f);
                    }
            }


            //front right angle sensor
            else if (PhysXRaycast.Fire(sensorStartPos, Quaternion.AngleAxis(frontSensorAngle, transform.up) * transform.forward, hit, sensorLength, sensorLayerMask, myRb.vehicleId))
            {
                if (drawDebugLines) Debug.DrawLine(sensorStartPos, hit.point);
                float increase = 0.5f;
                float telecastIncrease = 0.5f;
                if (Vector3.Angle(hit.normal, transform.up) > crashAngleThreshold*0.75 &&  Vector3.Dot(hit.normal, Vector3.up) < dotThreshold)
                {
                    if (hit.collider.gameObject.layer == playerLayer)
                    {
                        telecastIncrease *= playerSensorMultiplier;
                        playerAhead = true;
                        CalculateTimeToHit(hit.collider.attachedRigidBody);
                    }
                    else CalculateTimeToHit(hit.point);

                    crashSensorValue += increase;
                    telecastCrashSensorValue += telecastIncrease;
                    leftRightCoefficient += 0.25f;
                }
                else
                {
                    distList.Add(myRb.velocity.magnitude * sensorLength);
                    timeList.Add(1f);
                }
            }

            //front left sensor
            sensorStartPos -= transform.right * (frontSideSensorPosition * 2);
            if (PhysXRaycast.Fire(sensorStartPos, transform.forward, hit, sensorLength, sensorLayerMask, myRb.vehicleId)) {
                if (drawDebugLines) Debug.DrawLine(sensorStartPos, hit.point);
                float increase = 1f;
                float telecastIncrease = 1f;
                if (Vector3.Angle(hit.normal, transform.up) > crashAngleThreshold &&  Vector3.Dot(hit.normal, Vector3.up) < dotThreshold)
                {
                    if (hit.collider.gameObject.layer == playerLayer)
                    {
                        telecastIncrease *= playerSensorMultiplier;
                        playerAhead = true;
                        CalculateTimeToHit(hit.collider.attachedRigidBody);
                    }
                    else CalculateTimeToHit(hit.point);

                    crashSensorValue += increase;
                    telecastCrashSensorValue += telecastIncrease;
                    leftRightCoefficient -= 0.75f;
                }
                else
                {
                    distList.Add(myRb.velocity.magnitude * sensorLength);
                    timeList.Add(1f);
                }
            }

            //front left angle sensor
            else if (PhysXRaycast.Fire(sensorStartPos,
                Quaternion.AngleAxis(-frontSensorAngle, transform.up) * transform.forward, hit, sensorLength, sensorLayerMask, myRb.vehicleId))
            {
                if (drawDebugLines) Debug.DrawLine(sensorStartPos, hit.point);
                float increase = 0.5f;
                float telecastIncrease = 0.5f;
                if (Vector3.Angle(hit.normal, transform.up) > crashAngleThreshold*0.75 &&  Vector3.Dot(hit.normal, Vector3.up) < dotThreshold)
                {
                    if (hit.collider.gameObject.layer == playerLayer)
                    {
                        telecastIncrease *= playerSensorMultiplier;
                        playerAhead = true;
                        CalculateTimeToHit(hit.collider.attachedRigidBody);
                    }
                    else CalculateTimeToHit(hit.point);

                    crashSensorValue += increase;
                    telecastCrashSensorValue += telecastIncrease;
                    leftRightCoefficient -= 0.25f;
                }
                else
                {
                    distList.Add(myRb.velocity.magnitude * sensorLength);
                    timeList.Add(1f);
                }
            }

            //front center sensor
            if (PhysXRaycast.Fire(sensorStartPos, transform.forward, hit, sensorLength, sensorLayerMask, myRb.vehicleId))
                {
                    if (drawDebugLines) Debug.DrawLine(sensorStartPos, hit.point);
                    float increase = 1.5f;
                    float telecastIncrease = 1.5f;
                    if (Vector3.Angle(hit.normal, transform.up) > crashAngleThreshold &&  Vector3.Dot(hit.normal, Vector3.up) < dotThreshold)
                    {
                        if (hit.collider.gameObject.layer == playerLayer)
                        {
                            telecastIncrease *= playerSensorMultiplier;
                            playerAhead = true;
                            // Debug.Log("hit collider is: " + hit.collider + " and rb is: " + hit.collider.attachedRigidBody);
                            CalculateTimeToHit(hit.collider.attachedRigidBody);
                        }
                        else CalculateTimeToHit(hit.point);

                        crashSensorValue += increase;
                        telecastCrashSensorValue += telecastIncrease;
                    }
                    else
                    {
                        distList.Add(myRb.velocity.magnitude * sensorLength);
                        timeList.Add(1f);
                    }
                }
            PhysXRaycast.ReleaseRaycastHit(hit);

            float meanDist = Mathf.Infinity;
            float meanCrashTime = Mathf.Infinity;;

            if (distList.Count > 0) meanDist = distList.Average();
            if (timeList.Count > 0) meanCrashTime = timeList.Average();

            
            //Debug.Log(meanCrashTime);
            
            currentSensorReport.estimatedDistanceToHit = meanDist;
            currentSensorReport.estimatedTimeToHit = meanCrashTime;
            currentSensorReport.crashValue = crashSensorValue;
            currentSensorReport.telecastCrashValue = telecastCrashSensorValue;
            currentSensorReport.playerAhead = playerAhead;
            currentSensorReport.leftRightCoefficient = leftRightCoefficient;

    }
}
