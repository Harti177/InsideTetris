using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GamePuzzle
{
    public int xPosition;
    public int yPosition;
    public PuzzleType puzzleType;
    public bool activated;
}

public enum PuzzleType
{
    FourXTwo,
    ThreeXThree,
    FiveXFive
}