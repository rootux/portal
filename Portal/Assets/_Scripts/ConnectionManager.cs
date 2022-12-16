using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet.Managing;
using Newtonsoft.Json;
using FishNet.Transporting;

public class ConnectionManager : MonoBehaviour
{
    private NetworkManager m_NetworkManager;
    [SerializeField] private bool m_IsServer = false;
    [SerializeField] private string m_AdressServer = "";
    GlobalSettings globalSettings = null;

    private void Start()
    {
        if (GameObject.Find("globalSettings") == null)
        {
            globalSettings = PortalUtils.ImportJson("settings");
        }
        m_AdressServer = globalSettings.serverStaticIp;
        m_IsServer = globalSettings.isServer;
        
        m_NetworkManager = GetComponent<NetworkManager>();
        
        //JsonConverter

        m_NetworkManager.ClientManager.OnClientConnectionState += Test;
        connect();
    }

    private void connect()
    {
        if (m_IsServer == true)
        {
            m_NetworkManager.ServerManager.StartConnection();
            m_NetworkManager.ClientManager.StartConnection();
        }
        else
        {
            m_NetworkManager.ClientManager.StartConnection(m_AdressServer);
        }
    }

    private void Update()
    {
        //if(m_NetworkManager.ClientManager.Started)
    }

    public void Test(ClientConnectionStateArgs args)
    {
        if(args.ConnectionState == LocalConnectionState.Stopped)
        {
            connect();
        }
        Debug.Log(args.ConnectionState);
    }
}
