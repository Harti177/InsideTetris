using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FourXTwo : MonoBehaviour
{
    public static int[,] CalculateInnerIndices(int xPosition, int yPosition)
    {
        int[,] values = new int[8, 2];

        for (int i = 0; i < 8; i++)
        {
            values[i, 0] = xPosition + (i % 4);
            values[i, 1] = yPosition + (i / 4);
        }

        return values;
    }
}
