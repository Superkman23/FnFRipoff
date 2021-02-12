using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class JsonToSong : MonoBehaviour
{
  private void Start()
  {
    if(Global._PlayingSong == null)
      Global._PlayingSong = GetSong("TestSong");
  }

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
    return new Song(notes, songJson.BPM, songJson.NoteSpeed);
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
    //Make sure everything doesn't break by sending up as a last resort
    return Note.Direction.Up;
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
}

[System.Serializable]
public class NoteJson
{
  public float Beat = 0; // When the note should be hit
  public string Key = "Up";
  public float Duration = 0;
}