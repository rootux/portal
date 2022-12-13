﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;
using Windows.Kinect;
using KinectJoint = Windows.Kinect.Joint;

public class KinectSkeletonTracker : MonoBehaviour
{
    [SerializeField] private Avatar avatar;
    [SerializeField] private Vector3 lastJointPos;
    [SerializeField] private float velocity;

    [SerializeField] private VisualEffect visualEffect;

    private KinectSensor sensor;
    private BodyFrameReader bodyFrameReader;
    private Body[] bodies;
    private ulong trackedId = ulong.MaxValue;
    private Animator anim;

    private Dictionary<JointType, JointType> boneMap = new Dictionary<JointType, JointType>()
    {
        {JointType.FootLeft, JointType.AnkleLeft},
        {JointType.AnkleLeft, JointType.KneeLeft},
        {JointType.KneeLeft, JointType.HipLeft},
        {JointType.HipLeft, JointType.SpineBase},

        {JointType.FootRight, JointType.AnkleRight},
        {JointType.AnkleRight, JointType.KneeRight},
        {JointType.KneeRight, JointType.HipRight},
        {JointType.HipRight, JointType.SpineBase},

        {JointType.HandLeft, JointType.WristLeft},
        {JointType.WristLeft, JointType.ElbowLeft},
        {JointType.ElbowLeft, JointType.ShoulderLeft},
        {JointType.ShoulderLeft, JointType.SpineShoulder},

        {JointType.HandRight, JointType.WristRight},
        {JointType.WristRight, JointType.ElbowRight},
        {JointType.ElbowRight, JointType.ShoulderRight},
        {JointType.ShoulderRight, JointType.SpineShoulder},

        {JointType.SpineBase, JointType.SpineMid},
        {JointType.SpineMid, JointType.SpineShoulder},
        {JointType.SpineShoulder, JointType.Head}
    };

    private Dictionary<string, JointType> jointMap = new Dictionary<string, JointType>()
    {
        {"Spine Base", JointType.SpineBase},
        {"Spine Mid", JointType.SpineMid},
        {"Spine Shoulder", JointType.SpineShoulder},
        {"Head", JointType.Head},

        {"Foot Left", JointType.FootLeft},
        {"Ankle Left", JointType.AnkleLeft},
        {"Knee Left", JointType.KneeLeft},
        {"Hip Left", JointType.HipLeft},

        {"Foot Right", JointType.FootRight},
        {"Ankle Right", JointType.AnkleRight},
        {"Knee Right", JointType.KneeRight},
        {"Hip Right", JointType.HipRight},

        {"Wrist Left", JointType.WristLeft},
        {"Elbow Left", JointType.ElbowLeft},
        {"Shoulder Left", JointType.ShoulderLeft},

        {"Wrist Right", JointType.WristRight},
        {"Elbow Right", JointType.ElbowRight},
        {"Shoulder Right", JointType.ShoulderRight}
    };

    [SerializeField] private Transform headBone;
    [SerializeField] private Transform spineShoulder;
    [SerializeField] private Transform spine;
    [SerializeField] private Transform spineBase;
    
    [SerializeField] private Transform shoulderL;
    [SerializeField] private Transform elbowL;
    [SerializeField] private Transform wristL;
    
    [SerializeField] private Transform shoulderR;
    [SerializeField] private Transform elbowR;
    [SerializeField] private Transform wristR;
    
    [SerializeField] private Transform hipL;
    [SerializeField] private Transform kneeL;
    [SerializeField] private Transform ankleL;
    [SerializeField] private Transform footL;
    
    [SerializeField] private Transform hipR;
    [SerializeField] private Transform kneeR;
    [SerializeField] private Transform ankleR;
    [SerializeField] private Transform footR;
    [SerializeField] private float posLimiter;


    // Start is called before the first frame update
    void Start()
    {
        //avatar.
        sensor = KinectSensor.GetDefault();
        anim = GetComponent<Animator>();

        if (sensor != null)
        {
            bodyFrameReader = sensor.BodyFrameSource.OpenReader();

            if (!sensor.IsOpen)
            {
                sensor.Open();
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (bodyFrameReader != null)
        {
            var frame = bodyFrameReader.AcquireLatestFrame();

            if (frame != null)
            {
                if (bodies == null)
                {
                    bodies = new Body[frame.BodyCount];
                }

                frame.GetAndRefreshBodyData(bodies);
                frame.Dispose();
                frame = null;

                List<ulong> trackedIds = new List<ulong>();
                foreach (var body in bodies)
                {
                    if (body == null) continue;

                    if (body.IsTracked)
                    {
                        trackedIds.Add(body.TrackingId);
                    }
                }

                if (!trackedIds.Contains(trackedId))
                {
                    if (trackedIds.Count > 0)
                    {
                        trackedId = trackedIds[0];
                    }
                    else
                    {
                        trackedId = ulong.MaxValue;
                    }
                }

                foreach (var body in bodies)
                {
                    if (body.TrackingId == trackedId)
                    {
                        Transform currentJoint = null;
                        
                        foreach (var joint in jointMap)
                        {
                            switch (joint.Key)
                            {
                                case "Head":
                                    currentJoint = headBone;
                                    break;
                                case "Spine Shoulder":
                                    currentJoint = spineShoulder;
                                    break;
                                case "Spine Mid":
                                    currentJoint = spine;
                                    break;
                                case "Spine Base":
                                    currentJoint = spineBase;
                                    break;
                                
                                case "Shoulder Right":
                                    currentJoint = shoulderR;
                                    break;
                                case "Elbow Right":
                                    currentJoint = elbowR;
                                    break;
                                case "Wrist Right":
                                    currentJoint = wristR;                                   
                                    break;
                                
                                case "Shoulder Left":
                                    currentJoint = shoulderL;                                   
                                    break;
                                case "Elbow Left":
                                    currentJoint = elbowL;                                   
                                    break;
                                case "Wrist Left":
                                    currentJoint = wristL;                                   
                                    break;
                                
                                case "Hip Left":
                                    currentJoint = hipL;                                   
                                    break;
                                case "Knee Left":
                                    currentJoint = kneeL;                                   
                                    break;
                                case "Ankle Left":
                                    currentJoint = ankleL;                                   
                                    break;
                                case "Foot Left":                                   
                                    currentJoint = footL;                                   
                                    break;
                                
                                case "Hip Right":
                                    currentJoint = hipR;                                   
                                    break;
                                case "Knee Right":
                                    currentJoint = kneeR;                                   
                                    break;
                                case "Ankle Right":
                                    currentJoint = ankleR;                                   
                                    break;
                                case "Foot Right":
                                    currentJoint = footR;                                   
                                    break;
                            }
                            currentJoint.position = GetVector3FromJoint(currentJoint.position, body.Joints[joint.Value]);

/*                            if (currentJoint)
                            {
                                if (Mathf.Abs(currentJoint.position.magnitude - lastJointPos.magnitude) < posLimiter)
                                {
                                    Debug.Log(Mathf.Abs(currentJoint.position.magnitude - lastJointPos.magnitude));
                                    visualEffect.SetFloat("Velocity", velocity);
                                }
                                else
                                {
                                    Debug.Log("Stop!!" + (Mathf.Abs(currentJoint.position.magnitude - lastJointPos.magnitude)));
                                    visualEffect.SetFloat("Velocity", 0);
                                    //Debug.Break();
                                }
                                
                                currentJoint.position = GetVector3FromJoint(currentJoint.position, body.Joints[joint.Value]);
                                lastJointPos = currentJoint.position;
                            }*/
                        }

                        break;
                    }
                }
            }
        }
    }

    private Vector3 GetVector3FromJoint(Vector3 limbCurrentPos, KinectJoint joint)
    {
        return new Vector3(joint.Position.X, joint.Position.Y, joint.Position.Z);
    }

    private void OnDestroy()
    {
        if (bodyFrameReader != null)
        {
            bodyFrameReader.Dispose();
            bodyFrameReader = null;
        }

        //if (sensor != null)
        //{

        //}
    }
}