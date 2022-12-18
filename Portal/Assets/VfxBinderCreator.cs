using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX.Utility;
using Smrvfx;

public class VfxBinderCreator : MonoBehaviour
{
    public VFXPropertyBinder propBinder;
    public GameObject vfxHolder;
    public SkinnedMeshBaker meshBaker;
    public KinectManager kinectManager;
    public AvatarController avatarController;


    // Start is called before the first frame update
    void Start()
    {
        kinectManager = FindObjectOfType<KinectManager>();
        kinectManager.avatarControllers.Add(avatarController);
        
        propBinder = vfxHolder.AddComponent<VFXPropertyBinder>();
        propBinder.AddPropertyBinder<VFXSkinnedMeshBinder>().Target = meshBaker;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
