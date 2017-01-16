using System.Collections.Generic;
using UnityEngine;

public class PatrolRoute : MonoBehaviour
{
    public List<Vector3> patrolPoints;

    private void OnDrawGizmos()
    {
        if (patrolPoints.Count <= 1)
            return;

        for (int i = 0; i < patrolPoints.Count; i++)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawRay(patrolPoints[i], Vector3.up);
            Gizmos.color = Color.white;
            if (i == patrolPoints.Count - 1)
            {
                Gizmos.DrawLine(patrolPoints[i], patrolPoints[0]);
                continue;
            }
            else
            {
                Gizmos.DrawLine(patrolPoints[i], patrolPoints[i + 1]);
                continue;
            }
        }
    }

    public void StickToGround()
    {
        int i = 0; float offsetY = .1f;
        foreach (Vector3 point in patrolPoints)
        {
            RaycastHit hit;
            if (Physics.Raycast(point, Vector3.down, out hit, 500f))
            {
                patrolPoints[i] = hit.point + Vector3.up * offsetY;
            }
            i++;
        }
    }
}