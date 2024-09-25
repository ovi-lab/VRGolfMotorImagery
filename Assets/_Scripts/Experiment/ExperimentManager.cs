
using System;
using UnityEngine;

[RequireComponent(typeof(TrialHandler))]
public class ExperimentManager : SingletonMonoBehavior<ExperimentManager>
{
    [SerializeField] private GolfBallController golfBall;
    [SerializeField] private Transform hole;

    private TrialHandler trialHandler;
    private bool correctTrial = true;
    private void OnEnable()
    {
        trialHandler = GetComponent<TrialHandler>();
        InputHandler.Instance.OnButtonPress += HandleInput;
    }

    private void OnDisable()
    {
        InputHandler.Instance.OnButtonPress -= HandleInput;
    }

    private void HandleInput()
    {
        if (golfBall.IsMoving) return;
        if (correctTrial)
        {
            golfBall.FireBall(hole.position);
            correctTrial = false;
        }
        else
        {
            trialHandler.Misfire();
        }
    }
}
