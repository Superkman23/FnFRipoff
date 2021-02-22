using UnityEngine;

public class Arrow : MonoBehaviour
{
  [HideInInspector] public SpriteRenderer _Renderer;
  public bool _Pressed = false;
  public bool _GotPressResult = false;
  public Key _HeldNote;


  public SpriteRenderer _GlowSpriteRenderer;
  public float _FadeTime;

  public Color _PressedColor;
  public Color _UnpressedColor;
  public Color _GlowColor;

  private void Awake()
  {
    _Renderer = GetComponent<SpriteRenderer>();
    _GlowSpriteRenderer.color = new Color(_GlowColor.r, _GlowColor.g, _GlowColor.b, 0);
  }

  private void Update()
  {
    if (_Pressed)
    {
      _Renderer.color = _PressedColor;
    }
    else
    {
      _Renderer.color = _UnpressedColor;
    }

    _GlowSpriteRenderer.color = new Color(_GlowSpriteRenderer.color.r, _GlowSpriteRenderer.color.g, _GlowSpriteRenderer.color.b, _GlowSpriteRenderer.color.a - Time.deltaTime / _FadeTime);

  }

  public void StartGlow()
  {
    _GlowSpriteRenderer.color = _GlowColor;
  }


}
