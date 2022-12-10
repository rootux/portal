using System.Collections;
using System.Collections.Generic;
using Windows.Kinect;
using DefaultNamespace;
using UnityEngine;
using FishNet.Connection;
using FishNet.Object;
using UnityEngine.VFX;
using KinectVfx;

public class PlayerNetwork : NetworkBehaviour
{

    [SerializeField] private GameObject player;
    [SerializeField] private VisualEffect visualEffect;
    [SerializeField] private KinectSkeletonTracker kinectSkeletonTracker;

    // make sure we don't act on the other player 
    private void Start()
    {
        //visualEffect = player.GetComponent<VisualEffect>();
        //kinectSkeletonTracker = player.GetComponent<KinectSkeletonTracker>();
    }

    public override void OnStartClient()
    {
        base.OnStartClient();

        if (base.IsOwner)
        {
            transform.position = new Vector3(0, 0, 3);
            transform.eulerAngles = new Vector3(0, 180, 0);
            kinectSkeletonTracker.enabled = true;
        }
        else
        {
            kinectSkeletonTracker.enabled = false;
            transform.position = new Vector3(0, 0, 5);
            transform.eulerAngles = new Vector3(0, 0, 0);
            GetComponent<PlayerNetwork>().enabled = false;
        }
    }

    private void Update()
    {
        Dictionary<string, Vector3> body = GetBody();
        SetSkeltonServer(body);
    }

    private Dictionary<string, Vector3> GetBody()
    {
        Dictionary<string, Vector3> result = new Dictionary<string, Vector3>();
        foreach (var joint in Joints.JointMap.Keys)
        {
            Vector3 vector = visualEffect.GetVector3(joint);
            result[joint] = vector;
        }

        return result;
    }

    [ServerRpc]
    public void SetSkeltonServer(Dictionary<string, Vector3> skelton)
    {

        SendSkelton(skelton);
    }

    [ObserversRpc]
    public void SendSkelton(Dictionary<string, Vector3> skelton)
    {
        if (base.IsOwner == false)
        {

            Debug.Log("got data");
            foreach (var joint in Joints.JointMap.Keys)
            {
                Vector3 val;

                if (skelton.TryGetValue(joint, out val))
                {
                    visualEffect.SetVector3(joint, val);
                }
                else
                {
                    Debug.LogError("bad joint name");
                }
            }
        }
        // Debug.Log(skelton[0]);

    }
}
