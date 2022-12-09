using System.Collections;
using System.Collections.Generic;
using Windows.Kinect;
using DefaultNamespace;
using UnityEngine;
using FishNet.Connection;
using FishNet.Object;
using UnityEngine.VFX;

public class PlayerNetwork : NetworkBehaviour
{

    [SerializeField] private GameObject player;
    private VisualEffect visualEffect;

    // make sure we don't act on the other player 
    private void Start()
    {
        visualEffect = player.GetComponent<VisualEffect>();
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        if (base.IsOwner)
        {

        }
        else
        {
            GetComponent<PlayerNetwork>().enabled = false;
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            Dictionary<string, Vector3> body = GetBody();
            SetSkeltonServer(body);
        }
    }

    private Dictionary<string,Vector3> GetBody()
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
        Debug.Log("Received body");
        Debug.Log(skelton);
        // Debug.Log(skelton[0]);
        
    }
}
