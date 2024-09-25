using System;
using System.IO;
using TMPro;
using UnityEngine;

public class DataWriter : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI tmp;

    private void Start()
    {
        string folderPath = Path.Combine(Application.persistentDataPath, "MyDataFolder");
        // Debug.Log(Application.persistentDataPath);
        // Directory.CreateDirectory(folderPath);

        string filePath = Path.Combine(folderPath, "MyFile.txt");
        // File.WriteAllText(filePath, "Hello, Quest 2!");

        string readContent = File.ReadAllText(filePath);
        tmp.text = readContent;
    }
}
