using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
  public int _Columns;
  public string[] _FileNames;

  public Transform _ButtonParent;
  public Transform _OptionsMenu;
  public Transform _SongMenu;


  [Header("Difficulty Sliders")]
  public Slider _NoteSpeedSlider;
  public Slider _ColliderSizeSlider;


  public Vector2 _ButtonSize;
  public GameObject _ButtonTemplate;

  private void Start()
  {
    CreateMenuButtons();
    OpenSongMenu();
    _NoteSpeedSlider.value = Global._NoteSpeedMultiplier;
  }

  void CreateMenuButtons()
  {
    _FileNames = Directory.GetFiles(Application.streamingAssetsPath + "/Songs", "*.json");

    int rows = _FileNames.Length / _Columns;
    if (rows * _Columns < _FileNames.Length)
    {
      rows++;
    }


    int targetRow = 0;
    int targetColumn = -1;
    Vector2 startingPosition = new Vector2(-(float)_Columns / 2f * _ButtonSize.x, (float)rows / 2f * _ButtonSize.y);
    for (int i = 0; i < _FileNames.Length; i++)
    {

      targetColumn++;
      if (targetColumn >= _Columns)
      {
        targetRow++;
        targetColumn -= _Columns;
      }

      _FileNames[i] = Path.GetFileNameWithoutExtension(_FileNames[i]);

      //Positioning actually works but this still looks like shit
      GameObject newButton = Instantiate(_ButtonTemplate, _ButtonParent);
      newButton.name = _FileNames[i]; // This is painful to do
      Vector3 buttonPosition = new Vector3(targetColumn * _ButtonSize.x, -targetRow * _ButtonSize.y, 0) + (Vector3)startingPosition;
      newButton.GetComponent<RectTransform>().localPosition = buttonPosition;
      newButton.GetComponentInChildren<Text>().text = _FileNames[i];
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

  public void SetNoteSpeed()
  {
    Global._NoteSpeedMultiplier = _NoteSpeedSlider.value;
  }
  public void SetColliderSize()
  {
    //Global._HitColliderSize = _ColliderSizeSlider.value;
  }
}
