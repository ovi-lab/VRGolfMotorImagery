using System;
using System.IO;
using TMPro;
using UnityEngine;

public class ExperimentHandler : SingletonMonoBehavior<ExperimentHandler>
{
    [SerializeField] private TextMeshProUGUI tv;
    [TextArea, SerializeField] private string welcomeMessage;
    [TextArea, SerializeField] private string welcomeBackMessage;
    [TextArea, SerializeField] private string motorImageryMessage;
    [TextArea, SerializeField] private string golfMessage;

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

    protected override void Awake()
    {
        base.Awake();
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

        if (session == 1)
        {
            string introText = !string.IsNullOrWhiteSpace(participantName) ? $"{welcomeMessage}\n{participantName}!" : $"{welcomeMessage}!";
            string validationText = $"\nPlease read out the following: \nPID: {pid}, Session: {session}";
            tv.text = introText + validationText;
        }
        else if(session > 1)
        {
            string introText = !string.IsNullOrWhiteSpace(participantName) ? $"{welcomeBackMessage}\n{participantName}!" : $"{welcomeBackMessage}!";
            string validationText = $"\nPlease read out the following: \nPID: {pid}, Session: {session}";
            tv.text = introText + validationText;        }
        else
        {
            tv.text
                = "Something has gone wrong. Please ask the on-site researcher to check the experiment configuration setup";
            throw new Exception("Wrong Session value");
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
        File.WriteAllText(dataFilePath, "pid,session,block,trial,start_time,ball_fire_time,ball_stop_time,end_time,radial_error\n");
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
        if(!isBlockActive) StartBlock();
        if(isBlockActive && !isTrialActive)
        {
            StartTrial();
        }
    }

    public void StartBlock()
    {
        if (isBlockActive)
        {
            Debug.LogWarning("Block is already active.");
            return;
        }

        isBlockActive = true;
        trial = 1;
        Debug.Log($"Block {block} started.");
    }

    public void StartTrial()
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
        Debug.Log($"Trial {trial} has started");
    }

    public void BallFireTime()
    {
        if (!isTrialActive)
        {
            Debug.LogWarning("Cannot log ball fire time. No active trial.");
            return;
        }

        tv.text = golfMessage;
        ballFireTime = DateTime.Now.ToString("HH:mm:ss.fff");
    }

    public void BallStopTime()
    {
        if (!isTrialActive)
        {
            Debug.LogWarning("Cannot log ball stop time. No active trial.");
            return;
        }

        ballStopTime = DateTime.Now.ToString("HH:mm:ss.fff");
    }

    public void EndTrial(float radialError)
    {
        if (!isTrialActive)
        {
            Debug.LogWarning("No active trial to end.");
            return;
        }

        endTime = DateTime.Now.ToString("HH:mm:ss.fff");

        LogData(radialError);

        trial++;
        isTrialActive = false;
        Debug.Log($"Trial {trial - 1} has ended");
    }

    public void EndBlock()
    {
        if (!isBlockActive)
        {
            Debug.LogWarning("No active block to end.");
            return;
        }

        isBlockActive = false;
        block++;
        Debug.Log($"Block {block - 1} ended.");
    }

    private void LogData(float radialError)
    {
        string logEntry = $"{pid},{session},{block},{trial},{startTime},{ballFireTime},{ballStopTime},{endTime},{radialError}\n";
        File.AppendAllText(dataFilePath, logEntry);
    }
}
