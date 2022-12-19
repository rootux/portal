using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DefaultNamespace
{
// Sets the script to be executed later than all default scripts
// This is helpful for UI, since other things may need to be initialized before setting the UI
    [DefaultExecutionOrder(1000)]
    public class MenuUIHandler : MonoBehaviour
    {
        private void Start()
        {
            Cursor.visible = false; 
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                StartGame();
            }
            else if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                StartVideo();
            }else if (Input.GetKeyDown(KeyCode.Q))
            {
                QuitGame();
            }
        }
        
        public void StartGame() {
            Debug.Log("Starting Game...");
            var currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
            if (currentSceneIndex != 1)
            {
                SceneManager.LoadScene(1);
            }
        }

        public void StartVideo()
        {
            Debug.Log("Started Video...");
            var currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
            if (currentSceneIndex != 2)
            {
                SceneManager.LoadScene(2);
            }
        }
        
        public void QuitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
            Application.Quit();
        }
    }
}