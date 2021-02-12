using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//TODO: add music
public class Song
{
  public List<Note> _Notes;
  public float _BPM;
  public float _NoteSpeed;

  public Song(List<Note> notes, float bpm, float noteSpeed)
  {
    _Notes = notes;
    _BPM = bpm;
    _NoteSpeed = noteSpeed;
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
  public float _Duration; //For notes that you need to hold a key down for, in beats. 0 represents a basic note.

  public Note(Direction key, float time, float duration = 0, bool sent = false)
  {
    _Key = key;
    _Time = time;
    _Duration = duration;
    _Sent = sent;
  }
}