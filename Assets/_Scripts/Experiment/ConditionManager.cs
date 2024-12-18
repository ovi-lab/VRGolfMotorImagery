﻿using System;
using System.Collections.Generic;
using System.Linq;
using EditorAttributes;
using UnityEditor;
using UnityEngine;
using Random = System.Random;

public class ConditionManager : MonoBehaviour
{
    [SerializeField] private bool enableGizmos;

    [Header("Experiment Settings")]
    [SerializeField] private int blockCount;
    [SerializeField] private int trialCountPerBlock;
    [SerializeField] private int minErroneousTrialsPerBlock;
    [Validate("Invalid Parameters for Random Condition", nameof(isRandomConditionInvalid), buildKiller:true)]
    [SerializeField] private int maxErroneousTrialsPerBlock;

    [Header("Control Condition Settings")]
    [SerializeField] private float minTime = 6f;
    [SerializeField] private float maxTime = 8f;

    [Header("Perfect Condition Settings")]
    [SerializeField] private Transform holeTransform;

    [Header("Random Condition Settings")]
    [SerializeField] private Transform golfBall;
    [SerializeField, DataTable] private List<ErrorRadiusWithCount> errors;

    private int totalErrorCount;
    private List<Block> allBlocks = new List<Block>();
    private const float HOLE_RADIUS = 0.055f;
    private Random random;
    private bool isRandomConditionInvalid;
    private bool customParticipantOverride;

    public List<Block> GenerateBlocks(char condition, string pid, int session, bool randomizeSeed = false)
    {
        if (isRandomConditionInvalid) return null;
        if (!int.TryParse(pid, out int pidVal))
        {
            pidVal = 0;
        }

        int seed = condition + session + pidVal;
        random = randomizeSeed ? new Random(): new Random(seed);

        switch (condition)
        {
            case 'c':
                GenerateControlCondition();
                return allBlocks;
            case 'p':
                GeneratePerfectCondition();
                return allBlocks;
            case 'r':
                GenerateRandomCondition();
                return allBlocks;
            default:
                throw new Exception("Not a valid condition type");
        }
    }

    private void OnValidate()
    {
        int totalErrorsAcrossBlocks = 0;
        foreach (ErrorRadiusWithCount error in errors)
        {
            totalErrorsAcrossBlocks += error.Count;
        }

        if (minErroneousTrialsPerBlock * blockCount > totalErrorsAcrossBlocks ||
            maxErroneousTrialsPerBlock * blockCount < totalErrorsAcrossBlocks)
        {
            isRandomConditionInvalid = true;
        }
        else isRandomConditionInvalid = false;
    }

    private void GenerateControlCondition()
    {
        for (int i = 0; i < blockCount; i++)
        {
            Block block = new Block();
            block.Trials = new List<Trial>();
            for (int j = 0; j < trialCountPerBlock; j++)
            {
                Trial trial = new Trial();
                trial.Type = ConditionType.Control;
                trial.TargetPosition = new Vector3(minTime, 0f, maxTime);
                block.Trials.Add(trial);
            }
            allBlocks.Add(block);
        }
        PrintAllTrials();
    }

    private void GeneratePerfectCondition()
    {
        for (int i = 0; i < blockCount; i++)
        {
            Block block = new Block();
            block.Trials = new List<Trial>();
            for (int j = 0; j < trialCountPerBlock; j++)
            {
                Trial trial = new Trial();
                trial.Type = ConditionType.Perfect;
                trial.TargetPosition = holeTransform.position;
                block.Trials.Add(trial);
            }
            allBlocks.Add(block);
        }
        PrintAllTrials();
    }

    private void GenerateRandomCondition()
    {
        List<Vector3> errorPositions = GenerateErrorPositions();

        List<int> errorDistribution = DistributeErrorsAcrossBlocks(errorPositions.Count, minErroneousTrialsPerBlock, maxErroneousTrialsPerBlock);

        for (int i = 0; i < blockCount; i++)
        {
            Block block = new Block();
            block.Trials = new List<Trial>();
            List<Trial> trials = new List<Trial>();
            for (int j = 0; j < trialCountPerBlock; j++)
            {
                Trial trial = new Trial();

                if (errorDistribution[i] > 0)
                {
                    trial.Type = ConditionType.Error;
                    trial.TargetPosition = errorPositions[0];
                    errorPositions.RemoveAt(0);
                    errorDistribution[i]--;
                }
                else
                {
                    trial.Type = ConditionType.Perfect;
                    trial.TargetPosition = holeTransform.position;
                }
                trials.Add(trial);
            }
            trials.Shuffle(random);
            block.Trials = trials;
            allBlocks.Add(block);
        }
        PrintAllTrials();
    }

    private void PrintAllTrials()
    {
        string allTrialStr = "";
        int i = 1;
        foreach (Block block in allBlocks)
        {
            string blockStr = $"Block{i}\n||";
            foreach (Trial trial in block.Trials)
            {
                string color = "";
                switch (trial.Type)
                {
                    case ConditionType.Control:
                        color = "#d1d1d1";
                        break;
                    case ConditionType.Perfect:
                        color = "#ffffff";
                        break;
                    case ConditionType.Error:
                        // color = "#FF8D75";
                        float trialDistance = Vector3.Distance(trial.TargetPosition, holeTransform.position) - HOLE_RADIUS;
                        foreach (ErrorRadiusWithCount error in errors.Where(error => trialDistance >= error.StartRadius && trialDistance <= error.EndRadius))
                        {
                            color = "#" + ColorUtility.ToHtmlStringRGB(error.DebugConsoleColor);
                            break;
                        }
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                string type = trial.Type switch
                {
                    ConditionType.Control => "C", ConditionType.Perfect => "P", ConditionType.Error => "E"
                    , _ => throw new ArgumentOutOfRangeException()
                };
                blockStr += $"<color={color}>Type:{type};Dist:{Vector3.Distance(trial.TargetPosition, holeTransform.position):0.00}</color>||";
            }
            allTrialStr += blockStr + "\n";
            i++;
        }

        Debug.Log(allTrialStr);
    }

    private List<int> DistributeErrorsAcrossBlocks(int totalErrors, int minPerBlock, int maxPerBlock)
    {
        List<int> distribution = new List<int>(new int[blockCount]);

        for (int i = 0; i < blockCount; i++)
        {
            distribution[i] = minPerBlock;
            totalErrors -= minPerBlock;
        }

        while (totalErrors > 0)
        {
            for (int i = 0; i < blockCount && totalErrors > 0; i++)
            {
                if (distribution[i] >= maxPerBlock) continue;
                if (random.Next(1, 7) == 1)
                {
                    distribution[i]++;
                    totalErrors--;
                }
            }
        }

        return distribution;
    }

    private List<Vector3> GenerateErrorPositions()
    {
        bool validAngle = false;
        Vector3 errorPosition = Vector3.zero;

        Vector3 ballToHole = holeTransform.position - golfBall.position;
        float ballToHoleMag = ballToHole.magnitude;

        Vector3 holeLeft = holeTransform.position + Vector3.Cross(ballToHole, Vector3.up).normalized * 0.055f;
        Vector3 holeRight = holeTransform.position + -Vector3.Cross(ballToHole, Vector3.up).normalized * 0.055f;
        Vector3 ballLeft = holeLeft - holeTransform.position + golfBall.position;

        float leftLimit = Vector3.Angle(ballLeft-golfBall.position, holeLeft-golfBall.position);
        float rightLimit = Vector3.Angle(ballLeft-golfBall.position, holeRight-golfBall.position);

        List<Vector3> errorPositions = new List<Vector3>();
        foreach (ErrorRadiusWithCount error in errors)
        {
            for (int i = 0; i < error.Count; i++)
            {
                float errorRadius = random.NextFloat(error.StartRadius + HOLE_RADIUS, error.EndRadius + HOLE_RADIUS);
                while(!validAngle)
                {
                    float errorAngle = random.NextFloat(-180f, 180f);
                    errorPosition = holeTransform.position + Quaternion.AngleAxis(errorAngle, transform.up) * transform.forward * errorRadius;
                    Vector3 ballToPosition = errorPosition - golfBall.position;
                    float ballToPositionAngle = Vector3.Angle(ballToPosition, ballLeft-golfBall.position);
                    float ballToPositionMag = ballToPosition.magnitude;
                    if (ballToPositionAngle >= leftLimit && ballToPositionAngle <= rightLimit && ballToPositionMag >= ballToHoleMag) validAngle = false;
                    else validAngle = true;
                }
                errorPositions.Add(errorPosition);
                validAngle = false;
            }
        }
        errorPositions.Shuffle(random);
        return errorPositions;
    }
#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        if (!enableGizmos) return;
        Handles.color = Color.red; // Set line color
        Vector3 center = holeTransform.position;
        int segments = 250;
        List<ErrorRadiusWithCount> errorsWithBase = new List<ErrorRadiusWithCount>(errors);
        errorsWithBase.Insert(0, new ErrorRadiusWithCount(0, 0, 0, Color.black));
        int colIdx = 0;
        foreach (ErrorRadiusWithCount error in errorsWithBase)
        {
            Handles.color = error.DebugConsoleColor;
            Vector3[] ringPoints = new Vector3[segments + 1];
            for (int i = 0; i <= segments; i++)
            {
                float angle = (i / (float)segments) * Mathf.PI * 2;
                ringPoints[i] = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * (error.StartRadius + HOLE_RADIUS) + center;
            }
            Handles.DrawAAPolyLine(7, ringPoints);
            for (int i = 0; i <= segments; i++) // Close the loop by repeating the first point
            {
                float angle = (i / (float)segments) * Mathf.PI * 2;
                ringPoints[i] = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * (error.EndRadius + HOLE_RADIUS) + center;
            }
            Handles.DrawAAPolyLine(7, ringPoints);
            colIdx++;
        }

        Vector3 ballToHole = holeTransform.position - golfBall.position;
        Vector3 holeLeft = holeTransform.position + (Vector3.Cross(ballToHole, Vector3.up).normalized) * 0.055f;
        Vector3 holeRight = holeTransform.position + (-Vector3.Cross(ballToHole, Vector3.up).normalized) * 0.055f;
        Vector3 ballLeft = holeLeft - holeTransform.position + golfBall.position;
        Vector3 ballRight = holeRight - holeTransform.position + golfBall.position;

        Handles.color = Color.green;
        Handles.DrawLine(golfBall.position, holeTransform.position, 3f);
        Handles.color = Color.blue;
        Handles.DrawLine(holeTransform.position, holeLeft, 3f);
        Handles.color = Color.cyan;
        Handles.DrawLine(holeTransform.position, holeRight, 3f);
        Handles.color = Color.red;
        Handles.DrawLine(golfBall.position, holeLeft, 3f);
        Handles.color = Color.magenta;
        Handles.DrawLine(golfBall.position, holeRight, 3f);
        Handles.color = Color.yellow;
        Handles.DrawLine(golfBall.position, ballLeft, 3f);
        Handles.color = Color.gray;
        Handles.DrawLine(golfBall.position, ballRight, 3f);
    }
#endif
}

[Serializable]
public class ErrorRadiusWithCount
{
    public float StartRadius;
    public float EndRadius;
    public int Count;
    public Color DebugConsoleColor;

    public ErrorRadiusWithCount(float startRadius, float endRadius, int count, Color debugConsoleColor)
    {
        StartRadius = startRadius;
        EndRadius = endRadius;
        Count = count;
        DebugConsoleColor = debugConsoleColor;
    }
}

public class Block
{
    public List<Trial> Trials;
}
public class Trial
{
    public ConditionType Type;
    public Vector3 TargetPosition;
}

public enum ConditionType
{
    Control = 0,
    Perfect = 1,
    Error = 2
}
