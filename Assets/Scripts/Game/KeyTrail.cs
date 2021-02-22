using UnityEngine;

public class KeyTrail : MonoBehaviour
{
  public float _MoveSpeed;
  public SpriteRenderer _Renderer;
  private void Awake()
  {
    _Renderer = GetComponent<SpriteRenderer>();
  }

  void Update()
  {
    if(transform.parent == null && !LevelManager.IsPaused())
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
