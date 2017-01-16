using UnityEngine;

/// <summary>
/// Used to inform <see cref="AudiosBeingPlayedMGR"/> to hold this audiosource to <see cref="Sensors.SensorHearing"/>'s to listen
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class AudioEventMonoB : MonoBehaviour
{
    public enum AudioType
    {
        GunShot
        //Environment,
        //Explosion
    }

    public float updateInterval = .25f;
    public AudioType audioType;
    public Transform owner; // "You need to set owner from 'audio Instantiater MonoBehaviour' if owner is dynamic"

    [System.NonSerialized]
    public AudioSource audioSc;

    private float lastUpTime = -Mathf.Infinity;
    private AudiosBeingPlayedMGR abpMGR;

    public delegate void PlayedAudioHandler(
        AudioSource audioSource,
        Transform owner,
        AudioType audType);

    public event PlayedAudioHandler onAudioPlaying;

    public void PlayingAudio(AudioSource audioSource, Transform owner, AudioType audType)
    {
        if (onAudioPlaying != null)
            onAudioPlaying(audioSource, owner, audType);
    }

    private void Awake()
    {
        audioSc = GetComponent<AudioSource>();
        GameObject envFx = GameObject.FindGameObjectWithTag("EnvFx") as GameObject;
        if (envFx)
            abpMGR = envFx.GetComponent<AudiosBeingPlayedMGR>();
    }

    private void Start()
    {
        abpMGR.AddBeingPlayedAudio(this);
    }

    private void Update()
    {
        if (Time.time - lastUpTime > updateInterval)
        {
            PlayingAudio(audioSc, owner, audioType);
            lastUpTime = Time.time;
        }
    }

    public void OnDestroy()
    {
        abpMGR.RemoveAudio(this);
    }
}