using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;
using System.Text;
using System.Linq;
using System;

public class APIManager : MonoBehaviour
{

    private string apiUrl = "ADD_YOUR_API_ENDPOINT_LINK";

    public void TrainData(List<DataEntry> data) {

        StartCoroutine(TrainNewData(ConvertToTrainingData(data)));

    }
    IEnumerator TrainNewData(List<TrainingData> data)
    {
        string route = "tain_new_data";
        string jsonData = JsonConvert.SerializeObject(data);

        WWWForm form = new WWWForm();
        form.AddField("route", route);
        form.AddField("data", jsonData);
        using UnityWebRequest www = UnityWebRequest.Post(apiUrl, form);

        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError(www.error);
        }
        else
        {
            string json = www.downloadHandler.text;
            
            Debug.Log(json);
        }      
    }
    public IEnumerator PredictData(List<DataEntry> dataEntries, Action<List<TrainingData>,List<DataEntry>> callback)
    {
        List<TrainingData> data=ConvertToTrainingData(dataEntries);
        string route = "predict";
        string jsonData = JsonConvert.SerializeObject(data);

        WWWForm form = new WWWForm();
        form.AddField("route", route);
        form.AddField("data", jsonData);
        using UnityWebRequest www = UnityWebRequest.Post(apiUrl, form);

        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError(www.error);
        }
        else
        {
            string json = www.downloadHandler.text;
            List<TrainingData> predictions = JsonConvert.DeserializeObject<List<TrainingData>>(json);
          
            //Debug.Log(json);
            callback(predictions,dataEntries);

        }
    }

    List<TrainingData> ConvertToTrainingData(List<DataEntry> dataEntries)
    {
        return dataEntries.Select(entry => new TrainingData
        {
            features = new List<float> { entry.CorrectAnswer, entry.TimeSpent, entry.HesitationTimes, entry.Difficulty },
            label = entry.Weakness
        }).ToList();
    }
    List<PredectingData> ConvertToPredictingData(List<DataEntry> dataEntries)
    {
        return dataEntries.Select(entry => new PredectingData
        {
            features = new List<float> { entry.CorrectAnswer, entry.TimeSpent, entry.HesitationTimes, entry.Difficulty },
        }).ToList();
    }
}




// Define data structure
[System.Serializable]
public class TrainingData
{
    public List<float> features;
    public int label;
}
[System.Serializable]
public class PredectingData
{
    public List<float> features;
}

