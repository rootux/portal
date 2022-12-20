using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ToastManager : MonoBehaviour
{
   private IEnumerator coroutine = null;

   private float TIMEOUT = 3.0f;

   
   private void Awake()
   {
      SceneManager.sceneLoaded += OnSceneLoaded;
   }

   private void OnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
   {
      if (scene.name == "Video")
      {
         CancelAndHide();
      }
   }

   public void ShowYouHaveCall()
   {
      // stop any older runs of the call
      if (coroutine != null)
      {
         StopCoroutine(coroutine);
         coroutine = null;
      }

      Show();
      coroutine = WaitAndHide(TIMEOUT);
      StartCoroutine(coroutine);
   }

   void CancelAndHide()
   {
      if (coroutine != null)
      {
         StopCoroutine(coroutine);
         coroutine = null;
      }
      Hide();
   }

   private void Hide()
   {
      GetComponentInChildren<Canvas>().enabled = false;
   }

   private void Show()
   {
      GetComponentInChildren<Canvas>().enabled = true;
   }
   
   private IEnumerator WaitAndHide(float waitTime)
   {
      yield return new WaitForSeconds(waitTime);
      Hide();
   }
   
}
