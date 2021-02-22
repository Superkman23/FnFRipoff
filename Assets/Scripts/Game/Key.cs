using UnityEngine;

public class Key : MonoBehaviour
{
  public float _MoveSpeed = 4;
  public float _Time;
  public BoxCollider2D _Collider;
  public KeyTrail _Trail;
  public float _Duration;
  public bool _IsHeld;

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

    if(!LevelManager.IsPaused() && !_IsHeld)
      transform.Translate(Vector3.up * _MoveSpeed * Time.deltaTime);
  }

  public void UpdateVariables()
  {
    if (_Trail != null)
    {
      if (!_Trail._Renderer)
      {
        _Trail._Renderer = _Trail.GetComponent<SpriteRenderer>();
      }
      _Trail._Renderer.size = new Vector3(.3f, _MoveSpeed * _Duration * 60 / Global._PlayingSong._BPM, 1);
      _Trail.transform.localPosition = new Vector3(0, _Duration * _MoveSpeed * -30 / Global._PlayingSong._BPM, 0);
      _Trail._MoveSpeed = _MoveSpeed;
    }
  }


  public void GetHit(bool missed)
  {
    if (missed)
    {
      ReleaseTrail();
      LevelManager._Manager.MissNote();
    }
    Destroy(gameObject);
  }

  public void ReleaseTrail()
  {
    _Trail.transform.parent = null;
  }
}
