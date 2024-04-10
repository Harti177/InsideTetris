using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckInViewPort : MonoBehaviour
{
    public Vector3 IsObjectInViewPort()
    {
        return Camera.main.WorldToViewportPoint(transform.position);
    }
}
