using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

// Sets the script to be executed later than all default scripts
// This is helpful for UI, since other things may need to be initialized before setting the UI
[DefaultExecutionOrder(1000)]
public class MenuUIHandler : MonoBehaviour
{
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            StartVideo();
        }else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            StartGame();
        }
    }

    public void StartVideo()
    {
        Debug.Log("Started Video...");
        SceneManager.LoadScene(2);
    }

    public void StartGame()
    {
        Debug.Log("Starting Game...");
        SceneManager.LoadScene(1);
    }
}