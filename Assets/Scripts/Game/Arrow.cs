using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Arrow : MonoBehaviour
{
  [HideInInspector] public SpriteRenderer _Renderer;
  public float _PressedTime = 0;
  public bool _HitThisPress = false;

  private void Awake()
  {
    _Renderer = GetComponent<SpriteRenderer>();
  }
}
