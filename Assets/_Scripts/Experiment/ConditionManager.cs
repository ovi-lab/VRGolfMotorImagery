using System;
using System.Collections.Generic;
using EditorAttributes;
using UnityEngine;
using Random = UnityEngine.Random;

public class ConditionManager : MonoBehaviour
{
    [Header("Experiment Settings")]
    [SerializeField] private int blockCount;
    [SerializeField] private int trialCountPerBlock;
    [SerializeField] private int minErroneousTrialsPerBlock;
    [SerializeField] private int maxErroneousTrialsPerBlock;

    [Header("Control Condition Settings")]
    [SerializeField] private float minTime = 6f;
    [SerializeField] private float maxTime = 8f;

    [Header("Perfect Condition Settings")]
    [SerializeField] private Transform holeTransform;

    [Header("Random Condition Settings")]
    [SerializeField, DataTable] private List<ErrorRadiusWithCount> errors;

    private int totalErrorCount;
    private List<Block> allBlocks = new List<Block>();
    private const float HOLE_RADIUS = 0.055f;

    public List<Block> GenerateBlocks(char condition)
    {
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
            trials.Shuffle();
            block.Trials = trials;
            allBlocks.Add(block);
        }
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
                if (distribution[i] < maxPerBlock)
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
        List<Vector3> errorPositions = new List<Vector3>();
        foreach (ErrorRadiusWithCount error in errors)
        {
            for (int i = 0; i < error.Count; i++)
            {
                float errorRadius = Random.Range(error.StartRadius, error.EndRadius);
                float errorAngle = Random.Range(-175f, 175f);
                Vector3 errorPosition = holeTransform.position +
                                        Quaternion.AngleAxis(errorAngle, transform.up) * transform.forward *
                                        errorRadius;
                errorPositions.Add(errorPosition);
            }
        }
        return errorPositions;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Vector3 center = holeTransform.position;
        int segments = 250;
        List<ErrorRadiusWithCount> errorsWithBase = new List<ErrorRadiusWithCount>(errors);
        errorsWithBase.Insert(0, new ErrorRadiusWithCount(0, 0, 0));
        foreach (ErrorRadiusWithCount error in errorsWithBase)
        {
            for (int i = 0; i < segments; i++)
            {
                float angle = (i / (float)segments) * Mathf.PI * 2;
                float nextAngle = ((i + 1) / (float)segments) * Mathf.PI * 2;

                Vector3 point = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * (error.EndRadius + HOLE_RADIUS) + new Vector3(center.x, center.y, center.z);
                Vector3 nextPoint = new Vector3(Mathf.Cos(nextAngle), 0, Mathf.Sin(nextAngle)) * (error.EndRadius + HOLE_RADIUS) + new Vector3(center.x, center.y, center.z);

                Gizmos.DrawLine(point, nextPoint);
            }
        }
    }
}

[Serializable]
public class ErrorRadiusWithCount
{
    public float StartRadius;
    public float EndRadius;
    public int Count;

    public ErrorRadiusWithCount(float startRadius, float endRadius, int count)
    {
        StartRadius = startRadius;
        EndRadius = endRadius;
        Count = count;
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
