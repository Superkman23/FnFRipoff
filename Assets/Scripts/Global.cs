using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Global
{
  public static Song _PlayingSong;

  //Difficulty Settings
  public static float _NoteSpeedMultiplier = 1;
  public static float _HitColliderSize = 0.8f;
  public static float _MaxArrowPressTime = 0.1f;
  public static float _HealthPerHit = 2;
  public static float _HealthPerMiss = -4;
}
