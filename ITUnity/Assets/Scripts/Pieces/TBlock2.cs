public class TBlock2 : TetrisPiece
{
    public override (bool, int[,]) CalculateDownPosition(int xPosition, int yPosition, int xLength, int yLength)
    {
        if (yPosition - 1 < 0)
        {
            return (true, null);
        }

        int[,] values = new int[4, 2];

        values[0, 0] = xPosition;
        values[0, 1] = yPosition - 1;

        CalculateRest(ref values, xLength, yLength);

        return (false, values);
    }

    public override int[,] CalculateLeftPosition(int xPosition, int yPosition, int xLength, int yLength)
    {
        int[,] values = new int[4, 2];

        values[0, 0] = xPosition - 1 < 0 ? xLength - 1 : xPosition - 1;
        values[0, 1] = yPosition;

        CalculateRest(ref values, xLength, yLength);

        return values;
    }

    public override int[,] CalculateRightPosition(int xPosition, int yPosition, int xLength, int yLength)
    {
        int[,] values = new int[4, 2];

        values[0, 0] = xPosition + 1 > xLength - 1 ? 0 : xPosition + 1;
        values[0, 1] = yPosition;

        CalculateRest(ref values, xLength, yLength);

        return values;
    }

    public override (bool, int[,]) CalculateFromRotatePosition(int xPosition, int yPosition, int xLength, int yLength, int from = -1)
    {
        if (yPosition - 1 < 0)
        {
            return (true, null);
        }

        int[,] values = new int[4, 2];

        values[0, 0] = xPosition + 1 > xLength - 1 ? 0 : xPosition + 1;
        values[0, 1] = yPosition - 1;

        CalculateRest(ref values, xLength, yLength);

        return (false, values);
    }

    protected override void CalculateRest(ref int[,] values, int xLength, int yLength)
    {
        values[1, 0] = values[0, 0];
        values[1, 1] = values[0, 1] + 1 > yLength - 1 ? -1 : values[0, 1] + 1;

        values[2, 0] = values[0, 0];
        values[2, 1] = values[0, 1] + 2 > yLength - 1 ? -1 : values[0, 1] + 2;

        values[3, 0] = values[0, 0] + 1 > xLength - 1 ? 0 : values[0, 0] + 1;
        values[3, 1] = values[1, 1];
    }
}