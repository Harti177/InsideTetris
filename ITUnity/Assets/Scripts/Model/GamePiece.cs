using UnityEngine;

//Game piece
public class GamePiece
{
    public PieceType pieceType;
    
    //xPosition of the origin block in the wall 
    public int xPosition = -1; 

    //yPosition of the origin block in the wall 
    public int yPosition = -1;
    
    //Holds the position of each block of the piece in the wall (4 points - each point x and y - 2 * 4)
    public int[,] values;
    
    public Color color;

    public bool seenByTheUser = false; 
}

public enum PieceType
{
    IBlock1,
    IBlock2,
    IBlock3,
    IBlock4,
    OBlock1,
    OBlock2,
    OBlock3,
    OBlock4,
    TBlock1,
    TBlock2,
    TBlock3,
    TBlock4,
    ZBlock1,
    ZBlock2,
    ZBlock3,
    ZBlock4,
    SBlock1,
    SBlock2,
    SBlock3,
    SBlock4,
    LBlock1,
    LBlock2,
    LBlock3,
    LBlock4,
    JBlock1,
    JBlock2,
    JBlock3,
    JBlock4
}
