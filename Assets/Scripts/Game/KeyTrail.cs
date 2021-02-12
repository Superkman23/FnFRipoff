using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyTrail : MonoBehaviour
{
  public float _MoveSpeed;
  void Update()
  {
    if(transform.parent == null && !LevelManager._Manager._Paused)
    {
      transform.Translate(Vector3.up * _MoveSpeed * Time.deltaTime);

      //just some random number so we can save a bit of performance
      if(transform.position.y > 100)
      {
        Destroy(gameObject);
      }
    }
  }
}
