using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Oculus.Interaction;
using System.Threading.Tasks;

//Each block in the wall 
public class GameBlock : MonoBehaviour
{
    private int xPosition;
    private int yPosition;

    [SerializeField] Color defaultColour;

    private bool locked = false;

    [SerializeField] GameObject cue; 

    public void SetXPosition(int x)
    {
        xPosition = x;
    }

    public void SetYPosition(int y)
    {
        yPosition = y; 
    }

    public int GetXPosition(int x)
    {
        return xPosition;  
    }

    public int GetYPosition(int x)
    {
        return yPosition;
    }

    public bool CheckBox()
    {
        return locked;
    }

    public void FillBox(Color color, bool lockIt, bool dontChangeColour = false)
    {
        GetComponent<MeshRenderer>().enabled = true;
        if (!dontChangeColour) GetComponent<MeshRenderer>().material.color = color;
        //cue.SetActive(false); 
        locked = lockIt; 
    }

    public void EmptyBox()
    {
        GetComponent<MeshRenderer>().enabled = false; 
        GetComponent<MeshRenderer>().material.color = Color.white; 
        //cue.SetActive(true);
        locked = false; 
    }


    public Task<(bool, Color)> GetBoxDetails()
    {
        return Task.FromResult((locked, GetComponent<MeshRenderer>().material.color));
    }

    public void SetInteractorPosition(Transform interactor)
    {
        interactor.parent = transform;
        interactor.localPosition = Vector3.zero; 
        interactor.localRotation = Quaternion.identity;
        interactor.Rotate(0f, 90f, 0f, Space.Self);
    }
}
