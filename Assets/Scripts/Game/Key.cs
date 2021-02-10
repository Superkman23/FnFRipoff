using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Key : MonoBehaviour
{
  public Sprite _HitSprite;
  SpriteRenderer _Renderer;

  public float _MoveSpeed = 4;
  public float _Time;
  public float _DeathTime;
  public BoxCollider2D _Collider;

  private void Awake()
  {
    _Collider = GetComponent<BoxCollider2D>();
    _Renderer = GetComponent<SpriteRenderer>();
    _DeathTime = -1;
  }

  private void Update()
  {
    if(_DeathTime != -1)
    {
      if(_DeathTime <= 0)
      {
        Destroy(gameObject);
      }

      _DeathTime -= Time.deltaTime;
    }


    if(!LevelManager._Manager._Paused)
      transform.Translate(Vector3.up * _MoveSpeed * Time.deltaTime);
  }

  public void GetHit()
  {
    _Renderer.sprite = _HitSprite;
    _DeathTime = 0;
    _Collider.enabled = false;
  }

}
