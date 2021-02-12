using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
  public static LevelManager _Manager; // Used so I dont need to give every single object that edits health n stuff a reference to the active manager because that is kind of annoying to do

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
  public float _NoteSpawnY = -6;
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

    ManageEnd();
    ManageArrows();
    ManageSong();
    ManageUI();
    ManageEffects();
  }

  private void OnDestroy()
  {
    _Manager = null; //Allow a new manager to be set when another level is loaded
  }


  void ManageEnd()
  {
    if (_LastBeat == -1)
    {
      for (int i = 0; i < Global._PlayingSong._Notes.Count; i++)
      {
        Note note = Global._PlayingSong._Notes[i];

        if (note._Time > _LastBeat)
          _LastBeat = note._Time;
      }
    }

    if (_Health <= 0)
    {
      LoseSong();
    }

    if (_Time > _LastBeat + _EndDelay)
    {
      WinSong();
    }
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
          MissNote(Global._HealthPerMiss / 2);
        }
        _PlayerArrows[i]._PressedTime = 0;
        _PlayerArrows[i]._HitThisPress = false;
      }

      if (_PlayerArrows[i]._PressedTime > Global._MaxArrowPressTime && !_PlayerArrows[i]._HitThisPress)
      {
        MissNote(Global._HealthPerMiss / 2);
        _PlayerArrows[i]._HitThisPress = true;
      }

      if (_PlayerArrows[i]._PressedTime <= Global._MaxArrowPressTime && _PlayerArrows[i]._PressedTime > 0 && !_PlayerArrows[i]._HitThisPress)
      {
        Collider2D[] colliders = Physics2D.OverlapBoxAll(_PlayerArrows[i].transform.position, Vector2.one, 0);
        Key hitKey = null;

        float minDistance = 10f;

        foreach (Collider2D collider in colliders)
        {
          Key key = collider.GetComponent<Key>();
          if (key)
          {
            float distance = Mathf.Abs(_PlayerArrows[i].transform.position.y - collider.transform.position.y);
            if (!hitKey || distance < minDistance)
            {
              hitKey = key;
              minDistance = distance;
            }
          }
        }
        if (hitKey)
        {
          if (minDistance < 0.05f)
          {
            Debug.Log("PERFECT!");
            _Health += Global._HealthPerHit * 1.2f;
          }
          else if (minDistance < 0.2f)
          {
            Debug.Log("Great!");
            _Health += Global._HealthPerHit;
          }
          else if(minDistance < 0.9f)
          {
            Debug.Log("Good.");
            _Health += Global._HealthPerHit * 0.8f;
          }
          else
          {
            Debug.Log("Bad");
            _Health += Global._HealthPerHit * 0.5f;
          }

          hitKey.GetHit();
          _PlayerArrows[i]._HitThisPress = true;
          _PlayerArrows[i].StartGlow();
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

        key._MoveSpeed = Global._PlayingSong._NoteSpeed * Global._NoteSpeedMultiplier;
        key._Time = note._Time;
        key._Collider.size = Vector2.one;
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
      _Health += Global._HealthPerMiss;
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

  void LoseSong()
  {
    //TODO: add some lose effects
    SceneManager.LoadScene("Main Menu");
  }

  void WinSong()
  {
    //TODO: add win effects
    SceneManager.LoadScene("Main Menu");
  }

  //Reduce the amount of variables here when optimization is needed
  float GetSendBeat(float hitBeat)
  {
    float hitTime = BeatsToSeconds(hitBeat);
    float distance = Mathf.Abs(_NoteSpawnY - _PlayerArrowsY);
    float timeToCross = distance / Global._PlayingSong._NoteSpeed * Global._NoteSpeedMultiplier;

    return SecondsToBeats(hitTime - timeToCross);
  }
  float DistanceFromExtraBeats(float beats)
  {
    float time = BeatsToSeconds(beats);
    float distance = Mathf.Abs(_NoteSpawnY - _PlayerArrowsY);
    float timeToCross = distance / Global._PlayingSong._NoteSpeed * Global._NoteSpeedMultiplier;
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
