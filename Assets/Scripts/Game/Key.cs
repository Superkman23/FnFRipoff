using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Key : MonoBehaviour
{
  public float _MoveSpeed = 4;
  public float _Time;
  public BoxCollider2D _Collider;

  private void Awake()
  {
    _Collider = GetComponent<BoxCollider2D>();
  }

  private void Update()
  {
    if(!LevelManager._Manager._Paused)
      transform.Translate(Vector3.up * _MoveSpeed * Time.deltaTime);
  }

  public void GetHit()
  {
    Destroy(gameObject);
  }

}
