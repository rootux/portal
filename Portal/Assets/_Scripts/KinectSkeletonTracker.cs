using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;
using Windows.Kinect;
using DefaultNamespace;
using KinectJoint = Windows.Kinect.Joint;

public class KinectSkeletonTracker : MonoBehaviour
{
    [SerializeField]
    private VisualEffect visualEffect;

    private KinectSensor sensor;
    private BodyFrameReader bodyFrameReader;
    private Body[] bodies;
    private ulong trackedId = ulong.MaxValue;

    private Dictionary<JointType, JointType> boneMap = new Dictionary<JointType, JointType>() {

        {JointType.FootLeft, JointType.AnkleLeft },
        {JointType.AnkleLeft, JointType.KneeLeft },
        {JointType.KneeLeft, JointType.HipLeft },
        {JointType.HipLeft, JointType.SpineBase },

        {JointType.FootRight, JointType.AnkleRight },
        {JointType.AnkleRight,JointType.KneeRight     },
        {JointType.KneeRight,JointType.HipRight },
        {JointType.HipRight,JointType.SpineBase },

        {JointType.HandLeft, JointType.WristLeft },
        {JointType.WristLeft,JointType.ElbowLeft },
        {JointType.ElbowLeft,JointType.ShoulderLeft },
        {JointType.ShoulderLeft,JointType.SpineShoulder },

        {JointType.HandRight,JointType.WristRight },
        {JointType.WristRight,JointType.ElbowRight },
        {JointType.ElbowRight,JointType.ShoulderRight },
        {JointType.ShoulderRight,JointType.SpineShoulder },

        {JointType.SpineBase,JointType.SpineMid },
        {JointType.SpineMid,JointType.SpineShoulder },
        {JointType.SpineShoulder,JointType.Head }
    };

    

    // Start is called before the first frame update
    void Start()
    {
        sensor = KinectSensor.GetDefault();

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
                        // Todo check performance hit on fps
                        bool isBothHandsOpen = IsRightHandOpen(body) && IsLeftHandOpen(body);
                        bool isBothHandsClosed = IsRightHandClosed(body) && IsLeftHandClosed(body);
                        visualEffect.SetBool("BothHandsOpen", isBothHandsOpen);
                        visualEffect.SetBool("BothHandsClosed", isBothHandsClosed);
                        
                        foreach (var joint in Joints.JointMap)
                        {
                            visualEffect.SetVector3(joint.Key, GetVector3FromJoint(body.Joints[joint.Value]));
                        }
                        break;
                    }
                }
            }
        }
    }

    private bool IsRightHandOpen(Body body)
    {
        return (body.HandRightState == HandState.Open && body.HandRightConfidence == TrackingConfidence.High);
    }
    
    private bool IsRightHandClosed(Body body)
    {
        return (body.HandRightState == HandState.Closed && body.HandRightConfidence == TrackingConfidence.High);
    }

    private bool IsLeftHandOpen(Body body)
    {
        return (body.HandLeftState == HandState.Open && body.HandLeftConfidence == TrackingConfidence.High);
    }
    
    private bool IsLeftHandClosed(Body body)
    {
        return (body.HandLeftState == HandState.Closed && body.HandLeftConfidence == TrackingConfidence.High);
    }

    private static Vector3 GetVector3FromJoint(KinectJoint joint)
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
