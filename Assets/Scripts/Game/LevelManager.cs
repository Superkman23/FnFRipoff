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

  [Header("Effects")]
  //Zooming
  public Camera _ScreenCamera;
  public float _DefaultZoom;
  public float _ZoomReturnSpeed;
  //Fading
  public CanvasGroup _FadeGroup;
  //Hit Text
  public GameObject _HitTextPrefab;

  [Header("Notes")]
  public float _NoteSpawnY = -6;
  public GameObject[] _NotePrefabs;

  [Header("Arrows")]
  public Arrow[] _PlayerArrows; // Order is Left Down Up Right
  public Transform _PlayerArrowsTransform;
  [Range(0, 1)] public float _MaxHitDistance = 1; // If notes are outside here dont even bother checking them, you can't hit them
  [Range(0, 1)] public float _PerfectHitRange = 0.05f;
  [Range(0, 1)] public float _GreatHitRange = 0.4f;
  [Range(0, 1)] public float _GoodHitRange = 0.7f;

  public Color _PressedColor;
  public Color _UnpressedColor;

  [Header("Song")] // All in beats
  public float _Time; // Current time
  public int _StartingDelay; // beats before the song starts
  public int _EndDelay;  // how many beats before the song ends
  [HideInInspector] public float _LastBeat = -1; // When is there nothing left to play?

  [Header("Misc")]
  public float _Health = 50;
  public Text _BeatText;
  public Image _HealthBar;
  public Text _ScoreText;
  public float _Score;
  public AnimationCurve _ScoreScaling;
  public AudioSource _Backing;
  bool _StartedSong;

  private void Awake()
  {
    if (Global._PlayingSong == null) //Song didnt properly load, load a default one instead.
    {
      Global._PlayingSong = JsonToSong.GetSong("TestSong");
    }
    _Backing.clip = Global._PlayingSong._BackTrack;

    _FadeGroup.alpha = 1;
    if (_Manager == null)
    {
      _Manager = this;
    }
    else
    {
      Destroy(this);
    }
    EndPause();

    _Time -= SecondsToBeats(Global._FadeDuration) + _StartingDelay;
  }

  private void Update()
  {
    if(_Time > 0 && !_StartedSong)
    {
      _Backing.time = BeatsToSeconds(_Time);
      _Backing.Play();
      _StartedSong = true;
    }

    if (Input.GetKeyDown(KeyCode.Return) && 
      _Gamestate != Gamestate.LoseState &&
      _Gamestate != Gamestate.WinState)
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

    if (_Gamestate == Gamestate.WinState)
    {
      _FadeGroup.alpha += 1 / Global._FadeDuration * Time.deltaTime;
      if (_FadeGroup.alpha == 1)
      {
        SceneManager.LoadScene("Main Menu");
      }
      return;
    }
    if (_Gamestate == Gamestate.LoseState)
    {
      SceneManager.LoadScene("Main Menu");
    }



    if (_StartedSong)
    {
      _Time = SecondsToBeats(_Backing.time);
    }
    else
    {
      float elapsedTime = SecondsToBeats(Time.deltaTime);
      if (elapsedTime != -1)
      {
        _Time += elapsedTime;
      }
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

  private void OnDrawGizmos()
  {
    Gizmos.color = Color.red;
    for (int i = 0; i < _PlayerArrows.Length; i++)
    {
      // Multiplying by 2 to make it a distance to 0 rather than distance across, provides a better visual
      Gizmos.DrawWireCube(_PlayerArrows[i].transform.position, Vector3.one * 2 * _MaxHitDistance);
      Gizmos.DrawWireCube(_PlayerArrows[i].transform.position, Vector3.one * 2 * _MaxHitDistance * _PerfectHitRange);
      Gizmos.DrawWireCube(_PlayerArrows[i].transform.position, Vector3.one * 2 * _MaxHitDistance * _GreatHitRange);
      Gizmos.DrawWireCube(_PlayerArrows[i].transform.position, Vector3.one * 2 * _MaxHitDistance * _GoodHitRange);
    }
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
          _Score += SecondsToBeats(Time.deltaTime) * 100;
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

        float minDistance = _MaxHitDistance;

        foreach (Collider2D collider in colliders)
        {
          Key key = collider.GetComponent<Key>();
          if (key)
          {
            float distance = Mathf.Abs(_PlayerArrows[i].transform.position.y - collider.transform.position.y);
            if (distance < _MaxHitDistance && (!hitKey || distance < minDistance))
            {
              hitKey = key;
              minDistance = distance;
            }
          }
        }
        if (hitKey)
        {

          HitText hit = Instantiate(_HitTextPrefab, Vector3.zero, Quaternion.identity).GetComponent<HitText>();
          if (minDistance < _MaxHitDistance * _PerfectHitRange)
          {
            hit._Text.text = "Perfect!";
          }
          else if (minDistance < _MaxHitDistance * _GreatHitRange)
          {
            hit._Text.text = "Great!";
          }
          else if(minDistance < _MaxHitDistance * _GoodHitRange)
          {
            hit._Text.text = "Good";
          }
          else
          {
            hit._Text.text = "Ok.";
          }

          float percent = Mathf.Abs(1 - (minDistance / _MaxHitDistance));
          _Score += Mathf.RoundToInt(_ScoreScaling.Evaluate(percent) * 100);
          _Health += Mathf.RoundToInt(Global._HealthPerHit * _ScoreScaling.Evaluate(percent));

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
          _Score -= 10;
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
  }
  void ManageUI()
  {
    _BeatText.text = "Beat: " + Mathf.Floor(_Time);

    _ScoreText.text = "Score:\n" + Mathf.RoundToInt(_Score);

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
            _ScreenCamera.orthographicSize += effect._Scale * 0.2f;
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

  public void MissNote(float damage)
  {
    _Health += damage;
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
    float distance = Mathf.Abs(_NoteSpawnY - _PlayerArrowsTransform.position.y);
    float timeToCross = distance / Global._PlayingSong._NoteSpeed * Global._NoteSpeedMultiplier;

    return SecondsToBeats(hitTime - timeToCross);
  }
  public float DistanceFromExtraBeats(float beats)
  {
    float time = BeatsToSeconds(beats);
    float distance = Mathf.Abs(_NoteSpawnY - _PlayerArrowsTransform.position.y);
    float timeToCross = distance / Global._PlayingSong._NoteSpeed * Global._NoteSpeedMultiplier;
    float percentExtra = time / timeToCross;

    return distance * percentExtra;
  }

  void ChangeGameState(Gamestate state)
  {
    _PauseMenu.SetActive(state == Gamestate.PauseState);
    if(state == Gamestate.PauseState || state == Gamestate.WinState)
    {
      _Backing.Pause();
    } 
    else if(_Gamestate == Gamestate.PauseState) 
    {
      _Backing.UnPause();
    }

    _LastGamestate = _Gamestate;
    _Gamestate = state;
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
