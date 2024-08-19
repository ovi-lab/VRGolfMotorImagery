using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Firebase.Database;

[Serializable]
public class dataToSave
{
    // Data to save before the trial
    public int feedBackGroup;
    // Data to save after the trial(gaming data)
    public List<Vector2> resultList;
    // Just keep the colomn but leave it black(will fill it after the experiment)
    
}
public class DataSaver : MonoBehaviour
{
    public dataToSave dataToSave;
    public string Id;
    private DatabaseReference dbRef;
    [SerializeField] private Renderer ballRenderer;
    [SerializeField] private GolfBallController golfBallController;

    
    private void Awake()
    {
        dbRef = FirebaseDatabase.DefaultInstance.RootReference;
    }
    public void SaveDataFn()
    {
        string json = JsonUtility.ToJson(dataToSave);
        dbRef.Child("participants").Child(Id).SetRawJsonValueAsync(json);
    }

    public void LoadDataFn()
    {
        StartCoroutine(LoadDataEnum());
    }
    IEnumerator LoadDataEnum()
    {
        var severData = dbRef.Child("participants").Child(Id).GetValueAsync();
        yield return new WaitUntil(predicate: (() => severData.IsCompleted));
        
        print("process is completed");
    
        DataSnapshot snapshot = severData.Result;
        string jsonData = snapshot.GetRawJsonValue();
    
        if (jsonData != null)
        {
            print("server data found!");
    
            dataToSave = JsonUtility.FromJson<dataToSave>(jsonData);
        }else
        {
            print("no data found");
        }
    }
}


