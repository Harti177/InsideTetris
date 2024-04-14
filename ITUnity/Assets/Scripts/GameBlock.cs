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

    [SerializeField] private GameObject puzzlePrefab; 
    private GameObject puzzleGO; 
    [SerializeField] private GameObject emptyEffectPrefab; 
    private GameObject emptyEffectGO; 

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

    public void FillBox(Color color, bool lockIt, bool centre)
    {
        MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
        meshRenderer.enabled = true;
        meshRenderer.material.color = color;
        meshRenderer.material.SetColor("_EmissionColor", color);

        if(centre)
        {
            if (!lockIt)
            {
                meshRenderer.material.EnableKeyword("_EMISSION");
                meshRenderer.material.globalIlluminationFlags = MaterialGlobalIlluminationFlags.None;
            }
            else
            {
                meshRenderer.material.DisableKeyword("_EMISSION");
                meshRenderer.material.globalIlluminationFlags = MaterialGlobalIlluminationFlags.EmissiveIsBlack;
            }
        }

        if (puzzleGO != null && lockIt)
        {
            puzzleGO.GetComponent<MeshRenderer>().material.color = color;
            puzzleGO.GetComponent<MeshRenderer>().material.SetColor("_EmissionColor", color);
            puzzleGO.transform.localScale = Vector3.one * 1.1f; 
        }

        //cue.SetActive(false); 
        locked = lockIt; 
    }

    public void EmptyBox()
    {
        MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
        meshRenderer.enabled = false;
        meshRenderer.material.color = Color.white;
        //cue.SetActive(true);
        meshRenderer.material.DisableKeyword("_EMISSION");
        meshRenderer.material.globalIlluminationFlags = MaterialGlobalIlluminationFlags.EmissiveIsBlack;

        if (locked == true)
        {
            emptyEffectGO = Instantiate(emptyEffectPrefab, transform);
            emptyEffectGO.transform.localPosition = Vector3.zero;
            emptyEffectGO.transform.localRotation = Quaternion.identity;
            emptyEffectGO.transform.localScale = Vector3.one;
            StartCoroutine(DeleteEffect(2f));
        }

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

    public Color GetColor()
    {
        return GetComponent<MeshRenderer>().material.color; 
    }

    public void ActivatePuzzle()
    {
        puzzleGO = Instantiate(puzzlePrefab, transform);
        puzzleGO.transform.localPosition = Vector3.zero;
        puzzleGO.transform.localRotation = Quaternion.identity;
        puzzleGO.transform.localScale = Vector3.one * 0.25f;

        if (locked)
        {
            puzzleGO.GetComponent<MeshRenderer>().material.color = GetComponent<MeshRenderer>().material.color;
            puzzleGO.GetComponent<MeshRenderer>().material.SetColor("_EmissionColor", GetComponent<MeshRenderer>().material.color);
            puzzleGO.transform.localScale = Vector3.one * 1.1f;
        }
    }

    public void DeActivatePuzzle()
    {
        Destroy(puzzleGO);
    }

    public bool IsPuzzleActive()
    {
        return puzzleGO != null; 
    }

    IEnumerator DeleteEffect(float time)
    {
        yield return new WaitForSeconds(time);

        Destroy(emptyEffectGO); 
    }
}
