
using UnityEngine;

public class TrialHandler : SingletonMonoBehavior<TrialHandler>
{
    [Tooltip("The valid angle on either side of the hole for error"), Range(0, 180),SerializeField] private float maxErrorAngle;
    [SerializeField] private float maxErrorDistance;
    [SerializeField] private GolfBallController controller;
    [SerializeField] private Transform holePosition;


    void OnDrawGizmos()
    {
        Vector3 forward = holePosition.forward * maxErrorDistance;
        Vector3 leftBoundary = Quaternion.Euler(0, -maxErrorAngle, 0) * forward;
        Vector3 rightBoundary = Quaternion.Euler(0, maxErrorAngle, 0) * forward;

        Gizmos.color = Color.red;

        Gizmos.DrawWireSphere(holePosition.position, maxErrorDistance);

        Gizmos.DrawLine(holePosition.position, holePosition.position + leftBoundary);
        Gizmos.DrawLine(holePosition.position, holePosition.position + rightBoundary);
    }

}
