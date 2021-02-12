using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Key : MonoBehaviour
{
  public float _MoveSpeed = 4;
  public float _Time;
  public BoxCollider2D _Collider;
  public Transform _Trail;
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
      GetHit();
    }

    if(!LevelManager._Manager._Paused && !_IsHeld)
      transform.Translate(Vector3.up * _MoveSpeed * Time.deltaTime);
  }

  public void UpdateVariables()
  {
    if (_Trail != null)
    {
      _Trail.localScale = new Vector3(.3f, _MoveSpeed * _Duration * 60 / Global._PlayingSong._BPM, 1);
      _Trail.localPosition = new Vector3(0, _Duration * _MoveSpeed * -30 / Global._PlayingSong._BPM, 0);
    }
  }


  public void GetHit()
  {
    Destroy(gameObject);
  }
}
