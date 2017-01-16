using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Holds events that can be listened by <see cref="Sensors.SensorHearing"/>
/// </summary>
public class AudiosBeingPlayedMGR : MonoBehaviour
{
    public List<AudioEventMonoB> audiosBeingPlayed = new List<AudioEventMonoB>();

    public delegate void NewAudioAddedHandler(AudioEventMonoB mb, AudioSource audioSource, Transform owner, AudioEventMonoB.AudioType audType);

    public event NewAudioAddedHandler onAudioStartedInstantiated;

    public void AudioAdded(AudioEventMonoB mb, AudioSource audioSource, Transform owner, AudioEventMonoB.AudioType audType)
    {
        if (onAudioStartedInstantiated != null)
        {
            onAudioStartedInstantiated(mb, audioSource, owner, audType);
        }
    }

    public delegate void AudioDestroyedHandler(AudioEventMonoB mb, AudioSource audioSource, Transform owner, AudioEventMonoB.AudioType audType);

    public event AudioDestroyedHandler onAudioDestroyed;

    public void AudioDestroyed(AudioEventMonoB mb, AudioSource audioSource, Transform owner, AudioEventMonoB.AudioType audType)
    {
        if (onAudioDestroyed != null)
        {
            onAudioDestroyed(mb, audioSource, owner, audType);
        }
    }

    public void AddBeingPlayedAudio(AudioEventMonoB aemb)
    {
        audiosBeingPlayed.Add(aemb);
        AudioAdded(aemb, aemb.audioSc, aemb.owner, aemb.audioType);
    }

    public void RemoveAudio(AudioEventMonoB aemb)
    {
        if (audiosBeingPlayed.Contains(aemb))
        {
            audiosBeingPlayed.Remove(aemb);
            AudioDestroyed(aemb, aemb.audioSc, aemb.owner, aemb.audioType);
        }
    }
}