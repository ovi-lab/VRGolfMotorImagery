using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

[RequireComponent(typeof(TrialHandler))]
public class TrialHandler : SingletonMonoBehavior<TrialHandler>
{
    [Range(0, 180),SerializeField] private float maxErrorAngle;
    [SerializeField] private float minErrorDistance;
    [SerializeField] private float maxErrorDistance;
    [SerializeField] private GolfBallController controller;
    [SerializeField] private Transform holePosition;
    [SerializeField] private float meanRadialError;
    [SerializeField] private float totalTrialCount;
    [Range(0, 100), SerializeField] private float percentageError;
    [SerializeField] private TextMeshProUGUI tmp;


    private List<float> radialErrors;
    private Vector3 errorPosition;

    private void Start()
    {
        radialErrors = RandomValueGenerator.GenerateValues((int)(totalTrialCount * percentageError * 0.01f),
            meanRadialError, minErrorDistance, maxErrorDistance);
        errorPosition = holePosition.position;
    }

    public void Misfire()
    {
        float randomAngle = Random.Range(-maxErrorAngle, maxErrorAngle);
        errorPosition = controller.HoleTransform.position + Quaternion.AngleAxis(randomAngle, transform.up) * transform.forward * radialErrors[0];
        controller.ResetBall();
        controller.FireBall(errorPosition);
        tmp.text = "[DEBUG]\nError Trial";
    }


    void OnDrawGizmos()
    {
        Vector3 forward = holePosition.forward * maxErrorDistance;
        Vector3 leftBoundary = Quaternion.Euler(0, -maxErrorAngle, 0) * forward;
        Vector3 rightBoundary = Quaternion.Euler(0, maxErrorAngle, 0) * forward;

        Gizmos.color = Color.red;

        Gizmos.DrawWireSphere(holePosition.position, maxErrorDistance);
        Gizmos.DrawLine(holePosition.position, holePosition.position + leftBoundary);
        Gizmos.DrawLine(holePosition.position, holePosition.position + rightBoundary);

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(errorPosition, 0.1f);

    }

}
