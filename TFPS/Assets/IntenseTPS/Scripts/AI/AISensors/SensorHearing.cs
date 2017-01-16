using Information;
using System.Collections.Generic;
using UnityEngine;

namespace Sensors
{
    /// <summary>
    /// Event driven sensor that listens sources 'indirectly' which <see cref="AudioEventMonoB"/> script is attached, (<see cref="AudiosBeingPlayedMGR"/> need to exist in scene)
    /// </summary>
    public class SensorHearing : AISensorTrigger
    {
#if UNITY_EDITOR
        public bool showHearingShapes = true;

        [HideInInspector]
        public GUIStyle textGuiStyle = new GUIStyle();

        public Color color1 = Color.blue;
        public Color color2 = Color.green;
        public Color color3 = Color.green;
        public int labelsFontSize = 10;
        public Color labelsFontColor = Color.white;
        public float labelsDisableDistance = 80;
        public Font labelsFont;
#endif
        public float highIntensityRadius = 5;
        public float maxHearingRadius = 50;
        private Transform head;
        public bool useVolumeLevel = true;
        public float iLevel_a = 80;
        public float iLevel_b = 50;
        public bool useFallOff = true; // appliy falloff outside of highIntensity radius according to distance from head
        public Vector2 inBetweenVolumeLevel = new Vector2(.1f, .75f); // if volume is equal to or more than y volume intensity is considered %100 / if less than or equal to x value %0
        private AudiosBeingPlayedMGR abpMGR;
        private AIBrain ai;

        private List<ListeningSourceVar> currentlyListeningSources; // for quick finding sources in OnPlayingAudio event

        private class ListeningSourceVar
        {
            public InformationSuspicion info;
            public Transform transformId;
            public AudioSource audioSource;

            public ListeningSourceVar(InformationSuspicion _info, Transform _ownerOrAudSrc, AudioSource _audioSource)
            {
                info = _info;
                transformId = _ownerOrAudSrc;
                audioSource = _audioSource;
            }
        }

        public override void OnStart(AIBrain _ai)
        {
            ai = _ai;
            abpMGR = GameObject.FindGameObjectWithTag("EnvFx").GetComponent<AudiosBeingPlayedMGR>();
            abpMGR.onAudioStartedInstantiated += OnAudioAdded;
            abpMGR.onAudioDestroyed += OnAudioRemoved;
            currentlyListeningSources = new List<ListeningSourceVar>();

            head = ai.Transform;
        }

        public void OnAudioAdded(AudioEventMonoB mb, AudioSource audSrc, Transform owner, AudioEventMonoB.AudioType audType)
        {
            if (owner != null && (owner == ai.Transform || ai.Memory.isFriend(owner)))
                return;
            mb.onAudioPlaying += OnPlayingAudio;

            var info = ai.Memory.GetSuspicionWithBaseTransform<InformationSuspicion>(owner);
            if (info != null)
            {
                currentlyListeningSources.Add(new ListeningSourceVar(info, owner ? owner : audSrc.transform, audSrc));
            }
            else
            {
                // Create info
                InformationSuspicion newInfo = new InformationSuspicion("Unknown source Audio", audSrc.transform.position, 0);
                ai.Memory.Add(newInfo);
                // Add to listened infos for quick access
                currentlyListeningSources.Add(new ListeningSourceVar(newInfo, owner ? owner : audSrc.transform, audSrc));
            }
        }

        public void OnPlayingAudio(AudioSource audSrc, Transform owner, AudioEventMonoB.AudioType audType)
        {
            if (owner != null && (owner == ai.Transform || ai.Memory.isFriend(owner))) // write better
                return;

            float hearingPerc = GetHearingPercentage(audSrc.transform) / 100;

            // if info with owner is added by visual sensor(after creating a suspected hearing info "without BaseTransform" in OnAudioAdded), we should update the right info and remove old
            var info = ai.Memory.GetSuspicionWithBaseTransform<InformationSuspicion>(owner);

            if (info != null)
            {
                info.SuspicionFirm += hearingPerc;
                info.Update(owner.position, info.lastKnownPosition.Confidence /*or 0*/); // also update last known position
            }
            else // No visual sense yet
            {
                ListeningSourceVar sourceVar = currentlyListeningSources.Find(x => x.transformId == owner ? owner : audSrc.transform);
                if (sourceVar != null)
                {
                    sourceVar.info.Update(sourceVar.transformId.position, hearingPerc/*sourceVar.info.lastKnownPosition.Confidence*//*or 0*/); // also update last known position
                }
                else
                {
#if UNITY_EDITOR
                    //Debug.LogError("Wrong organization of hearing sensor variables");
#endif
                }
            }
        }

        public void OnAudioRemoved(AudioEventMonoB mb, AudioSource audSrc, Transform owner, AudioEventMonoB.AudioType audType)
        {
            if (owner != null && (owner == ai.Transform || ai.Memory.isFriend(owner)))
                return;
            mb.onAudioPlaying -= OnPlayingAudio;

            for (int i = 0; i < currentlyListeningSources.Count; i++)
            {
                if (currentlyListeningSources[i].audioSource == audSrc)
                {
                    currentlyListeningSources.RemoveAt(i);
                }
            }
        }

        public float GetHearingPercentage(Transform target)
        {
            // hearing percentage decreased/increased linear by distance&volume
            if (!target)
                return 0;
            AudioSource source = target.GetComponent<AudioSource>();
            if (!source)
                return 0;
            float distToTarget = Vector3.Distance(target.position, head.position);
            if (!source.isPlaying || distToTarget > maxHearingRadius)
                return 0;
            float volI = target.GetComponent<AudioSource>().volume;
            if (distToTarget < highIntensityRadius)
                if (useVolumeLevel)
                    return iLevel_a;
                else
                    return iLevel_a;
            else
            {
                float outI = iLevel_b;
                outI = outI * (-distToTarget + maxHearingRadius) / (maxHearingRadius - highIntensityRadius); // max iLevel_b, min 0

                volI = Mathf.Clamp(volI, inBetweenVolumeLevel.x, inBetweenVolumeLevel.y);
                volI = (volI - inBetweenVolumeLevel.x) / (inBetweenVolumeLevel.y - inBetweenVolumeLevel.x);
                return outI * volI;
            }
        }
    }
}