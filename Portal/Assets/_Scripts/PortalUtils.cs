using UnityEngine;

public class PortalUtils
{
    public static GlobalSettings ImportJson(string path)
    {
        TextAsset textAsset = Resources.Load<TextAsset>(path);
        return JsonUtility.FromJson<GlobalSettings>(textAsset.text);
    }
}
