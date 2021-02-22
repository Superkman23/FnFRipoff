using System.Collections.Generic;
//TODO: add music
public class Song
{
  public List<Note> _Notes;
  public List<Effect> _Effects;
  public float _BPM;
  public float _NoteSpeed;

  public Song(List<Note> notes, List<Effect> effects, float bpm, float noteSpeed)
  {
    _Notes = notes;
    _Effects = effects;
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

public class Effect
{
  public enum EffectType
  {
    Zoom
  }
  public float _Time;
  public EffectType _Type;
  public bool _Sent;

  public Effect(EffectType type, float time, bool sent = false)
  {
    _Type = type;
    _Time = time;
    _Sent = sent;
  }

}
