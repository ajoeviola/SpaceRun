using UnityEngine;
using System;
using UnityEngine.Audio;

public class AudioManager : Singleton<AudioManager>
{
    [Header("Set in Inspector")]
    public Sound[] sounds;

    public void Play(string name)
    {
        foreach (Sound s in sounds)
        {
            s.source = gameObject.AddComponent<AudioSource>();
            s.source.clip = s.clip;
        }

        Sound sound = Array.Find(sounds, sounds => sounds.name == name);
        sound.source.Play();
         
    }

    public void Pause(string name)
    {
        foreach (Sound s in sounds)
        {
            s.source = gameObject.AddComponent<AudioSource>();
            s.source.clip = s.clip;
        }

        Sound sound = Array.Find(sounds, sounds => sounds.name == name);
        sound.source.Pause();

    }
}
