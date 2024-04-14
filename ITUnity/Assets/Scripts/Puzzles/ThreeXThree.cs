using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThreeXThree : MonoBehaviour
{
    public static int[,] CalculateInnerIndices(int xPosition, int yPosition)
    {
        int[,] values = new int[9, 2];

        for (int i = 0; i < 9; i++)
        {
            values[i, 0] = xPosition + (i % 3);
            values[i, 1] = yPosition + (i / 3);
        }

        return values;
    }
}
