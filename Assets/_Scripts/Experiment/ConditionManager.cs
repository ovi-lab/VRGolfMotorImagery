using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class ConditionManager : MonoBehaviour
{
    [Header("Experiment Settings")]
    [SerializeField] private int blockCount;
    [SerializeField] private int trialCountPerBlock;
    [Range(0, 100), SerializeField] private float percentageError;
    [SerializeField] private int minErroneousTrialsPerBlock;
    [SerializeField] private int maxErroneousTrialsPerBlock;

    [Header("Control Condition Settings")]
    [SerializeField] private float minTime = 6f;
    [SerializeField] private float maxTime = 8f;

    [Header("Perfect Condition Settings")]
    [SerializeField] private Transform holeTransform;

    [Header("Random Condition Settings")]
    [Range(0, 180),SerializeField] private float maxErrorAngle;
    [SerializeField] private float minErrorDistance;
    [SerializeField] private float maxErrorDistance;
    [SerializeField] private float meanRadialError;

    private List<Vector3> errorPositions = new List<Vector3>();
    private List<Block> allBlocks = new List<Block>();

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
        int totalTrialCount = blockCount * trialCountPerBlock;
        int errorTrialCount = (int)(totalTrialCount * percentageError * 0.01f);
        GenerateErrorPositions(errorTrialCount);

        List<int> errorDistribution = DistributeErrorsAcrossBlocks(errorTrialCount, blockCount, minErroneousTrialsPerBlock, maxErroneousTrialsPerBlock);

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

        //ValidateDistribution();
    }

    private List<int> DistributeErrorsAcrossBlocks(int totalErrors, int blockCount, int minPerBlock, int maxPerBlock)
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

    private void ValidateDistribution()
    {

        int totalErrors = 0;
        float totalRadialError = 0;
        bool failed = false;

        foreach (Block block in allBlocks)
        {
            int blockErrorCount = 0;
            foreach (Trial trial in block.Trials)
            {
                if (trial.Type == ConditionType.Error)
                {
                    blockErrorCount++;
                    totalErrors++;
                    totalRadialError += Vector3.Distance(trial.TargetPosition, holeTransform.position);
                }
            }

            if (blockErrorCount < minErroneousTrialsPerBlock || blockErrorCount > maxErroneousTrialsPerBlock)
            {
                Debug.LogError($"Min Errors: {minErroneousTrialsPerBlock}; Max Errors: {maxErroneousTrialsPerBlock}; Block Error: {blockErrorCount}");
                failed = true;
                break;
            }
        }

        Debug.Log($"Average radial error {totalRadialError/totalErrors}");
        Debug.Log($"Expected average radial error {meanRadialError}");
        Debug.Log($"Total Error Trials: {totalErrors}");
        Debug.Log($"Expected Error Trials: {(int)(blockCount * trialCountPerBlock * percentageError * 0.01f)}");
        if (failed)
        {
            Debug.LogError("Block error count constraints failed");
        }
        else
        {
            Debug.Log("Minimum and maximum errors per block were satisfied!");
        }
    }

    private void GenerateErrorPositions(int errorTrialCount)
    {
        List<float> radialErrors = RandomValueGenerator.GenerateValues(errorTrialCount, meanRadialError, minErrorDistance, meanRadialError);
        foreach (float radialError in radialErrors)
        {
            float randomAngle = Random.Range(-maxErrorAngle, maxErrorAngle);
            Vector3 errorPosition = holeTransform.position + Quaternion.AngleAxis(randomAngle, transform.up) * transform.forward * radialError;
            errorPositions.Add(errorPosition);
        }
    }

    void OnDrawGizmos()
    {
        Vector3 forward = holeTransform.forward * maxErrorDistance;
        Vector3 leftBoundary = Quaternion.Euler(0, -maxErrorAngle, 0) * forward;
        Vector3 rightBoundary = Quaternion.Euler(0, maxErrorAngle, 0) * forward;

        Gizmos.color = Color.red;

        Gizmos.DrawWireSphere(holeTransform.position, maxErrorDistance);
        Gizmos.DrawLine(holeTransform.position, holeTransform.position + leftBoundary);
        Gizmos.DrawLine(holeTransform.position, holeTransform.position + rightBoundary);
    }

    public bool ValidateConstraints()
    {
        int blockMinError = blockCount * minErroneousTrialsPerBlock;
        int blockMaxError = blockCount * maxErroneousTrialsPerBlock;
        int expectedErrorCount = (int)(blockCount * trialCountPerBlock * percentageError * 0.01f);
        return expectedErrorCount >= blockMinError && expectedErrorCount <= blockMaxError;
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
