using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MissBox : MonoBehaviour
{

  private void Update()
  {
    Collider2D[] colliders = Physics2D.OverlapBoxAll(transform.position, transform.localScale, 0);

    foreach (Collider2D collider in colliders)
    {
      Key key = collider.GetComponent<Key>();
      if (key)
      {
        key.GetHit();
        LevelManager._Manager.MissNote();
      }
    }
  }
}
