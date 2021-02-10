using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//TODO: add music
public class Song
{
  public List<Note> _Notes;
  public float _BPM;

  public Song(List<Note> notes, float bpm)
  {
    _Notes = notes;
    _BPM = bpm;
  }
}

public class Note
{
  public enum Direction
  {
    Left,
    Down,
    Up,
    Right
  }

  public float _Time; // When the note should be hit
  public Direction _Key;
  public bool _Sent; //Ensure that a note isnt sent twice

  public Note(Direction key, float time, bool sent = false)
  {
    _Key = key;
    _Time = time;
    _Sent = sent;
  }
}