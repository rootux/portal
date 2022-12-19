using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;
using Windows.Kinect;
using DefaultNamespace;
using KinectJoint = Windows.Kinect.Joint;

namespace DefaultNamespace
{
    public class KinectSkeletonTracker : MonoBehaviour
    {
        [SerializeField] private VisualEffect visualEffect;

        private KinectSensor sensor;
        private BodyFrameReader bodyFrameReader;
        private Body[] bodies;
        private ulong trackedId = ulong.MaxValue;

        [SerializeField] private PlayerNetwork m_PlayerNetwork;

        private void Awake()
        {
            m_PlayerNetwork = GetComponent<PlayerNetwork>();
        }

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
                            // visualEffect.SetBool("BothHandsOpen", isBothHandsOpen);
                            // visualEffect.SetBool("BothHandsClosed", isBothHandsClosed);

                            foreach (var joint in Joints.JointMap)
                            {
                                m_PlayerNetwork.SetJoint(joint.Key, GetVector3FromJoint(body.Joints[joint.Value]));
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
}