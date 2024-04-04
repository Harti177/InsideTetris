using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using System.Threading.Tasks;
using TMPro;

public class GamePlay : MonoBehaviour
{
    [SerializeField] private GameWall gameWall;
    [SerializeField] private GameScore gameScore;
    [SerializeField] private AzureHandler azureHandler;
    [SerializeField] private UserHandler userHandler;
    [SerializeField] private SaveLoadHandler saveLoadHandler;

    private GameBlock[,] array = new GameBlock[0,0];
    private SavedGame savedGame;
    private int xLength = 0;
    private int yLength = 0;

    private float time = 0f;
    private float playTime = 0.5f;

    private int score = 0; 
    private int lines = 0;
    private int level = 0; 
   
    private GamePiece currentPiece;

    private GameMove currentMove;

    private bool initialised = false;
    private bool gameStarted = false;
    private bool stepReached = false;
    private bool gamePaused = false;
    private bool piecePlacedDown = false;
    private bool gameOver = false;
    private bool notResumed = false; 

    Array PieceTypeValues = Enum.GetValues(typeof(PieceType));

    #region UI
    [SerializeField] private GameObject loadingText;
    [SerializeField] private GameObject playButton;
    [SerializeField] private GameObject pauseButton;
    [SerializeField] private GameObject resumeGameButton;
    [SerializeField] private GameObject endButton;
    [SerializeField] private GameObject saveHighScoreButton;
    [SerializeField] private GameObject saveScoreText;
    [SerializeField] private GameObject userUI;

    [SerializeField] private TextMeshPro pauseResumeText;
    [SerializeField] private TextMeshPro loadingTextText;
    [SerializeField] private TextMeshPro saveScoreTextText;
    [SerializeField] private GameObject highScoresText;
    #endregion UI

    [SerializeField] private Transform interactor;

    private Color[] colors = new Color[3] { Color.red, Color.yellow, Color.blue };

    [SerializeField] private bool useSample;

    // Update is called once per frame
    void Update()
    {
        if (gamePaused && saveScoreText.activeInHierarchy && score > 0)
        {
            string gameOverText = gameOver ? "Game over! " : ""; 
            if (userHandler.GetUserName() == "")
            {
                saveScoreTextText.text = gameOverText + "Congrats! High score! Login or register to save score";
            }
            else
            {
                saveScoreTextText.text = gameOverText + "Congrats! High score! Click save to save score";
                saveHighScoreButton.SetActive(true);
            }
        }

        if (gamePaused && !saveScoreText.activeInHierarchy && gameOver)
        {
            string gameOverText = gameOver ? "Game over! " : "";
            loadingTextText.text = gameOverText + score; 
        }

        if(gameStarted && !Application.isFocused && !gamePaused && !useSample)
        {
            OnPauseButtonPressed(); 
        }

        //Game Paused
        if (gamePaused)
        {
            return; 
        }

        //Move time
        time += Time.deltaTime;

        //Game Play
        if (gameStarted)
        {
            //Setting the level 
            level = (lines / 4);

            //Piece initialization
            if (piecePlacedDown || currentPiece == null)
            {
                if(currentPiece != null) FillBox(currentPiece.values, true);

                if (piecePlacedDown)
                {
                    int noOfLinesCompleteThisRound = 0; 
                    for (int j = 0; j < array.GetLength(1); j++)
                    {
                        bool notFilled = false; 
                        for (int i = 0; i < array.GetLength(0); i++)
                        {
                            if (!CheckBox(i, j)) notFilled = true;

                            if (notFilled)
                            {
                                break;
                            }

                        }
                        if (!notFilled)
                        {
                            noOfLinesCompleteThisRound++;
                            for (int b = j; b < array.GetLength(1); b++)
                            {
                                for (int a = 0; a < array.GetLength(0); a++)
                                {
                                    EmptyBox(a, b);

                                    if(b != array.GetLength(1) - 1)
                                    {
                                        if(CheckBox(a, b + 1))
                                        {
                                            FillBox(a, b, true, true);
                                        }
                                    }
                                }
                            }

                            j--;
                        }
                    }

                    score += 4;

                    if (noOfLinesCompleteThisRound > 0)
                        score += (noOfLinesCompleteThisRound + (2 * (noOfLinesCompleteThisRound - 1))) * (100 * (xLength/40));
                    loadingTextText.text = "Score " + score.ToString();
                    lines += noOfLinesCompleteThisRound; 
                }

                int xPositionForNext = UnityEngine.Random.Range(0, array.GetLength(0));
                int yPositionForNext = array.GetLength(1);

                piecePlacedDown = false;

                float speed = score > 50 ? (score / 50) : 5; 
                playTime = 0.6f;
                currentMove = null;

                currentPiece = new GamePiece();
                currentPiece.pieceType = (PieceType)PieceTypeValues.GetValue(UnityEngine.Random.Range(0, 28));

                currentPiece.color = colors[UnityEngine.Random.Range(0, colors.Length)];

                currentPiece.yPosition = yPositionForNext;
                currentPiece.xPosition = xPositionForNext;
            }

            //Set null move if the piece is not fully available 
            if (currentMove != null && currentPiece.yPosition >= yLength)
            {
                currentMove = null;
            }

            //For now no move and step down not at the same frame - But its not working like that - check again anyway it should not work like that
            if (currentMove == null)
            {
                stepReached = time > playTime ? true : false;

                if (stepReached)
                {
                    time = 0f;
                }
            }

            //Bring the piece down fast. Later a way to stop moving the piece fast 
            if (currentMove != null && currentMove.moveType == MoveType.down)
            {
                playTime = 0.05f;
                currentMove = null;
            }

            int[,] newPieceValues = null;

            PieceType pieceType = currentPiece.pieceType;
            Debug.Log(pieceType);

            TetrisPiece currentTetrisPiece = new TBlock1();
            TetrisPiece toRotateTetrisPiece = new TBlock1();
            PieceType toRotatePieceType = PieceType.TBlock2;
            int from = -1; 
            
            switch (pieceType)
            {
                case PieceType.IBlock1:
                    {
                        currentTetrisPiece = new IBlockHorizontal();
                        toRotateTetrisPiece = new IBlockVertical();
                        toRotatePieceType = PieceType.IBlock2;
                        from = 1; 
                        break;
                    }
                case PieceType.IBlock2:
                    {
                        currentTetrisPiece = new IBlockVertical();
                        toRotateTetrisPiece = new IBlockHorizontal();
                        toRotatePieceType = PieceType.IBlock3;
                        from = 2;
                        break;
                    }
                case PieceType.IBlock3:
                    {
                        currentTetrisPiece = new IBlockHorizontal();
                        toRotateTetrisPiece = new IBlockVertical();
                        toRotatePieceType = PieceType.IBlock4;
                        from = 3;
                        break;
                    }
                case PieceType.IBlock4:
                    {
                        currentTetrisPiece = new IBlockVertical();
                        toRotateTetrisPiece = new IBlockHorizontal();
                        toRotatePieceType = PieceType.IBlock1;
                        from = 4;
                        break;
                    }
                case PieceType.OBlock1:
                case PieceType.OBlock2:
                case PieceType.OBlock3: 
                case PieceType.OBlock4:
                    {
                        currentTetrisPiece = new OBlock();
                        toRotateTetrisPiece = new OBlock();
                        toRotatePieceType = PieceType.OBlock1;
                        from = -1;
                        break;
                    }
                case PieceType.TBlock1:
                    {
                        currentTetrisPiece = new TBlock1();
                        toRotateTetrisPiece = new TBlock2();
                        toRotatePieceType = PieceType.TBlock2;
                        break;
                    }
                case PieceType.TBlock2:
                    {
                        currentTetrisPiece = new TBlock2();
                        toRotateTetrisPiece = new TBlock3();
                        toRotatePieceType = PieceType.TBlock3;
                        break;
                    }
                case PieceType.TBlock3:
                    {
                        currentTetrisPiece = new TBlock3();
                        toRotateTetrisPiece = new TBlock4();
                        toRotatePieceType = PieceType.TBlock4;
                        break;
                    }
                case PieceType.TBlock4:
                    {
                        currentTetrisPiece = new TBlock4();
                        toRotateTetrisPiece = new TBlock1();
                        toRotatePieceType = PieceType.TBlock1;
                        break;
                    }
                case PieceType.ZBlock1:
                    {
                        currentTetrisPiece = new ZBlockHorizontal();
                        toRotateTetrisPiece = new ZBlockVertical();
                        toRotatePieceType = PieceType.ZBlock2;
                        from = 1; 
                        break;
                    }
                case PieceType.ZBlock2:
                    {
                        currentTetrisPiece = new ZBlockVertical();
                        toRotateTetrisPiece = new ZBlockHorizontal();
                        toRotatePieceType = PieceType.ZBlock3;
                        from = 2;
                        break;
                    }
                case PieceType.ZBlock3:
                    {
                        currentTetrisPiece = new ZBlockHorizontal();
                        toRotateTetrisPiece = new ZBlockVertical();
                        toRotatePieceType = PieceType.ZBlock4;
                        from = 3;
                        break;
                    }
                case PieceType.ZBlock4:
                    {
                        currentTetrisPiece = new ZBlockVertical();
                        toRotateTetrisPiece = new ZBlockHorizontal();
                        toRotatePieceType = PieceType.ZBlock1;
                        from = 4;
                        break;
                    }
                case PieceType.SBlock1:
                    {
                        currentTetrisPiece = new SBlockHorizontal();
                        toRotateTetrisPiece = new SBlockVertical();
                        toRotatePieceType = PieceType.SBlock2;
                        from = 1;
                        break;
                    }
                case PieceType.SBlock2:
                    {
                        currentTetrisPiece = new SBlockVertical();
                        toRotateTetrisPiece = new SBlockHorizontal();
                        toRotatePieceType = PieceType.SBlock3;
                        from = 2;
                        break;
                    }
                case PieceType.SBlock3:
                    {
                        currentTetrisPiece = new SBlockHorizontal();
                        toRotateTetrisPiece = new SBlockVertical();
                        toRotatePieceType = PieceType.SBlock4;
                        from = 3;
                        break;
                    }
                case PieceType.SBlock4:
                    {
                        currentTetrisPiece = new SBlockVertical();
                        toRotateTetrisPiece = new SBlockHorizontal();
                        toRotatePieceType = PieceType.SBlock1;
                        from = 4;
                        break;
                    }
                case PieceType.LBlock1:
                    {
                        currentTetrisPiece = new LBlock1();
                        toRotateTetrisPiece = new LBlock2();
                        toRotatePieceType = PieceType.LBlock2;
                        break;
                    }
                case PieceType.LBlock2:
                    {
                        currentTetrisPiece = new LBlock2();
                        toRotateTetrisPiece = new LBlock3();
                        toRotatePieceType = PieceType.LBlock3;
                        break;
                    }
                case PieceType.LBlock3:
                    {
                        currentTetrisPiece = new LBlock3();
                        toRotateTetrisPiece = new LBlock4();
                        toRotatePieceType = PieceType.LBlock4;
                        break;
                    }
                case PieceType.LBlock4:
                    {
                        currentTetrisPiece = new LBlock4();
                        toRotateTetrisPiece = new LBlock1();
                        toRotatePieceType = PieceType.LBlock1;
                        break;
                    }
                case PieceType.JBlock1:
                    {
                        currentTetrisPiece = new JBlock1();
                        toRotateTetrisPiece = new JBlock2();
                        toRotatePieceType = PieceType.JBlock2;
                        break;
                    }
                case PieceType.JBlock2:
                    {
                        currentTetrisPiece = new JBlock2();
                        toRotateTetrisPiece = new JBlock3();
                        toRotatePieceType = PieceType.JBlock3;
                        break;
                    }
                case PieceType.JBlock3:
                    {
                        currentTetrisPiece = new JBlock3();
                        toRotateTetrisPiece = new JBlock4();
                        toRotatePieceType = PieceType.JBlock4;
                        break;
                    }
                case PieceType.JBlock4:
                    {
                        currentTetrisPiece = new JBlock4();
                        toRotateTetrisPiece = new JBlock1();
                        toRotatePieceType = PieceType.JBlock1;
                        break;
                    }
            }

            if (stepReached)
            {
                bool placedDown;

                (placedDown, newPieceValues) = currentTetrisPiece.CalculateDownPosition(currentPiece.xPosition, currentPiece.yPosition, array.GetLength(0), array.GetLength(1));

                if (placedDown)
                {
                    piecePlacedDown = true;
                    return;
                }
            }

            if (currentMove != null && currentMove.moveType == MoveType.left)
            {
                newPieceValues = currentTetrisPiece.CalculateLeftPosition(currentPiece.xPosition, currentPiece.yPosition, array.GetLength(0), array.GetLength(1));
            }

            if (currentMove != null && currentMove.moveType == MoveType.right)
            {
                newPieceValues = currentTetrisPiece.CalculateRightPosition(currentPiece.xPosition, currentPiece.yPosition, array.GetLength(0), array.GetLength(1));
            }

            if (currentMove != null && currentMove.moveType == MoveType.rotate)
            {
                bool blocked;

                (blocked, newPieceValues) = toRotateTetrisPiece.CalculateFromRotatePosition(currentPiece.xPosition, currentPiece.yPosition, array.GetLength(0), array.GetLength(1), from);

                if (blocked)
                {
                    newPieceValues = null;
                }
                else
                {
                    currentPiece.pieceType = toRotatePieceType;
                }
            }

            if (newPieceValues != null)
            {
                Debug.Log(string.Join(" ", newPieceValues.Cast<int>()));
                if (!CheckBox(newPieceValues))
                {
                    if (currentPiece.values != null) EmptyBox(currentPiece.values);
                    FillBox(newPieceValues);
                    currentPiece.xPosition = newPieceValues[0, 0];
                    currentPiece.yPosition = newPieceValues[0, 1];              
                    currentPiece.values = new int[4, 2];
                    Array.Copy(newPieceValues, currentPiece.values, newPieceValues.Length);
                    MoveInteractor();
                }
                else
                {
                    if((newPieceValues[0, 1] == yLength - 1 || newPieceValues[1, 1] == yLength - 1 || newPieceValues[2, 1] == yLength - 1 || newPieceValues[3, 1] == yLength - 1) && currentMove == null)
                    {
                        GameOver();
                    }
                    else if(currentMove != null)
                    {

                    }
                    else
                    {
                        piecePlacedDown = true; 
                    }
                }
            }

            currentMove = null;
        }

        //Localtesting not in vr - Initialise the bricks in the wall 
        if (!initialised && useSample)
        {
            time = 0f;

            initialised = true;

            gameWall.CreateBricksSample(out array);

            xLength = array.GetLength(0);
            yLength = array.GetLength(1);

            loadingText.SetActive(false);
            playButton.SetActive(true);
            highScoresText.SetActive(true);
            userUI.SetActive(true);

            CheckAndLoadGameIfSaved(); 
        }

        //Initialise the bricks in the wall 
        if (!initialised) 
        {
            time = 0f;

            //Check if scene sense successfully initialised 
            if (!gameWall.IsWallReady()) 
            {
                return;
            }

            initialised = true;

            gameWall.CreateBricks(out array);

            xLength = array.GetLength(0);
            yLength = array.GetLength(1);

            loadingText.gameObject.SetActive(false);
            playButton.gameObject.SetActive(true);
            highScoresText.gameObject.SetActive(true);
            userUI.SetActive(true);

            CheckAndLoadGameIfSaved();
        }
    }

    private void MoveInteractor()
    {
        int xPosition;
        int yPosition;

        (xPosition, yPosition) = GetCentrePosition();

        SetInteractorPosition(xPosition, yPosition);
    }

    //Get centre of each tetris piece type 
    private (int, int) GetCentrePosition()
    {
        int xPosition = -1;
        int yPosition = -1;

        switch (currentPiece.pieceType)
        {
            case PieceType.IBlock1:
            case PieceType.IBlock2:
            case PieceType.IBlock3:
            case PieceType.IBlock4:
                xPosition = currentPiece.values[2, 0];
                yPosition = currentPiece.values[2, 1];
                break;
            case PieceType.OBlock1:
            case PieceType.OBlock2:
            case PieceType.OBlock3:
            case PieceType.OBlock4:
                xPosition = currentPiece.values[0, 0];
                yPosition = currentPiece.values[0, 1];
                break;
            case PieceType.TBlock1:
                xPosition = currentPiece.values[1, 0];
                yPosition = currentPiece.values[1, 1];
                break;
            case PieceType.TBlock2:
                xPosition = currentPiece.values[1, 0];
                yPosition = currentPiece.values[1, 1];
                break;
            case PieceType.TBlock3:
                xPosition = currentPiece.values[2, 0];
                yPosition = currentPiece.values[2, 1];
                break;
            case PieceType.TBlock4:
                xPosition = currentPiece.values[1, 0];
                yPosition = currentPiece.values[1, 1];
                break;
            case PieceType.ZBlock1:
                xPosition = currentPiece.values[0, 0];
                yPosition = currentPiece.values[0, 1];
                break;
            case PieceType.ZBlock2:
                xPosition = currentPiece.values[1, 0];
                yPosition = currentPiece.values[1, 1];
                break;
            case PieceType.ZBlock3:
                xPosition = currentPiece.values[3, 0];
                yPosition = currentPiece.values[3, 1];
                break;
            case PieceType.ZBlock4:
                xPosition = currentPiece.values[2, 0];
                yPosition = currentPiece.values[2, 1];
                break;
            case PieceType.SBlock1:
                xPosition = currentPiece.values[1, 0];
                yPosition = currentPiece.values[1, 1];
                break;
            case PieceType.SBlock2:
                xPosition = currentPiece.values[2, 0];
                yPosition = currentPiece.values[2, 1];
                break;
            case PieceType.SBlock3:
                xPosition = currentPiece.values[2, 0];
                yPosition = currentPiece.values[2, 1];
                break;
            case PieceType.SBlock4:
                xPosition = currentPiece.values[1, 0];
                yPosition = currentPiece.values[1, 1];
                break;
            case PieceType.LBlock1:
                xPosition = currentPiece.values[1, 0];
                yPosition = currentPiece.values[1, 1];
                break;
            case PieceType.LBlock2:
                xPosition = currentPiece.values[2, 0];
                yPosition = currentPiece.values[2, 1];
                break;
            case PieceType.LBlock3:
                xPosition = currentPiece.values[2, 0];
                yPosition = currentPiece.values[2, 1];
                break;
            case PieceType.LBlock4:
                xPosition = currentPiece.values[1, 0];
                yPosition = currentPiece.values[1, 1];
                break;
            case PieceType.JBlock1:
                xPosition = currentPiece.values[1, 0];
                yPosition = currentPiece.values[1, 1];
                break;
            case PieceType.JBlock2:
                xPosition = currentPiece.values[1, 0];
                yPosition = currentPiece.values[1, 1];
                break;
            case PieceType.JBlock3:
                xPosition = currentPiece.values[2, 0];
                yPosition = currentPiece.values[2, 1];
                break;
            case PieceType.JBlock4:
                xPosition = currentPiece.values[2, 0];
                yPosition = currentPiece.values[2, 1];
                break;

        }

        return (xPosition, yPosition); 
    }

    private void GameOver()
    {
        gameOver = true;

        OnPauseButtonPressed();
        
        endButton.SetActive(true);

        saveLoadHandler.DeleteGame();
    }

    private void ResetGame()
    {
        for (int i = 0; i < array.GetLength(0); i++)
        {
            for (int j = 0; j < array.GetLength(1); j++)
            {
                EmptyBox(i, j);
            }
        }

        currentPiece = null;
        currentMove = null;
        score = 0;
        level = 0;
        lines = 0; 
    }

    private async void CheckAndLoadGameIfSaved()
    {
        savedGame = await saveLoadHandler.LoadGameAsync();
        if (savedGame != null && 
            savedGame.gameBlockProperties.GetLength(0) == array.GetLength(0) &&
            savedGame.gameBlockProperties.GetLength(1) == array.GetLength(1))
        {
            resumeGameButton.SetActive(true);
        }
    }

    public bool CheckBox(int[,] values)
    {
        for (int i = 0; i < values.GetLength(0); i++)
        {
                if (values[i, 0] != -1 && values[i, 1] != -1 && CheckBox(values[i, 0], values[i, 1])) return true; 
        }

        return false;
    }

    private bool CheckBox(int x, int y)
    {
        return array[x, y].CheckBox();
    }

    public void FillBox(int[,] values, bool lockIt = false)
    {
        for (int i = 0; i < values.GetLength(0); i++)
        {
            if (values[i, 0] != -1 && values[i, 1] != -1)
                FillBox(values[i, 0], values[i, 1], lockIt);
        }
    }

    private void FillBox(int x, int y, bool lockIt, bool dontChangeColour = false)
    {
        array[x, y].FillBox(currentPiece.color, lockIt, dontChangeColour);
    }

    public void EmptyBox(int[,] values)
    {
        for (int i = 0; i < values.GetLength(0); i++)
        {
            if (values[i, 0] != -1 && values[i, 1] != -1)
                EmptyBox(values[i, 0], values[i, 1]);
        }

    }

    private void EmptyBox(int x, int y)
    {
        array[x, y].EmptyBox(); 
    }

    private async Task<bool> SaveGameAsync()
    {
        GameBlockProperty[,] gameBlockProperties = new GameBlockProperty[array.GetLength(0), array.GetLength(1)];

        for (int i = 0; i < array.GetLength(0); i++)
        {
            for (int j = 0; j < array.GetLength(1); j++)
            {
                GameBlockProperty gameBlockProperty = new GameBlockProperty();
                
                bool locked;
                Color color;
                (locked, color) = await array[i, j].GetBoxDetails();

                SerializableColor serializableColor = new SerializableColor();
                serializableColor.r = color.r;
                serializableColor.g = color.g;
                serializableColor.b = color.b;
                serializableColor.a = color.a;

                gameBlockProperty.locked = locked;
                gameBlockProperty.color = serializableColor;

                gameBlockProperties[i, j] = gameBlockProperty; 
            }
        }

        savedGame = new SavedGame();
        savedGame.gameBlockProperties = gameBlockProperties;
        savedGame.score = score;
        savedGame.lines = lines;
        savedGame.level = level;

        await saveLoadHandler.SaveGameAsync(savedGame);

        return true; 
    }

    private void SetInteractorPosition(int x, int y)
    {
        if(x == -1 || y == -1)
        {
            x = currentPiece.values[0, 0];
            y = currentPiece.values[1, 0];
        }

        array[x, y].SetInteractorPosition(interactor);
    }

    [ContextMenu("OnPlayButtonPressed")]
    public void OnPlayButtonPressed()
    {
        for (int j = 0; j < yLength; j++)
        {
            for (int i = 0; i < xLength; i++)
            {
                array[i, j].gameObject.SetActive(true);
                array[i, j].EmptyBox();
            }
        }

        currentPiece = null;
        currentMove = null;

        score = 0;
        lines = 0;
        level = 0;

        playButton.SetActive(false);
        resumeGameButton.SetActive(false);
        saveLoadHandler.DeleteGame(); 
        highScoresText.SetActive(false);
        userUI.SetActive(false);

        gameStarted = true;
        Debug.Log("Play has started");
        gamePaused = false;
        notResumed = true; 

        loadingText.SetActive(true);
        loadingTextText.text = "Score " + score.ToString();

        interactor.gameObject.SetActive(true);
    }

    [ContextMenu("OnResumeButtonPressed")]
    public void OnResumeGameButtonPressed()
    {
        for (int j = 0; j < yLength; j++)
        {
            for (int i = 0; i < xLength; i++)
            {
                array[i, j].gameObject.SetActive(true);

                if (savedGame.gameBlockProperties[i, j].locked)
                {
                    Color color = new Color(savedGame.gameBlockProperties[i, j].color.r, savedGame.gameBlockProperties[i, j].color.g, savedGame.gameBlockProperties[i, j].color.b, savedGame.gameBlockProperties[i, j].color.a);
                    array[i, j].FillBox(color, true, false);
                }
                else
                {
                    array[i, j].EmptyBox(); 
                }
            }
        }

        currentPiece = null;
        currentMove = null;

        score = savedGame.score;
        lines = savedGame.lines;
        level = savedGame.level; 

        playButton.SetActive(false);
        resumeGameButton.SetActive(false);
        highScoresText.SetActive(false);
        userUI.SetActive(false);

        gameStarted = true;
        Debug.Log("Resume has started");
        gamePaused = false;
        notResumed = false;

        loadingText.SetActive(true);
        loadingTextText.text = "Score " + score.ToString();

        interactor.gameObject.SetActive(true);
    }

    [ContextMenu("OnPauseButtonPressed")]
    public void OnPauseButtonPressed()
    {
        gamePaused = !gamePaused;

        interactor.gameObject.SetActive(!gamePaused);
        pauseButton.SetActive(gamePaused);
        endButton.SetActive(gamePaused);
        saveHighScoreButton.SetActive(gamePaused);
        saveScoreText.SetActive(gamePaused);
        userUI.SetActive(gamePaused);

        if (gamePaused && notResumed)
        {
            StartCoroutine(azureHandler.CheckIfHighScore(score, value =>
            {
                if (value)
                {
                    saveScoreText.SetActive(true);
                }
            }));
        }
    }

    [ContextMenu("OnEndButtonPressed")]
    public async void OnEndButtonPressed()
    {
        if(!gameOver) await SaveGameAsync();

        CheckAndLoadGameIfSaved(); 

        ResetGame();

        playButton.SetActive(true);
        highScoresText.SetActive(true);
        pauseButton.SetActive(false);
        endButton.SetActive(false);
        saveHighScoreButton.SetActive(false);
        saveScoreText.SetActive(false);
        loadingText.SetActive(false);

        gameStarted = false;
        gamePaused = false;

        interactor.gameObject.SetActive(false);

        gameScore.RefreshScoreList(); 
    }

    [ContextMenu("OnSaveButtonPressed")]
    public void OnSaveButtonPressed()
    {
        StartCoroutine(azureHandler.SetHighScore(score, userHandler.GetUserName()));
    }

    public void OnMovePlayed(string move)
    {
        MoveType moveType = MoveType.left;

        switch (move)
        {
            case "left":
                moveType = MoveType.left;
                break;
            case "right":
                moveType = MoveType.right;
                break;
            case "down":
                moveType = MoveType.down;
                break;
            case "rotate":
                moveType = MoveType.rotate;
                break;
        }
        currentMove = new GameMove();
        currentMove.moveType = moveType;
    }

    public bool IsPlaying()
    {
        return gameStarted; 
    }

    private async void OnApplicationQuit()
    {
        if(!gameOver && gameStarted) await SaveGameAsync();
    }
}