using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet.Managing;
//using Newtonsoft.Json;
using FishNet.Transporting;

namespace DefaultNamespace
{
    public class ConnectionManager : MonoBehaviour
    {
        private NetworkManager m_NetworkManager;
        [SerializeField] private bool m_IsServer = false;
        [SerializeField] private string m_AdressServer = "";

        private void Start()
        {
            m_AdressServer = GlobalSettings.Instance.serverStaticIp;
            m_IsServer = GlobalSettings.Instance.isServer;
            bool shouldShowServerHud = GlobalSettings.Instance.isDebug;
            GameObject.FindWithTag("Stats").SetActive(shouldShowServerHud);
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
            if (args.ConnectionState == LocalConnectionState.Stopped)
            {
                connect();
            }

            Debug.Log(args.ConnectionState);
        }
    }
}