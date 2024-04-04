using UnityEngine;

public class SavedGame
{
    public GameBlockProperty[,] gameBlockProperties; 
    public int score; 
    public int lines; 
    public int level; 
}

public class GameBlockProperty
{
    public bool locked;
    public SerializableColor color; 
}

public class SerializableColor
{
    public float r;
    public float g;
    public float b;
    public float a;
}