using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelManager : MonoBehaviour
{
  public enum Gamestate
  {
    GameplayState,
    PauseState,
    LoseState,
    WinState
  };

  public static LevelManager _Manager; // Used so I dont need to give every single object that edits health n stuff a reference to the active manager because that is kind of annoying to do

  [Header("General Stuff")]
  public Gamestate _Gamestate;
  public Gamestate _LastGamestate;
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
  public float _MaxHitDistance = 1; //overlap to check for notes
  public float _PlayerArrowsY;
  public Color _PressedColor;
  public Color _UnpressedColor;

  [Header("Song")] //All in beats
  float _Time; //Current time
  public int _StartingDelay; // time before the song starts (in seconds)
  public int _EndDelay;  // how many beats before the song ends
  [HideInInspector] public float _LastBeat = -1; // when do notes stop coming

  [Header("Misc")]
  public float _Health = 50;
  public Text _BeatText;
  public Image _HealthBar;

  [Header("Fading")]
  public CanvasGroup _FadeGroup;
  public float _TargetFadeAlpha;

  private void Awake()
  {
    if (Global._PlayingSong == null) //Song didnt properly load, load a default one instead.
    {
      Global._PlayingSong = JsonToSong.GetSong("TestSong");
    }

    _FadeGroup.alpha = 1;
    _TargetFadeAlpha = 0;
    if (_Manager == null)
    {
      _Manager = this;
    }
    else
    {
      Destroy(this);
    }
    EndPause();
    _Time -= SecondsToBeats(Global._FadeDuration + _StartingDelay);
  }

  private void Update()
  {
    if (Input.GetKeyDown(KeyCode.Return) && _Gamestate != Gamestate.LoseState)
    {
      if (!IsPaused())
      {
        StartPause();
      }
      else
      {
        EndPause();
      }
    }

    if (IsPaused())
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

        if (note._Time + note._Duration > _LastBeat)
          _LastBeat = note._Time + note._Duration;
      }
    }

    if (_Health <= 0)
    {
      ChangeGameState(Gamestate.LoseState);
    }

    if (_Time > _LastBeat + _EndDelay)
    {
      ChangeGameState(Gamestate.WinState);
    }
  }

  void ManageArrows()
  {
    bool[] input = GetInput();
    for (int i = 0; i < 4; i++)
    {
      if (input[i])
      {
        if(_PlayerArrows[i]._HeldNote != null)
        {
          _PlayerArrows[i].StartGlow();
          _Health += SecondsToBeats(Time.deltaTime) * Global._HealthPerHit;
        }
        _PlayerArrows[i]._Pressed = true;
      }
      else
      {
        _PlayerArrows[i]._Pressed = false;
        _PlayerArrows[i]._GotPressResult = false;
        if (_PlayerArrows[i]._HeldNote != null)
        {
          _PlayerArrows[i]._HeldNote.ReleaseTrail();
          _PlayerArrows[i]._HeldNote.GetHit(false);
          _PlayerArrows[i]._HeldNote = null;
        }
      }

      if (_PlayerArrows[i]._Pressed && _PlayerArrows[i]._GotPressResult == false)
      {
        Collider2D[] colliders = Physics2D.OverlapBoxAll(_PlayerArrows[i].transform.position, Vector2.one * _MaxHitDistance, 0);
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
          if (minDistance < _MaxHitDistance * 0.07f)
          {
            Debug.Log("PERFECT!");
            _Health += Global._HealthPerHit * 1.2f;
          }
          else if (minDistance < _MaxHitDistance * 0.4f)
          {
            Debug.Log("Great!");
            _Health += Global._HealthPerHit;
          }
          else if(minDistance < _MaxHitDistance * 0.7f)
          {
            Debug.Log("Good.");
            _Health += Global._HealthPerHit * 0.8f;
          }
          else
          {
            Debug.Log("Bad.");
            _Health += Global._HealthPerHit * 0.5f;
          }

          if (hitKey._Duration != 0)
          {
            hitKey._IsHeld = true;
            _PlayerArrows[i]._HeldNote = hitKey;
            hitKey.transform.position = _PlayerArrows[i].transform.position;
          }
          else
          {
            hitKey.GetHit(false);
          }

          _PlayerArrows[i].StartGlow();
        }
        else
        {
          MissNote(Global._HealthPerMiss / 2);
        }
        _PlayerArrows[i]._GotPressResult = true;
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
        key._Duration = note._Duration;
        key._Collider.size = Vector2.one;
        key.UpdateVariables();
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
    _FadeGroup.alpha -= 1 / Global._FadeDuration * Time.deltaTime;
    _ScreenCamera.orthographicSize = Mathf.Lerp(_ScreenCamera.orthographicSize, _DefaultZoom, _ZoomReturnSpeed * Time.deltaTime);


    for (int i = 0; i < Global._PlayingSong._Effects.Count; i++)
    {
      Effect effect = Global._PlayingSong._Effects[i];

      if (!effect._Sent && effect._Time < _Time)
      {
        int type = (int)effect._Type;

        switch (type)
        {
          case 0:
            _ScreenCamera.orthographicSize += _ZoomPerBeat;
            break;
        }
        effect._Sent = true;
      }
    }
  }

  public void StartPause()
  {
    ChangeGameState(Gamestate.PauseState);
  }
  public void EndPause()
  {
    ChangeGameState(Gamestate.GameplayState);
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
  public float BeatsToSeconds(float beats)
  {
    if (Global._PlayingSong == null)
    {
      return -1;
    }

    float seconds = beats * 60;
    seconds /= Global._PlayingSong._BPM;
    return seconds;
  }
  public float SecondsToBeats(float seconds)
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

  void ChangeGameState(Gamestate state)
  {
    _LastGamestate = _Gamestate;
    _Gamestate = state;

    _PauseMenu.SetActive(state == Gamestate.PauseState);
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

  public static bool IsPaused()
  {
    return _Manager._Gamestate == Gamestate.PauseState;
  }
}
