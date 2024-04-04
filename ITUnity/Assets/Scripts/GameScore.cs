using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Linq; 

public class GameScore : MonoBehaviour
{
    [SerializeField] private AzureHandler azureHandler;

    [SerializeField] private TextMeshPro[] namesText;
    [SerializeField] private TextMeshPro[] scoresText;

    [SerializeField] private TextMeshPro nameText;
    [SerializeField] private TextMeshPro scoreText;

    // Start is called before the first frame update
    void Start()
    {
        RefreshScoreList(); 
    }

    private void GetHighScoresCallBack(List<GameHighScore> highScores)
    {
        int count = highScores.Count >= 10 ? 10 : highScores.Count;

        for(int i = 0; i < count; i++)
        {
            namesText[i].text = (i+1) + ". " + highScores[i].userName;
            scoresText[i].text = highScores[i].userScore.ToString(); 
        }

        GameHighScore highScore = highScores.FirstOrDefault(x => x.userName == "Hari");
        if(highScore != null)
        {
            nameText.text = (highScores.IndexOf(highScore) + 1) + ". " + highScore.userName;
            scoreText.text = highScore.userScore.ToString();
        } 
    } 

    public void RefreshScoreList()
    {
        StartCoroutine(azureHandler.GetHighScores(GetHighScoresCallBack));
    }
}
