public abstract class TetrisPiece
{
    public abstract (bool, int[,]) CalculateDownPosition(int xPosition, int yPosition, int xLength, int yLength);

    public abstract int[,] CalculateLeftPosition(int xPosition, int yPosition, int xLength, int yLength);

    public abstract int[,] CalculateRightPosition(int xPosition, int yPosition, int xLength, int yLength);

    public abstract (bool, int[,]) CalculateFromRotatePosition(int xPosition, int yPosition, int xLength, int yLength, int from);

    protected abstract void CalculateRest(ref int[,] values, int xLength, int yLength);
}
