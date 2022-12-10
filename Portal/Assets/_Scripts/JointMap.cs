using System.Collections.Generic;
using Windows.Kinect;

namespace DefaultNamespace
{
    public class Joints
    {
        public static Dictionary<string, JointType> JointMap = new Dictionary<string, JointType>()
        {
            { "Spine Base", JointType.SpineBase },
            { "Spine Mid", JointType.SpineMid },
            { "Spine Shoulder", JointType.SpineShoulder },
            { "Head", JointType.Head },

            { "Foot Left", JointType.FootLeft },
            { "Ankle Left", JointType.AnkleLeft },
            { "Knee Left", JointType.KneeLeft },
            { "Hip Left", JointType.HipLeft },

            { "Foot Right", JointType.FootRight },
            { "Ankle Right", JointType.AnkleRight },
            { "Knee Right", JointType.KneeRight },
            { "Hip Right", JointType.HipRight },

            { "Wrist Left", JointType.WristLeft },
            { "Elbow Left", JointType.ElbowLeft },
            { "Shoulder Left", JointType.ShoulderLeft },

            { "Wrist Right", JointType.WristRight },
            { "Elbow Right", JointType.ElbowRight },
            { "Shoulder Right", JointType.ShoulderRight }
        };
    }
}