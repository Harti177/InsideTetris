using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Events;
using System.Linq;
using Newtonsoft.Json; 
    
public class AzureHandler : MonoBehaviour
{
    [SerializeField] private UserHandler userhandler;

    [SerializeField] private GameScore gameScore; 

    string setUrl = "https://insidetetris.azurewebsites.net/api/SetHighScores?code=psHBbDVlmQBeIvfL5UXFWVaJ3vujMzPWzPrMXftNh5f0AzFuHJDVLw==";
    string getUrl = "https://insidetetris.azurewebsites.net/api/GetHighScores?code=rOd0WQSw5sOuE8E-O7HmkESbaSa4HZu2mgrltff169kEAzFu0I3wcg=="; 

    public IEnumerator SetUser(string userName, string userPassword)
    {
        string json = "";

        using (UnityWebRequest getRequest = UnityWebRequest.Get(getUrl))
        {
            yield return getRequest.SendWebRequest();

            if (getRequest.result != UnityWebRequest.Result.Success)
            {

            }
            else
            {
                var data = getRequest.downloadHandler.text;

                List<GameHighScore> highScores = JsonConvert.DeserializeObject<List<GameHighScore>>(data);

                GameHighScore gameHighScore = new GameHighScore();
                gameHighScore.userName = userName;
                gameHighScore.userPassword = userPassword;

                highScores.Add(gameHighScore);

                List<GameHighScore> sortedScores = highScores.OrderByDescending(o => o.userScore).ToList();

                json = JsonConvert.SerializeObject(highScores.Take(10000));

                Debug.Log(json);
            }

            if (json != "")
            {
                using (UnityWebRequest setRequest = UnityWebRequest.Post(setUrl, json, "application/json"))
                {
                    yield return setRequest.SendWebRequest();

                    if (setRequest.result != UnityWebRequest.Result.Success)
                    {
                        Debug.LogError(setRequest.error);
                    }
                    else
                    {
                        Debug.Log("Score updated successfully");
                    }
                }
            }
        }
    }

    public IEnumerator SetHighScore(int userScore, string userName)
    {
        string json = "";

        using (UnityWebRequest getRequest = UnityWebRequest.Get(getUrl))
        {
            yield return getRequest.SendWebRequest();

            if (getRequest.result != UnityWebRequest.Result.Success)
            {
                
            }
            else
            {
                var data = getRequest.downloadHandler.text;

                List<GameHighScore> highScores = JsonConvert.DeserializeObject<List<GameHighScore>>(data);

                GameHighScore highscore = highScores.FirstOrDefault(x => x.userName == userName);

                if(highscore != null && highscore.userScore < userScore)
                {
                    highScores[highScores.IndexOf(highscore)].userScore = userScore;

                    json = JsonConvert.SerializeObject(highScores.OrderByDescending(o => o.userScore).ToList());

                    Debug.Log(json);
                }
            }

            if (json != "")
            {
                using (UnityWebRequest setRequest = UnityWebRequest.Post(setUrl, json, "application/json"))
                {
                    yield return setRequest.SendWebRequest();

                    if (setRequest.result != UnityWebRequest.Result.Success)
                    {
                        gameScore.RefreshScoreList(); 
                    }
                    else
                    {
                        Debug.Log("Score updated successfully");
                    }
                }
            }
        }
    }

    public IEnumerator CheckIfHighScore(int score, UnityAction<bool> callBack)
    {
        Debug.Log(score);
        using (UnityWebRequest getRequest = UnityWebRequest.Get(getUrl))
        {
            yield return getRequest.SendWebRequest();

            if (getRequest.result != UnityWebRequest.Result.Success)
            {
                callBack.Invoke(false);
            }
            else
            {
                var data = getRequest.downloadHandler.text;

                List<GameHighScore> highScores = JsonConvert.DeserializeObject<List<GameHighScore>>(data);

                GameHighScore highScore = new GameHighScore();
                highScore.userName = "insidetetris150320240149";
                highScore.userScore = score;

                highScores.Add(highScore);

                List<GameHighScore> sortedScores = highScores.OrderByDescending(o => o.userScore).ToList();

                GameHighScore thisScore = sortedScores.Where(score => score.userName == name).FirstOrDefault();
                if (sortedScores.IndexOf(thisScore) < 10000)
                {
                    callBack.Invoke(true);
                }
                else
                {
                    callBack.Invoke(false);
                }
            }
        }
    }

    public IEnumerator GetHighScores(UnityAction<List<GameHighScore>> callBack)
    {
        using (UnityWebRequest getRequest = UnityWebRequest.Get(getUrl))
        {
            yield return getRequest.SendWebRequest();

            if (getRequest.result != UnityWebRequest.Result.Success)
            {
                callBack.Invoke(new List<GameHighScore>());
            }
            else
            {
                var data = getRequest.downloadHandler.text;

                List<GameHighScore> scores = JsonConvert.DeserializeObject<List<GameHighScore>>(data);

                callBack.Invoke(scores);
            }
        }
    }
}
