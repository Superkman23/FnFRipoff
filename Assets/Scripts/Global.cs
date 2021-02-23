using UnityEngine;

public static class Global
{
  public static Song _PlayingSong;

  //Difficulty Settings
  public static float _NoteSpeedMultiplier = 1;
  public static float _PrecisionScale = 1f;
  public static float _MaxArrowPressTime = 0.1f;
  public static float _HealthPerHit = 2;
  public static float _HealthPerMiss = -4;

  //Fade Settings
  public static float _FadeDuration = 1f;

  public static int _TargetFrameRate = 60;

}