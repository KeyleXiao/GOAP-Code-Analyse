using System.Collections.Generic;
using UnityEngine;

public class GameCam : MonoBehaviour
{
    [System.Serializable]
    public class PosNParent
    {
        public Transform parent;
        public Vector3 localPos;
        public Vector3 localRot;
    }

    public int index;
    public bool testMode = false;
    public List<PosNParent> allCamPositions = new List<PosNParent>();

    private void Start()
    {
    }

    private void Update()
    {
        if (allCamPositions.Count == 0 || index >= allCamPositions.Count)
            return;

        transform.SetParent(allCamPositions[index].parent);
        if (!testMode)
        {
            transform.localPosition = allCamPositions[index].localPos;
            transform.localRotation = Quaternion.Euler(allCamPositions[index].localRot);
        }

        if (Input.GetKeyDown(KeyCode.RightArrow))
            index = ++index % allCamPositions.Count;
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
            index = --index < 0 ? allCamPositions.Count - 1 : 0;
    }
}