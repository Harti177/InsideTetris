using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using TMPro;

public class UserHandler : MonoBehaviour
{
    [SerializeField] private AzureHandler azureHandler; 

    private List<GameHighScore> highScores;
    private List<string> userNames;

    private string userName = "";

    [SerializeField] private TextMeshPro userNameText;
    [SerializeField] private GameObject loginButton;
    [SerializeField] private GameObject newuserButton;

    [SerializeField] private GameKeyboard keyboard;
    [SerializeField] private GameObject ui;

    private void Start()
    {
        StartCoroutine(azureHandler.GetHighScores(GetHighScoresCallBack));
    }

    private void GetHighScoresCallBack(List<GameHighScore> values)
    {
        highScores = new List<GameHighScore>(values);

        userNames = new List<string>();

        foreach (var highscore in highScores)
        {
            userNames.Add(highscore.userName);
        }
    }

    public void Login()
    {
        ui.SetActive(false);
        userNameText.gameObject.SetActive(false);
        loginButton.SetActive(false);
        newuserButton.SetActive(false);
        keyboard.gameObject.SetActive(true);
        keyboard.InitiateKeyboard("Enter your user name", LoginNameCallBack);
    }

    private void LoginNameCallBack(bool cancelled, string value)
    {
        if (cancelled)
        {
            keyboard.gameObject.SetActive(false);
            userName = "";
            ui.SetActive(true);
            userNameText.gameObject.SetActive(true);
            loginButton.SetActive(true);
            newuserButton.SetActive(true);
            return;
        }

        if (!userNames.Contains(value))
        {
            keyboard.InitiateKeyboard("User name not present. Enter correct name", LoginNameCallBack);
            return;
        }

        userName = value; 

        keyboard.InitiateKeyboard("Enter your password", LoginPasswordCallBack);
    }

    private void LoginPasswordCallBack(bool cancelled, string value)
    {
        if (cancelled)
        {
            keyboard.gameObject.SetActive(false);
            userName = "";
            ui.SetActive(true);
            userNameText.gameObject.SetActive(true);
            loginButton.SetActive(true);
            newuserButton.SetActive(true);
            return;
        }

        GameHighScore highscore = highScores.FirstOrDefault(x => x.userName == userName);

        if(highscore.userPassword != value)
        {
            keyboard.InitiateKeyboard("Password wrong. Enter your password again", LoginPasswordCallBack);
            return;
        }

        keyboard.gameObject.SetActive(false);
        userNameText.text = "User Name: " + userName;
        userNameText.gameObject.SetActive(true);
        ui.SetActive(true);
    }

    public void Newuser()
    {
        ui.SetActive(false);
        userNameText.gameObject.SetActive(false);
        loginButton.SetActive(false);
        newuserButton.SetActive(false);
        keyboard.gameObject.SetActive(true);
        keyboard.InitiateKeyboard("Enter a unique value for username", NewUserNameCallBack);
    }

    private void NewUserNameCallBack(bool cancelled, string value)
    {
        if (cancelled)
        {
            keyboard.gameObject.SetActive(false);
            userName = "";
            ui.SetActive(true);
            userNameText.gameObject.SetActive(true);
            loginButton.SetActive(true);
            newuserButton.SetActive(true);
            return;
        }

        if (userNames.Contains(value))
        {
            keyboard.InitiateKeyboard("Name already taken. Enter a another unique value for username", NewUserNameCallBack);
            return;
        }

        userName = value;

        keyboard.InitiateKeyboard("Enter a password value (min 4 characters)", NewUserPasswordCallBack);
    }

    private void NewUserPasswordCallBack(bool cancelled, string value)
    {
        if (cancelled)
        {
            keyboard.gameObject.SetActive(false);
            userName = "";
            ui.SetActive(true);
            userNameText.gameObject.SetActive(true);
            loginButton.SetActive(true);
            newuserButton.SetActive(true);
            return;
        }

        if (value.Length < 4)
        {
            keyboard.InitiateKeyboard("Password should be min 4 characters. Enter a another value", NewUserPasswordCallBack);
            return;
        }

        keyboard.gameObject.SetActive(false);
        userNameText.text = "User Name: " + userName;
        userNameText.gameObject.SetActive(true);
        ui.SetActive(true);

        StartCoroutine(azureHandler.SetUser(userName, value));
    }

    public string GetUserName()
    {
        return userName; 
    }
}
