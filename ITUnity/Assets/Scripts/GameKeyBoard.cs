using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;
using System.Threading.Tasks; 

public class GameKeyboard : MonoBehaviour
{
    [SerializeField] private TextMeshPro inputField;
    [SerializeField] private TextMeshPro hintText;

    private UnityAction<bool, string> callBack; 

    private string currentText = "";

    public void ButtonClicked(string character)
    {
        if (character == "enter")
        {
            callBack.Invoke(false, inputField.text);
        }
        else if (character == "backspace")
        {
            // Handle Backspace key press
            if (currentText.Length > 0)
            {
                currentText = currentText.Substring(0, currentText.Length - 1);
                inputField.text = currentText;
            }
        }else if (character == "cancel")
        {
            callBack.Invoke(true, "");
        }
        else
        {
            currentText += character;
            inputField.text = currentText;
        }
    }

    public void InitiateKeyboard(string hint, UnityAction<bool, string> action)
    {
        hintText.text = hint;
        inputField.text = "";
        currentText = "";

        callBack = action; 
    }
}