using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HitText : MonoBehaviour
{
  public Text _Text;
  public float _FadeDuration;
  public float _RiseSpeed;
  public void Update()
  {
    _Text.color = new Color(_Text.color.r, _Text.color.g, _Text.color.b, _Text.color.a - (1 / _FadeDuration * Time.deltaTime));
    if(_Text.color.a <= 0)
    {
      Destroy(gameObject);
    }
    else
    {
      transform.Translate(Vector3.up * _RiseSpeed * Time.deltaTime);
    }
  }


}
