using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Key : MonoBehaviour
{
  public float _MoveSpeed = 4;
  public float _Time;
  public BoxCollider2D _Collider;
  public KeyTrail _Trail;
  public float _Duration;
  public bool _IsHeld;
  public bool _HasBeenHit;

  private void Awake()
  {
    _Collider = GetComponent<BoxCollider2D>();
    UpdateVariables();
  }

  private void Update()
  {
    if (_IsHeld)
    {
      _Duration -= LevelManager._Manager.SecondsToBeats(Time.deltaTime);
      UpdateVariables();
    }
    if(_Duration < 0)
    {
      GetHit(false);
    }

    if(!LevelManager._Manager._Paused && !_IsHeld)
      transform.Translate(Vector3.up * _MoveSpeed * Time.deltaTime);
  }

  public void UpdateVariables()
  {
    if (_Trail != null)
    {
      _Trail.transform.localScale = new Vector3(.3f, _MoveSpeed * _Duration * 60 / Global._PlayingSong._BPM, 1);
      _Trail.transform.localPosition = new Vector3(0, _Duration * _MoveSpeed * -30 / Global._PlayingSong._BPM, 0);
      _Trail._MoveSpeed = _MoveSpeed;
    }
  }


  public void GetHit(bool missed)
  {
    if (missed)
    {
      _Trail.transform.parent = null;
      if (!_HasBeenHit)
        LevelManager._Manager.MissNote();
    }
    Destroy(gameObject);
  }
}
