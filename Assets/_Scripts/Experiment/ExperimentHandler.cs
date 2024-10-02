using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

[RequireComponent(typeof(ConditionManager))]
public class ExperimentHandler : SingletonMonoBehavior<ExperimentHandler>
{
    [SerializeField] private GolfBallController controller;
    [SerializeField] private TextMeshProUGUI tv;
    [Range(2, 10), SerializeField] private int maximumSessionCount;
    [TextArea, SerializeField] private string welcomeMessage;
    [TextArea, SerializeField] private string welcomeBackMessage;
    [TextArea, SerializeField] private string motorImageryMessage;
    [TextArea, SerializeField] private string golfMessage;
    [TextArea, SerializeField] private string trialEndMessage;
    [TextArea, SerializeField] private string blockEndMessage;
    [TextArea, SerializeField] private string canceledPreviousTrial;
    [SerializeField] private bool showPrevTrialNumInMessage;
    [TextArea, SerializeField] private string canceledCurrentTrial;
    [SerializeField] private bool showCurrTrialNumInMessage;
    [TextArea, SerializeField] private string thanksMessage;

    private string pid;
    private int session;
    private string participantName;
    private char condition;
    private int block = 1;
    private int trial = 1;
    private bool isBlockActive;
    private bool isTrialActive;
    private bool isBallMoving;
    private bool invalidConfig;
    private bool experimentEnd;
    private string startTime;
    private string endTime;
    private string ballFireTime;
    private string ballStopTime;
    private string dataFilePath;
    private bool receivedAllBlocks;
    private List<Block> allBlocks = new List<Block>();
    private ConditionType currentType;

    protected override void Awake()
    {
        base.Awake();
        try
        {
            string configDirectory = Path.Combine(Application.persistentDataPath, "Config");
            if (!Directory.Exists(configDirectory))
            {
                Directory.CreateDirectory(configDirectory);
            }

            string configFilePath = Path.Combine(configDirectory, "config.txt");
            if (!File.Exists(configFilePath))
            {
                tv.text
                    = "Something has gone wrong\nPlease ask the on-site researcher to check the experiment configuration setup\nConfig File is missing";
                invalidConfig = true;
                return;
            }

            string fileContents = File.ReadAllText(configFilePath);

            string[] lines = fileContents.Split('\n');
            pid = lines[0].Split(':')[1].Trim();
            session = int.Parse(lines[1].Split(':')[1].Trim());
            try
            {
                condition = char.ToLower(char.Parse(lines[2].Split(':')[1].Trim()));
            }
            catch
            {
                tv.text
                    = "Something has gone wrong\nPlease ask the on-site researcher to check the experiment configuration setup\nInvalid Condition";
                invalidConfig = true;
                return;
            }

            try
            {
                participantName = lines[3].Split(':')[1].Trim();
            }
            catch
            {
                Debug.LogWarning("No name provided");
            }

            string dateTime = DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss");
            string dataFileName = $"{pid}_{condition}_{session}_{dateTime}.csv";
            string dataDirectory = Path.Combine(Application.persistentDataPath, "Data");

            if (condition is not 'c' and not 'p' and not 'r')
            {
                tv.text
                    = "Something has gone wrong\nPlease ask the on-site researcher to check the experiment configuration setup\nInvalid Condition";
                invalidConfig = true;
                return;
            }

            int conditionCode = condition switch
            {
                'c' => 0,
                'p' => 1,
                'r' => 2,
                _ => -1
            };

            switch (session)
            {
                case 1:
                {
                    string introText = !string.IsNullOrWhiteSpace(participantName)
                        ? $"{welcomeMessage}\n{participantName}!"
                        : $"{welcomeMessage}!";
                    string validationText = $"\nPlease read this code:\n{pid}-{conditionCode}-{session}";
                    tv.text = introText + validationText;
                    break;
                }
                case > 1 when session <= maximumSessionCount:
                {
                    string introText = !string.IsNullOrWhiteSpace(participantName)
                        ? $"{welcomeBackMessage}\n{participantName}!"
                        : $"{welcomeBackMessage}!";
                    string validationText = $"\nPlease read this code:\n{pid}-{conditionCode}-{session}";
                    tv.text = introText + validationText;
                    break;
                }
                default:
                    tv.text
                        = "Something has gone wrong\nPlease ask the on-site researcher to check the experiment configuration setup\nInvalid Session";
                    invalidConfig = true;
                    return;
            }


            if (!Directory.Exists(dataDirectory))
            {
                Directory.CreateDirectory(dataDirectory);
            }

            string participantPath = Path.Combine(dataDirectory, pid);
            if (!Directory.Exists(participantPath))
            {
                Directory.CreateDirectory(participantPath);
            }

            dataFilePath = Path.Combine(participantPath, dataFileName);
            File.WriteAllText(dataFilePath,
                "pid,condition,session,block,trial,success,start_time,ball_fire_time,ball_stop_time,end_time,radial_error\n");
            allBlocks = GetComponent<ConditionManager>().GenerateBlocks(condition);
            receivedAllBlocks = true;
        }
        catch
        {
            tv.text =
                "Something has gone wrong\nPlease ask the on-site researcher to check the experiment configuration setup";
            invalidConfig = true;
        }
    }

    private void OnEnable()
    {
        InputHandler.Instance.OnTriggerPull += HandleInput;
        InputHandler.Instance.OnCancelTrial += HandleUndo;
    }

    private void OnDisable()
    {
        InputHandler.Instance.OnTriggerPull -= HandleInput;
        InputHandler.Instance.OnCancelTrial -= HandleUndo;
    }

    private void HandleInput()
    {
        if (invalidConfig) return;
        if (experimentEnd) return;
        if (!receivedAllBlocks) Debug.LogError("HOW DID WE GET HERE?");
        if (!isBlockActive) StartBlock();
        else if (isBlockActive && !isTrialActive) StartTrial();
        else if (isBlockActive && isTrialActive)
        {
            if (isBallMoving) return;
            FireBall();
        }
    }

    private void HandleUndo()
    {
        if (invalidConfig) return;
        if (experimentEnd) return;
        if (!receivedAllBlocks) Debug.LogError("HOW DID WE GET HERE?");
        UndoTrial();
    }

    private void StartBlock()
    {
        if (isBlockActive)
        {
            Debug.LogWarning("Block is already active.");
            return;
        }

        isBlockActive = true;
        trial = 1;
        Debug.Log($"Block Started: {block}");
        StartTrial();
    }

    private void StartTrial()
    {
        if (!isBlockActive)
        {
            Debug.LogWarning("No active block. Start a block first.");
            return;
        }

        if (isTrialActive)
        {
            Debug.LogWarning("Trial is already active.");
            return;
        }

        startTime = DateTime.Now.ToString("HH:mm:ss.fff");
        isTrialActive = true;
        tv.text = motorImageryMessage;
        Debug.Log($"Trial Started: {trial}");
    }

    private void FireBall()
    {
        if (!isTrialActive)
        {
            Debug.LogWarning("Cannot log ball fire time. No active trial.");
            return;
        }

        Trial targetTrial = allBlocks[block - 1].Trials[trial - 1];
        if (targetTrial.Type == ConditionType.Control)
        {
            StartCoroutine(ControlTrialTimer(Random.Range(targetTrial.TargetPosition.x, targetTrial.TargetPosition.z)));
        }
        else
        {
            controller.FireBall(targetTrial.TargetPosition);
            controller.OnBallStop.AddListener(StopBall);
        }

        currentType = targetTrial.Type;
        tv.text = golfMessage;
        Debug.Log("Ball Fired");
        ballFireTime = DateTime.Now.AddSeconds(controller.Phaser.AnimTime).ToString("HH:mm:ss.fff");
        isBallMoving = true;
    }

    private void StopBall()
    {
        if (!isTrialActive)
        {
            Debug.LogWarning("Cannot log ball stop time. No active trial.");
            return;
        }

        controller.OnBallStop.RemoveListener(StopBall);
        ballStopTime = DateTime.Now.AddSeconds(-controller.Phaser.AnimTime).ToString("HH:mm:ss.fff");
        Debug.Log("Ball Stopped");
        isBallMoving = false;
        EndTrial(Vector3.Distance(controller.transform.position, controller.HoleTransform.position));
    }

    private IEnumerator ControlTrialTimer(float time)
    {
        yield return new WaitForSeconds(time);
        ballStopTime = DateTime.Now.ToString("HH:mm:ss.fff");
        Debug.Log("Ball Stopped");
        isBallMoving = false;
        EndTrial(-1f);
    }

    private void EndTrial(float radialError)
    {
        if (!isTrialActive)
        {
            Debug.LogWarning("No active trial to end.");
            return;
        }

        endTime = DateTime.Now.ToString("HH:mm:ss.fff");
        Debug.Log($"Trial Ended: {trial}");
        if (currentType == ConditionType.Perfect) radialError = 0f;
        else if (currentType == ConditionType.Control) radialError = -1f;
        LogData(radialError);

        trial++;
        isTrialActive = false;
        if (allBlocks[block - 1].Trials.Count < trial)
        {
            EndBlock();
            trial = 1;
        }
        else
        {
            tv.text = trialEndMessage;
        }
    }

    private void EndBlock()
    {
        if (!isBlockActive)
        {
            Debug.LogWarning("No active block to end.");
            return;
        }

        isBlockActive = false;
        Debug.Log($"Block Ended: {block}");
        block++;
        if (allBlocks.Count < block)
        {
            tv.text = thanksMessage;
            Debug.Log($"Experiment Ended");
            experimentEnd = true;
        }
        else
        {
            tv.text = blockEndMessage;
        }
    }

    private void LogData(float radialError)
    {
        string radialErrorString = radialError >= 0 ? radialError.ToString(CultureInfo.InvariantCulture) : "N/A";
        string logEntry =
            $"{pid},{condition},{session},{block},{trial},True,{startTime},{ballFireTime},{ballStopTime},{endTime},{radialErrorString}\n";
        File.AppendAllText(dataFilePath, logEntry);
        Debug.Log("Logged Data");
    }

    private void UndoTrial()
    {
        if (trial == 1 && block == 1)
        {
            Debug.LogWarning("Cannot undo trial: Already at first trial of first block.");
            return;
        }

        string failedFilePath = dataFilePath[..^4] + "_failed.csv";

        string[] allLines = File.ReadAllLines(dataFilePath);

        string lastRecordedTrial = allLines[^1];
        string headerLine = allLines[0] + "\n";

        if (!File.Exists(failedFilePath))
        {
            File.WriteAllText(failedFilePath, headerLine);
        }

        // one of two things can happen here
        // either the trial is ongoing
        // or we're in the middle of two trials
        // if the trial is ongoing, do a check,
        // log as much data as possible (??)
        // set the thing as false, and repeat
        // else do the logic below

        if (isTrialActive)
        {
            string logFailedEntry = $"{pid},{condition},{session},{block},{trial},False,{startTime}";
            if (isBallMoving)
            {
                logFailedEntry += $",{ballFireTime},N/A,N/A,N/A";
            }
            else
            {
                logFailedEntry += ",N/A,N/A,N/A,N/A";
            }
            controller.Phaser.Disappear();
            controller.ResetBall();
            isBallMoving = false;
            File.AppendAllText(failedFilePath, string.Join(",", logFailedEntry) + "\n");
            tv.text = showCurrTrialNumInMessage ? $"{block}:{trial}\n" + canceledCurrentTrial : canceledCurrentTrial;
        }
        else
        {
            if (allLines.Length <= 1)
            {
                Debug.LogWarning("Cannot undo trial: No recorded trial data.");
                return;
            }

            string[] failedTrialData = lastRecordedTrial.Split(',');
            failedTrialData[5] = "False";
            File.AppendAllText(failedFilePath, string.Join(",", failedTrialData) + "\n");
            File.WriteAllLines(dataFilePath, allLines.Take(allLines.Length - 1).ToArray());
            controller.Phaser.Disappear();
            controller.ResetBall();
            trial--;
            if (trial < 1)
            {
                block--;
                trial = allBlocks[block - 1].Trials.Count;
                isBlockActive = true;
            }
            tv.text = showPrevTrialNumInMessage ? $"{block}:{trial}\n" + canceledPreviousTrial : canceledPreviousTrial;
        }
        isTrialActive = false;
    }
}
