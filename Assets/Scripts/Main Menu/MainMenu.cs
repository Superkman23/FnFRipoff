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
  public Transform _OptionsMenu;
  public Transform _SongMenu;

  public Vector2 buttonSize;
  public GameObject buttonTemplate;

  private void Start()
  {
    CreateMenuButtons();
  }

  void CreateMenuButtons()
  {
    fileNames = Directory.GetFiles(Application.streamingAssetsPath + "/Songs", "*.json");

    int rows = fileNames.Length / columns;
    if (rows * columns < fileNames.Length)
    {
      rows++;
    }


    int targetRow = 0;
    int targetColumn = -1;
    Vector2 startingPosition = new Vector2(-(float)columns / 2f * buttonSize.x, (float)rows / 2f * buttonSize.y);
    for (int i = 0; i < fileNames.Length; i++)
    {

      targetColumn++;
      if (targetColumn >= columns)
      {
        targetRow++;
        targetColumn -= columns;
      }

      fileNames[i] = Path.GetFileNameWithoutExtension(fileNames[i]);

      //Positioning actually works but this still looks like shit
      GameObject newButton = Instantiate(buttonTemplate, buttonParent);
      newButton.name = fileNames[i]; // This is painful to do
      Vector3 buttonPosition = new Vector3(targetColumn * buttonSize.x, -targetRow * buttonSize.y, 0) + (Vector3)startingPosition;
      newButton.GetComponent<RectTransform>().localPosition = buttonPosition;
      newButton.GetComponentInChildren<Text>().text = fileNames[i];
      Button button = newButton.GetComponent<Button>();
      button.onClick.AddListener(() => LoadSong(newButton.name));
    }
  }



  public void OpenOptionsMenu()
  {
    _OptionsMenu.gameObject.SetActive(true);
    _SongMenu.gameObject.SetActive(false);
  }
  public void OpenSongMenu()
  {
    _OptionsMenu.gameObject.SetActive(false);
    _SongMenu.gameObject.SetActive(true);
  }

  public void LoadSong(string songName)
  {
    Global._PlayingSong = JsonToSong.GetSong(songName);
    SceneManager.LoadScene("Game");
  }
}
