using UnityEngine;

// this class implements the whole feet planting solution for a specific animator
public class FootPlanting
{
    private AnimatorKinematics m_AnimatorKinematics = null;

    // play steps sound
    public System.Action OnLeftFootPlantAction = null;

    public System.Action OnRightFootPlantAction = null;

    // Decals script reference to get footstep fx
    private Decals decalScript;

    // The different surfaces and their sounds.
    public AudioSurface[] surfaces;

    private FootStepSoundsAndFx footStepProps;
    private Animator m_Animator;
    private readonly float c_FootRayStartUpFix = .05f;

    public FootPlanting(FootStepSoundsAndFx _props, Animator _animator)
    {
        footStepProps = _props;
        surfaces = _props.surfaces;
        m_AnimatorKinematics = new AnimatorKinematics(_animator);
        m_Animator = _animator;
        OnLeftFootPlantAction += OnLeftFootPlant;
        OnRightFootPlantAction += OnRightFootPlant;
        decalScript = GameObject.FindGameObjectWithTag("EnvFx").GetComponent<Decals>();
    }

    public void FootPlantOnAnimatorIK(int layerIndex)
    {
        if (layerIndex == 0)
        {
            float deltaTime = Time.deltaTime;

            // update animator kinematics
            m_AnimatorKinematics.Update(deltaTime);

            // foot plant actions for left and right
            if (m_AnimatorKinematics.m_LeftFootStabilizer.m_Stabilizing && OnLeftFootPlantAction != null)
                OnLeftFootPlantAction();
            if (m_AnimatorKinematics.m_RightFootStabilizer.m_Stabilizing && OnRightFootPlantAction != null)
                OnRightFootPlantAction();
        }
    }

    // get foot bottom position
    private Vector3 GetFootPosition(bool left)
    {
        AvatarIKGoal ikGoal = left ? AvatarIKGoal.LeftFoot : AvatarIKGoal.RightFoot;
        float footBottomHeight = left ? m_Animator.leftFeetBottomHeight : m_Animator.rightFeetBottomHeight;

        Vector3 footPos = m_Animator.GetIKPosition(ikGoal);
        Quaternion footRot = m_Animator.GetIKRotation(ikGoal);

        footPos += footRot * new Vector3(0, -footBottomHeight, 0);

        return footPos;
    }

    private void OnLeftFootPlant()
    {
        FootFallSoundAndFootStepFX(true);
    }

    private void OnRightFootPlant()
    {
        FootFallSoundAndFootStepFX(false);
    }

    private void FootFallSoundAndFootStepFX(bool left)
    {
        Vector3 position = GetFootPosition(left);
        RaycastHit hit;
        if (!Physics.Raycast(position + Vector3.up * c_FootRayStartUpFix, -Vector3.up, out hit, 1.5f, footStepProps.footStepSoundAndFxLayermask))
            return;

        // FootStep Fx
        if (footStepProps.useFootStepFx)
        {
            GameObject stepFxPrefab = decalScript.getRandomFootStepFx(hit.collider.tag);
            if (stepFxPrefab)
            {
                GameObject.Instantiate(stepFxPrefab, hit.point, Quaternion.FromToRotation(Vector3.up, hit.normal));
            }
        }

        // FootStep Sound
        if (!footStepProps.useFootStepSound)
            return;

        for (int i = 0; i < surfaces.Length; i++)
        {
            if (surfaces[i].tag == hit.collider.tag)
            {
                surfaces[i].PlayRandomClip();
            }
        }
    }
}

[System.Serializable]
public class AudioSurface
{
    public string tag;              // The tag on the surfaces that play these sounds.
    public AudioClip[] clips;       // The different clips that can be played on this surface.
    public AudioSource source;      // The AudioSource that will play the clips.

    public void PlayRandomClip()
    {
        // If there are no clips to play return.
        if (clips == null || clips.Length == 0)
            return;

        // Find a random clip and play it.
        int index = Next(clips.Length);
        if (source.gameObject.activeSelf)
            source.PlayOneShot(clips[index]);
    }

    private int[] randomIndices = null;
    private int randomIndex = 0;
    private int prevValue = -1;

    public int Next(int len)
    {
        if (len <= 1)
            return 0;

        if (randomIndices == null || randomIndices.Length != len)
        {
            randomIndices = new int[len];
            for (int i = 0; i < randomIndices.Length; i++)
                randomIndices[i] = i;
        }

        if (randomIndex == 0)
        {
            int count = 0;
            do
            {
                for (int i = 0; i < len - 1; i++)
                {
                    int j = Random.Range(i, len);
                    if (j != i)
                    {
                        int tmp = randomIndices[i];
                        randomIndices[i] = randomIndices[j];
                        randomIndices[j] = tmp;
                    }
                }
            } while (prevValue == randomIndices[0] && ++count < 10); // Make sure the new first element is different from the last one we played
        }

        int value = randomIndices[randomIndex];
        if (++randomIndex >= randomIndices.Length)
            randomIndex = 0;

        prevValue = value;
        return value;
    }
}