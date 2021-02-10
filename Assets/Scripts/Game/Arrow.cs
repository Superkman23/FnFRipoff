using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Arrow : MonoBehaviour
{
  [HideInInspector] public SpriteRenderer _Renderer;
  public float _PressedTime = 0;
  public bool _HitThisPress = false;

  public Color _PressedColor;
  public Color _UnpressedColor;

  private void Awake()
  {
    _Renderer = GetComponent<SpriteRenderer>();
  }

  private void Update()
  {
    if (_PressedTime > 0)
    {
      _Renderer.color = _PressedColor;
    }
    else
    {
      _Renderer.color = _UnpressedColor;
    }
  }
}
