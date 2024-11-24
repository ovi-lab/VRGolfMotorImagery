using System;
using System.Collections.Generic;
using EditorAttributes;
using UnityEngine;
using Random = System.Random;

public class ConditionManager : MonoBehaviour
{

    [SerializeField] private bool debugBlocks;

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
    [SerializeField, Validate("Invalid Parameters for Random Condition", nameof(isRandomConditionInvalid), buildKiller:true)] private bool isRandomConditionInvalid;
    [SerializeField, DataTable] private List<ErrorRadiusWithCount> errors;

    private int totalErrorCount;
    private List<Block> allBlocks = new List<Block>();
    private const float HOLE_RADIUS = 0.055f;
    private Random random;

    public List<Block> GenerateBlocks(char condition, string pid, int session)
    {
        if (isRandomConditionInvalid) return null;

        if (!int.TryParse(pid, out int pidVal))
        {
            pidVal = 0;
        }

        int seed = condition + session + pidVal;
        random = new Random(seed);

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
        if(debugBlocks) PrintAllTrials();
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
        if(debugBlocks) PrintAllTrials();
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
        if(debugBlocks) PrintAllTrials();
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
                string color = trial.Type switch
                {
                    ConditionType.Perfect => "#75FF8C", ConditionType.Error => "#FF8D75", ConditionType.Control => "#9485FF"
                    , _ => throw new ArgumentOutOfRangeException()
                };
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
        List<Vector3> errorPositions = new List<Vector3>();
        foreach (ErrorRadiusWithCount error in errors)
        {
            for (int i = 0; i < error.Count; i++)
            {
                float errorRadius = random.NextFloat(error.StartRadius, error.EndRadius);
                float errorAngle = random.NextFloat(-175f, 175f);
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
