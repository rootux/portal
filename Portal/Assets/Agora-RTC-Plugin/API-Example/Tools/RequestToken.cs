using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

[Serializable]
public class TokenObject {
  public string rtcToken;
}

namespace Agora.Util
{
  public static class HelperClass
  {
    public static IEnumerator FetchToken(
        string url, string channel, uint userId, Action<string> callback = null
    )
    {
      string fullServerAddress = string.Format(
        "{0}/rtc/{1}/publisher/uid/{2}/?expiry=950400", url, channel, userId
      );
      Debug.Log("Contacting " + fullServerAddress);
      UnityWebRequest request = UnityWebRequest.Get(fullServerAddress);
      yield return request.SendWebRequest();

      if (request.isNetworkError || request.isHttpError) {
        Debug.Log(request.error);
        callback(null);
        yield break;
      }

      TokenObject tokenInfo = JsonUtility.FromJson<TokenObject>(
        request.downloadHandler.text
      );
      Debug.Log("Got token from the server" + request.downloadHandler.text);

      callback(tokenInfo.rtcToken);
    }
  }
}
