using UnityEngine;
using UnityEngine.SceneManagement;

namespace DefaultNamespace
{
// Sets the script to be executed later than all default scripts
// This is helpful for UI, since other things may need to be initialized before setting the UI
    [DefaultExecutionOrder(1000)]
    public class MenuUIHandler : MonoBehaviour
    {

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                StartGame();
            }
            else if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                StartVideo();
            }
        }
        
        public void StartGame() {
            Debug.Log("Starting Game...");
            SceneManager.LoadScene(1);
        }

        public void StartVideo()
        {
            Debug.Log("Started Video...");
            SceneManager.LoadScene(2);
        }
    }
}