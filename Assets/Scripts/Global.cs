using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Global
{
  public static Song _PlayingSong;

  //Difficulty Settings
  public static float _NoteSpeedMultiplier = 1;
 // public static float _HitColliderSize = 1f; //this will be replaced by a precision modifier, which will affect how close you need to be to the note
  public static float _MaxArrowPressTime = 0.1f;
  public static float _HealthPerHit = 2;
  public static float _HealthPerMiss = -4;
}
