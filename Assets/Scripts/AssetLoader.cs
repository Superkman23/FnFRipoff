using UnityEngine;
using System.IO;
using UnityEngine.Networking;

public static class AssetLoader
{
  //This class will be used to load sprites or other types of files down the line

  public static AudioClip GetClipFromFile(string trackName)
  {
    string path = Application.streamingAssetsPath + "/Audio/Backings/" + trackName;
    if (!File.Exists(path))
    {
      return null;
    }
    string url = string.Format("file://{0}", path);
    UnityWebRequest request = UnityWebRequestMultimedia.GetAudioClip(url, AudioType.OGGVORBIS);
    request.SendWebRequest();
    while (!request.isDone)
    {
    }
    AudioClip clip = DownloadHandlerAudioClip.GetContent(request);
    return clip;
  }
}
