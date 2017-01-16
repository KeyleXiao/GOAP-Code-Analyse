using UnityEngine;

[System.Serializable]
public class FootStepSoundsAndFx
{
    public bool useFootStepSound = true;
    public bool useFootStepFx = false;
    public LayerMask footStepSoundAndFxLayermask;

    // The different surfaces and their sounds.
    public AudioSurface[] surfaces;
}