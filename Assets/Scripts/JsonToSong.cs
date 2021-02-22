using System.Collections.Generic;
using UnityEngine;
using System.IO;

public static class JsonToSong
{
  public static Song GetSong(string songName)
  {
    string path = Application.streamingAssetsPath + "/Songs/" + songName + ".json";
    string jsonString = File.ReadAllText(path);
    SongJson songJson = JsonUtility.FromJson<SongJson>(jsonString);
    return SongJsonToSong(songJson);
  }
  public static Song SongJsonToSong(SongJson songJson)
  {
    List<Note> notes = new List<Note>();

    foreach(NoteJson noteJson in songJson.Notes)
    {
      Note.Direction direction = GetDirection(noteJson.Key);
      notes.Add(new Note(direction, noteJson.Beat, noteJson.Duration));
    }
    List<Effect> effects = new List<Effect>();

    foreach (EffectJson effectJson in songJson.Effects)
    {
      Effect.EffectType type = GetEffect(effectJson.Effect);
      effects.Add(new Effect(type, effectJson.Beat));
    }

    return new Song(notes, effects, songJson.BPM, songJson.NoteSpeed);
  }

  public static Note.Direction GetDirection(string input)
  {
    if(input == "Up")
    {
      return Note.Direction.Up;
    }
    if (input == "Down")
    {
      return Note.Direction.Down;
    }
    if (input == "Left")
    {
      return Note.Direction.Left;
    }
    if (input == "Right")
    {
      return Note.Direction.Right;
    }

    return Note.Direction.Up;
  }

  public static Effect.EffectType GetEffect(string input)
  {
    if(input == "Zoom")
    {
      return Effect.EffectType.Zoom;
    }

    return Effect.EffectType.Zoom;
  }

}


//Json versions of notes and songs, only contains needed data.
[System.Serializable]
public class SongJson
{
  //public NoteJson[] Notes;
  public int BPM;
  public float NoteSpeed;
  public NoteJson[] Notes;
  public EffectJson[] Effects;
}

[System.Serializable]
public class NoteJson
{
  public float Beat = 0; // When the note should be hit
  public string Key = "Up";
  public float Duration = 0;
}

[System.Serializable]
public class EffectJson
{
  public float Beat = 0; // when the effect plays
  public string Effect = "Zoom";
}
