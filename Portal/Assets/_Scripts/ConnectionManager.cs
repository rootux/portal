using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet.Managing;
using Newtonsoft.Json;

public class ConnectionManager : MonoBehaviour
{
    private NetworkManager m_NetworkManager;
    [SerializeField] private bool m_IsServer = false;
    [SerializeField] private string m_AdressServer = "";

    private void Start()
    {
        m_NetworkManager = GetComponent<NetworkManager>();
        //JsonConverter

        if(m_IsServer == true)
        {
            m_NetworkManager.ServerManager.StartConnection();
            m_NetworkManager.ClientManager.StartConnection();
        }
        else
        {
            m_NetworkManager.ClientManager.StartConnection(m_AdressServer);
        }
    }
}
