using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
  public int columns;
  public string[] fileNames;

  public Transform buttonParent;
  public Vector2 buttonSize;
  public GameObject buttonTemplate;

  private void Start()
  {
    fileNames = Directory.GetFiles(Application.streamingAssetsPath + "/Songs", "*.json");

    for(int i = 0; i < fileNames.Length; i++)
    {
      fileNames[i] = Path.GetFileNameWithoutExtension(fileNames[i]);

      //Positioning doesnt work but it'll do for now
      GameObject newButton = Instantiate(buttonTemplate, buttonParent);
      Vector3 buttonPosition = new Vector3(0, 500 - (i * 200), 0);
      newButton.GetComponent<RectTransform>().localPosition = buttonPosition;
      newButton.name = fileNames[i]; // This is painful to do
      newButton.GetComponentInChildren<Text>().text = fileNames[i];
      Button button = newButton.GetComponent<Button>();
      button.onClick.AddListener(() => LoadSong(newButton.name));
    }
  }

  public void LoadSong(string songName)
  {
    Global._PlayingSong = JsonToSong.GetSong(songName);
    SceneManager.LoadScene("Game");
  }





}
