using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
  public static LevelManager _Manager;

  [Header("Menu Stuff")]
  public bool _Paused;
  public GameObject _PauseMenu;

  [Header("Beat Zoom")]
  public Camera _ScreenCamera;
  public float _DefaultZoom;
  public float _ZoomPerBeat;
  public float _ZoomReturnSpeed;
  float _TimeUntilNextZoom;

  [Header("Notes")]
  public float _NoteSpawnY = -4;
  public GameObject[] _NotePrefabs;

  [Header("Arrows")]
  public Arrow[] _PlayerArrows; // Order is Left Down Up Right
  public float _PlayerArrowsY;
  public Color _PressedColor;
  public Color _UnpressedColor;

  [Header("Song")] //All in beats
  float _Time; //Current time
  public int _StartingDelay; // how many beats before the song starts
  public int _EndDelay;  // how many beats before the song ends
  [HideInInspector] public float _LastBeat = -1; // when do notes stop coming

  [Header("Difficulty Settings")]
  public float _NoteMoveSpeed = 4;
  public float _HitColliderSize = 1;
  public float _MaxArrowPressTime = 0.1f; //How long a key can be pressed (in seconds) until it will no longer destroy new notes
  public float _HealthPerHit = 2;
  public float _HealthPerMiss = -6;

  //Misc
  public float _Health = 50;

  [Header("Debug")]
  public Text _BeatText;
  public Image _HealthBar;

  private void Awake()
  {

    if (_Manager == null)
    {
      _Manager = this;
    }
    else
    {
      Destroy(this);
    }
    EndPause();
    _Time = -_StartingDelay;
  }

  private void Update()
  {
    if (Input.GetKeyDown(KeyCode.Return))
    {
      if (!_Paused)
      {
        StartPause();
      }
    }

    if (Global._PlayingSong == null || _Paused)
    {
      return;
    }

    if (_LastBeat == -1)
    {
      for (int i = 0; i < Global._PlayingSong._Notes.Count; i++)
      {
        Note note = Global._PlayingSong._Notes[i];

        if(note._Time > _LastBeat)
          _LastBeat = note._Time;
      }
    }

    if(_Time > _LastBeat + _EndDelay)
    {
      //TOOD add some fade effect
      SceneManager.LoadScene("Main Menu");
    }



    ManageArrows();
    ManageSong();
    ManageUI();
    ManageEffects();
  }

  private void OnDestroy()
  {
    _Manager = null; //Allow a new manager to be set when another level is loaded
  }

  void ManageArrows()
  {
    bool[] input = GetInput();
    for (int i = 0; i < 4; i++)
    {
      if (input[i])
      {
        _PlayerArrows[i]._PressedTime += Time.deltaTime;
      }
      else
      {
        if (!_PlayerArrows[i]._HitThisPress && _PlayerArrows[i]._PressedTime > 0)
        {
          MissNote(_HealthPerMiss / 2);
        }
        _PlayerArrows[i]._PressedTime = 0;
        _PlayerArrows[i]._HitThisPress = false;
      }

      if (_PlayerArrows[i]._PressedTime > _MaxArrowPressTime && !_PlayerArrows[i]._HitThisPress)
      {
        MissNote(_HealthPerMiss / 2);
        _PlayerArrows[i]._HitThisPress = true;
      }

      if (_PlayerArrows[i]._PressedTime <= _MaxArrowPressTime && _PlayerArrows[i]._PressedTime > 0 && !_PlayerArrows[i]._HitThisPress)
      {
        Collider2D[] colliders = Physics2D.OverlapBoxAll(_PlayerArrows[i].transform.position, Vector2.one * _HitColliderSize, 0);
        Key hitKey = null;

        foreach (Collider2D collider in colliders)
        {
          Key key = collider.GetComponent<Key>();

          if (key)
          {
            if (!hitKey || hitKey._Time > key._Time)
            {
              hitKey = key;
            }
          }
        }
        if (hitKey)
        {
          hitKey.GetHit();
          _PlayerArrows[i]._HitThisPress = true;
          _Health += _HealthPerHit;
        }
      }
    }
  }
  void ManageSong()
  {
    //TODO: Sort the notes based on time to prevent checking every note every frame (More useful for larger songs
    for (int i = 0; i < Global._PlayingSong._Notes.Count; i++)
    {
      Note note = Global._PlayingSong._Notes[i];
      float sendTime = GetSendBeat(note._Time);

      if (!note._Sent && sendTime <= _Time)
      {
        //DistanceFromExtraBeats() spawns the note at the proper position based on how much extra time has passed (in case of framerate problems).
        Key key = Instantiate(_NotePrefabs[(int)note._Key],
          new Vector3(_PlayerArrows[(int)note._Key].transform.position.x, _NoteSpawnY + DistanceFromExtraBeats(_Time - sendTime), 0),
          _NotePrefabs[(int)note._Key].transform.rotation).GetComponent<Key>();
        Global._PlayingSong._Notes[i]._Sent = true;

        key._MoveSpeed = _NoteMoveSpeed;
        key._Time = note._Time;
        key._Collider.size = _HitColliderSize * Vector2.one;
      }
    }


    float elapsedTime = SecondsToBeats(Time.deltaTime);
    if (elapsedTime != -1)
    {
      _Time += elapsedTime;
    }
  }
  void ManageUI()
  {
    _BeatText.text = "Beat: " + (int)_Time;

    _Health = Mathf.Clamp(_Health, 0, 100);
    _HealthBar.fillAmount = Mathf.Lerp(_HealthBar.fillAmount, _Health / 100, 6f * Time.deltaTime);
  }
  void ManageEffects()
  {
    //TODO: make zoom effects based on song
    _ScreenCamera.orthographicSize = Mathf.Lerp(_ScreenCamera.orthographicSize, _DefaultZoom, _ZoomReturnSpeed * Time.deltaTime);

    if (_TimeUntilNextZoom <= 0)
    {
      _ScreenCamera.orthographicSize += _ZoomPerBeat;
      _TimeUntilNextZoom += BeatsToSeconds(1);
    }
    _TimeUntilNextZoom -= Time.deltaTime;
  }

  public void StartPause()
  {
    _Paused = true;
    _PauseMenu.SetActive(true);
  }
  public void EndPause()
  {
    _Paused = false;
    _PauseMenu.SetActive(false);
  }

  public void MissNote(float damage = -1)
  {
    if (damage < 0)
    {
      _Health += _HealthPerMiss;
    }
    else
    {
      _Health += damage;
    }
  }
  float BeatsToSeconds(float beats)
  {
    if (Global._PlayingSong == null)
    {
      return -1;
    }

    float seconds = beats * 60;
    seconds /= Global._PlayingSong._BPM;
    return seconds;
  }
  float SecondsToBeats(float seconds)
  {
    if (Global._PlayingSong == null)
    {
      //return -1;
    }

    float beats = seconds * Global._PlayingSong._BPM;
    beats /= 60;
    return beats;
  }

  //Reduce the amount of variables here when optimization is needed
  float GetSendBeat(float hitBeat)
  {
    float hitTime = BeatsToSeconds(hitBeat);
    float distance = Mathf.Abs(_NoteSpawnY - _PlayerArrowsY);
    float timeToCross = distance / _NoteMoveSpeed;

    return SecondsToBeats(hitTime - timeToCross);
  }
  float DistanceFromExtraBeats(float beats)
  {
    float time = BeatsToSeconds(beats);
    float distance = Mathf.Abs(_NoteSpawnY - _PlayerArrowsY);
    float timeToCross = distance / _NoteMoveSpeed;
    float percentExtra = time / timeToCross;

    return distance * percentExtra;
  }



  bool[] GetInput()
  {
    bool[] input = new bool[4];

    input[0] = Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow);  // Left
    input[1] = Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow);  // Down
    input[2] = Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow);    // Up
    input[3] = Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow); // Right
    return input;

  }
}
