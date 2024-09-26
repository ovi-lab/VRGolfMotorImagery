using System;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(ConditionManager))]
public class ExperimentHandler : SingletonMonoBehavior<ExperimentHandler>
{
    [SerializeField] private GolfBallController controller;
    [SerializeField] private TextMeshProUGUI tv;
    [TextArea, SerializeField] private string welcomeMessage;
    [TextArea, SerializeField] private string welcomeBackMessage;
    [TextArea, SerializeField] private string motorImageryMessage;
    [TextArea, SerializeField] private string golfMessage;
    [TextArea, SerializeField] private string restMessage;
    [TextArea, SerializeField] private string thanksMessage;

    private string pid;
    private int session;
    private string participantName;
    private int condition;
    private int block = 1;
    private int trial = 1;
    private bool isBlockActive;
    private bool isTrialActive;
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
            string configFilePath = Path.Combine(configDirectory, "config.txt");

            if (!File.Exists(configFilePath)) throw new FileNotFoundException(configFilePath);
            string fileContents = File.ReadAllText(configFilePath);

            string[] lines = fileContents.Split('\n');
            pid = lines[0].Split(':')[1].Trim();
            session = int.Parse(lines[1].Split(':')[1].Trim());
            participantName = lines[2].Split(':')[1].Trim();
            condition = int.Parse(lines[3].Split(':')[1].Trim());

            string dateTime = DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss");
            string dataFileName = $"{pid}_{session}_{dateTime}.csv";
            string dataDirectory = Path.Combine(Application.persistentDataPath, "Data");

            if (condition is not 0 and not 1 and not 2)
            {
                tv.text
                    = "Something has gone wrong. Please ask the on-site researcher to check the experiment configuration setup";
                throw new Exception("Wrong Condition Value");
            }

            if (session == 1)
            {
                string introText = !string.IsNullOrWhiteSpace(participantName)
                    ? $"{welcomeMessage}\n{participantName}!"
                    : $"{welcomeMessage}!";
                string validationText = $"\nPlease read out the following: \nPID: {pid}, Session: {session}";
                tv.text = introText + validationText;
            }
            else if (session > 1)
            {
                string introText = !string.IsNullOrWhiteSpace(participantName)
                    ? $"{welcomeBackMessage}\n{participantName}!"
                    : $"{welcomeBackMessage}!";
                string validationText = $"\nPlease read this code:\n {condition}-{pid}-{session}";
                tv.text = introText + validationText;
            }
            else
            {
                tv.text
                    = "Something has gone wrong. Please ask the on-site researcher to check the experiment configuration setup";
                throw new Exception("Wrong Session Value");
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
                "pid,session,block,trial,start_time,ball_fire_time,ball_stop_time,end_time,radial_error\n");
            allBlocks = GetComponent<ConditionManager>().GenerateBlocks(condition);
            receivedAllBlocks = true;
        }
        catch(Exception e)
        {
            tv.text = e.ToString();
        }
    }

    private void OnEnable()
    {
        InputHandler.Instance.OnButtonPress += HandleInput;
    }

    private void OnDisable()
    {
        InputHandler.Instance.OnButtonPress -= HandleInput;
    }

    private void HandleInput()
    {
        if(!receivedAllBlocks) Debug.LogError("HOW DID WE GET HERE?");
        if(!isBlockActive) StartBlock();
        else if (isBlockActive && isTrialActive)
        {
            FireBall();
        }
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
        Trial targetTrial =  allBlocks[block - 1].Trials[trial - 1];
        controller.FireBall(targetTrial.TargetPosition);
        currentType = targetTrial.Type;
        controller.OnBallStop.AddListener(StopBall);
        tv.text = golfMessage;
        ballFireTime = DateTime.Now.ToString("HH:mm:ss.fff");
        Debug.Log("Ball Fired");
    }

    private void StopBall()
    {
        if (!isTrialActive)
        {
            Debug.LogWarning("Cannot log ball stop time. No active trial.");
            return;
        }
        controller.OnBallStop.RemoveListener(StopBall);
        ballStopTime = DateTime.Now.ToString("HH:mm:ss.fff");
        Debug.Log("Ball Stopped");
        EndTrial(Vector3.Distance(controller.transform.position, controller.HoleTransform.position));
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
            StartTrial();
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
        }
        else
        {
            tv.text = restMessage;
        }
    }

    private void LogData(float radialError)
    {
        string logEntry = $"{pid},{session},{block},{trial},{startTime},{ballFireTime},{ballStopTime},{endTime},{radialError}\n";
        File.AppendAllText(dataFilePath, logEntry);
        Debug.Log("Logged Data");
    }
}
