using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FiveXFive : MonoBehaviour
{
    public static int[,] CalculateInnerIndices(int xPosition, int yPosition)
    {
        int[,] values = new int[25, 2];

        for (int i = 0; i < 25; i++)
        {
            values[i, 0] = xPosition + (i % 5);
            values[i, 1] = yPosition + (i / 5);
        }

        return values;
    }
}
