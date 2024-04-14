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
    private float placeDownTime = 0f;
    private float piecesLeftUntilPuzzleExplodes = 0; 

    private int score = 0; 
    private int lines = 0;
    private int level = 0;
    private int puzzleLevel = 50;
    private int puzzleCode = 0;
   
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
    Array PuzzleTypeValues = Enum.GetValues(typeof(PuzzleType));

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
    [SerializeField] private GameObject piecesLeftToSolvePuzzleText;
    [SerializeField] private TextMeshPro piecesLeftToSolvePuzzleTextText;
    #endregion UI

    #region Audio
    [SerializeField] private AudioSource gameStartAudio;
    [SerializeField] private AudioSource gameOverAudio;
    [SerializeField] private AudioSource pieceDownAudio;
    [SerializeField] private AudioSource piecePlacedAudio;
    [SerializeField] private AudioSource lineAudio;
    [SerializeField] private AudioSource gameBackgroundAudio;
    #endregion Audio

    [SerializeField] private Material floorMaterial;
    [SerializeField] private Material ceilingMaterial;

    [SerializeField] private Transform interactor;

    [SerializeField] private Transform indicator; 

    private Color[] colors = new Color[3] { Color.red, Color.green, Color.blue };

    [SerializeField] private bool useSample;

    //Puzzle
    GamePuzzle gamePuzzle;

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

        if(currentPiece != null)
        {
            if (currentPiece.seenByTheUser)
            {
                //Increase time
                time += Time.deltaTime;
                placeDownTime += Time.deltaTime;
            }
            else
            {
                Vector3 viewpos = indicator.GetComponent<CheckInViewPort>().IsObjectInViewPort();
                if (viewpos.x > 0 && viewpos.y > 0 && viewpos.z > 0)
                {
                    currentPiece.seenByTheUser = true;
                }
            }
        }

        //Game Play
        if (gameStarted)
        {
            //Setting the level 
            level = (lines / 4);

            //Piece initialization
            if ((piecePlacedDown && placeDownTime > 0.5f) || currentPiece == null)
            {
                if(currentPiece != null) FillBox(currentPiece.values, true);

                if (piecePlacedDown)
                {
                    piecePlacedAudio.Play();
                    piecesLeftUntilPuzzleExplodes--;
                    piecesLeftToSolvePuzzleTextText.text = piecesLeftUntilPuzzleExplodes.ToString() + " left to solve puzzle before explosion";

                    if (gamePuzzle != null)
                    {
                        int[,] indices;
                        switch (gamePuzzle.puzzleType)
                        {
                            case PuzzleType.FourXTwo:
                                indices = FourXTwo.CalculateInnerIndices(gamePuzzle.xPosition, gamePuzzle.yPosition);
                                break;
                            case PuzzleType.FiveXFive:
                                indices = FiveXFive.CalculateInnerIndices(gamePuzzle.xPosition, gamePuzzle.yPosition);
                                break;
                            case PuzzleType.ThreeXThree:
                                indices = ThreeXThree.CalculateInnerIndices(gamePuzzle.xPosition, gamePuzzle.yPosition);
                                break;
                            default:
                                indices = FiveXFive.CalculateInnerIndices(gamePuzzle.xPosition, gamePuzzle.yPosition);
                                break;
                        }

                        bool notFilled = false;
                        for (int i = 0; i < indices.GetLength(0); i++)
                        {
                            if (!CheckBox(indices[i,0], indices[i, 1])) notFilled = true;

                            if (notFilled)
                            {
                                break;
                            }
                        }
                        if (!notFilled)
                        {
                            gamePuzzle = null; 
                            score += 100;
                            lineAudio.Play();
                            piecesLeftUntilPuzzleExplodes = 0;
                            piecesLeftToSolvePuzzleText.SetActive(false);
                            
                            /*for (int b = indices[0, 1]; b < array.GetLength(1); b++)
                            {
                                for (int a = indices[0, 0]; a <= indices[indices.GetLength(0) - 1, 0]; a++)
                                {
                                    EmptyBox(a, b);
                                    array[a, b].DeActivatePuzzle();

                                    if (b >= indices[indices.GetLength(0) - 1, 1] && b != array.GetLength(1) - 1)
                                    {
                                        if (CheckBox(a, b + 1))
                                        {
                                            FillBox(a, b + 1 - 5, true, GetBoxColor(a, b + 1));
                                        }
                                    }
                                }
                            }*/

                            for (int i = 0; i < indices.GetLength(0); i++)
                            {
                                if (array[indices[i, 0], indices[i, 1]].IsPuzzleActive())
                                {
                                    int y = indices[i, 1];
                                    int count = 0; 
                                    while (y < yLength)
                                    {
                                        if (array[indices[i, 0], y].IsPuzzleActive())
                                        {
                                            EmptyBox(indices[i, 0], y);
                                            array[indices[i, 0], y].DeActivatePuzzle();
                                            count++; 
                                        }
                                        if (!array[indices[i, 0], y].IsPuzzleActive() && CheckBox(indices[i, 0], y))
                                        {
                                            FillBox(indices[i, 0], y - count, true, array[indices[i, 0],y].GetColor(), false);
                                            EmptyBox(indices[i, 0], y);
                                        }
                                        y++; 
                                    }
                                }
                            }
                        }
                    }

                    if(gamePuzzle != null && piecesLeftUntilPuzzleExplodes <= 0)
                    {
                        piecesLeftToSolvePuzzleText.SetActive(false);
                        GameOver();
                        return; 
                    }

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
                            lineAudio.Play();
                            for (int b = j; b < array.GetLength(1); b++)
                            {
                                for (int a = 0; a < array.GetLength(0); a++)
                                {
                                    EmptyBox(a, b);


                                    if(b != array.GetLength(1) - 1)
                                    {
                                        if(CheckBox(a, b + 1))
                                        {
                                            FillBox(a, b, true, GetBoxColor(a, b + 1), false);
                                        }
                                    }
                                }
                            }

                            j--;
                        }
                    }

                    score += 4;

                    if (noOfLinesCompleteThisRound > 0)
                        score += (noOfLinesCompleteThisRound + (2 * (noOfLinesCompleteThisRound - 1))) * 100;
                        //score += (noOfLinesCompleteThisRound + (2 * (noOfLinesCompleteThisRound - 1))) * (100 * (int)Mathf.Ceil(xLength / 40f));
                    loadingTextText.text = "Score " + score.ToString();
                    lines += noOfLinesCompleteThisRound;

                    if (((score == 50) || (score > 0 && score/puzzleLevel >= 1)) && gamePuzzle == null)
                    {
                        puzzleLevel = score + 200; 
                        gamePuzzle = new GamePuzzle();
                        Debug.Log( "Hari " + (xLength / 10f) + " " + (xLength / 10f <= 20));
                        piecesLeftUntilPuzzleExplodes = xLength/10f <= 20 ? 20 : xLength / 10f;
                        piecesLeftToSolvePuzzleText.SetActive(true);
                        piecesLeftToSolvePuzzleTextText.text = ((int)piecesLeftUntilPuzzleExplodes).ToString() + " left to solve puzzle before explosion";

                        if (puzzleCode > Enum.GetValues(typeof(PuzzleType)).Length - 1) puzzleCode = 0;

                        gamePuzzle.puzzleType = (PuzzleType)puzzleCode;
                        puzzleCode++;
                        gamePuzzle.xPosition = UnityEngine.Random.Range(0, xLength - 4);
                        gamePuzzle.yPosition = UnityEngine.Random.Range(3, 7);
                    }
                }

                int xPositionForNext = UnityEngine.Random.Range(0, array.GetLength(0));
                int yPositionForNext = array.GetLength(1);

                piecePlacedDown = false;

                float speed = (score / 5000f) > 0.35f ? 0.35f : (score / 5000f); 
                playTime = 0.75f - speed;
                currentMove = null;

                currentPiece = new GamePiece();
                currentPiece.pieceType = (PieceType)PieceTypeValues.GetValue(UnityEngine.Random.Range(0, 28));

                currentPiece.color = colors[UnityEngine.Random.Range(0, colors.Length)];

                currentPiece.yPosition = yPositionForNext;
                currentPiece.xPosition = xPositionForNext;

                indicator.gameObject.SetActive(true);
                array[xPositionForNext, yPositionForNext - 1].SetInteractorPosition(indicator);
                indicator.GetComponentInChildren<TextMeshPro>().text = GetPieceIndicator();
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
                    if(indicator.gameObject.activeInHierarchy) indicator.gameObject.SetActive(false); 
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

            if (stepReached && !piecePlacedDown)
            {
                bool placedDown;

                (placedDown, newPieceValues) = currentTetrisPiece.CalculateDownPosition(currentPiece.xPosition, currentPiece.yPosition, array.GetLength(0), array.GetLength(1));

                if (placedDown)
                {
                    piecePlacedDown = true;
                    placeDownTime = 0f; 
                }
            }

            if (currentMove != null && currentMove.moveType == MoveType.left)
            {
                piecePlacedDown = false;
                newPieceValues = currentTetrisPiece.CalculateLeftPosition(currentPiece.xPosition, currentPiece.yPosition, array.GetLength(0), array.GetLength(1));
            }

            if (currentMove != null && currentMove.moveType == MoveType.leftten)
            {
                piecePlacedDown = false;
                newPieceValues = currentTetrisPiece.CalculateLeftPosition(currentPiece.xPosition, currentPiece.yPosition, array.GetLength(0), array.GetLength(1));

                for (int i = 1; i < 9; i++)
                {
                    if (CheckBox(newPieceValues))
                    {
                        break; 
                    }

                    newPieceValues = currentTetrisPiece.CalculateLeftPosition(newPieceValues[0,0], newPieceValues[0,1], array.GetLength(0), array.GetLength(1));
                }
            }

            if (currentMove != null && currentMove.moveType == MoveType.right)
            {
                piecePlacedDown = false; 
                newPieceValues = currentTetrisPiece.CalculateRightPosition(currentPiece.xPosition, currentPiece.yPosition, array.GetLength(0), array.GetLength(1));
            }

            if (currentMove != null && currentMove.moveType == MoveType.rightten)
            {
                piecePlacedDown = false;
                newPieceValues = currentTetrisPiece.CalculateRightPosition(currentPiece.xPosition, currentPiece.yPosition, array.GetLength(0), array.GetLength(1));

                for (int i = 1; i < 9; i++)
                {
                    if (CheckBox(newPieceValues))
                    {
                        break;
                    }

                    newPieceValues = currentTetrisPiece.CalculateRightPosition(newPieceValues[0, 0], newPieceValues[0, 1], array.GetLength(0), array.GetLength(1));
                }
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
                    FillBox(newPieceValues, false);
                    currentPiece.xPosition = newPieceValues[0, 0];
                    currentPiece.yPosition = newPieceValues[0, 1];              
                    currentPiece.values = new int[4, 2];
                    Array.Copy(newPieceValues, currentPiece.values, newPieceValues.Length);
                    MoveInteractor();
                    pieceDownAudio.Play();
                }
                else
                {
                    if((newPieceValues[0, 1] == yLength - 1 || newPieceValues[1, 1] == yLength - 1 || newPieceValues[2, 1] == yLength - 1 || newPieceValues[3, 1] == yLength - 1) && currentMove == null)
                    {
                        GameOver();
                        return;
                    }
                    else if(currentMove != null)
                    {

                    }
                    else
                    {
                        piecePlacedDown = true;
                        placeDownTime = 0f; 
                    }
                }
            }

            currentMove = null;

            if(gamePuzzle != null)
            {
                if (!gamePuzzle.activated)
                {
                    gamePuzzle.activated = true; 
                    int[,] indices;
                    switch (gamePuzzle.puzzleType)
                    {
                        case PuzzleType.FourXTwo:
                            indices = FourXTwo.CalculateInnerIndices(gamePuzzle.xPosition, gamePuzzle.yPosition);
                            break;
                        case PuzzleType.FiveXFive:
                            indices = FiveXFive.CalculateInnerIndices(gamePuzzle.xPosition, gamePuzzle.yPosition);
                            break;
                        case PuzzleType.ThreeXThree:
                            indices = ThreeXThree.CalculateInnerIndices(gamePuzzle.xPosition, gamePuzzle.yPosition);
                            break;
                        default:
                            indices = FiveXFive.CalculateInnerIndices(gamePuzzle.xPosition, gamePuzzle.yPosition);
                            break;
                    }

                    int indicesXLength = indices.GetLength(0);
                    for (int i = 0; i < indicesXLength; i++)
                    {
                        array[indices[i, 0], indices[i, 1]].ActivatePuzzle();
                    }
                }
            }
        }

        //Localtesting not in vr - Initialise the bricks in the wall 
        if (!initialised && useSample)
        {
            time = 0f;

            initialised = true;

            gameWall.CreateBricksSample(out array);

            xLength = array.GetLength(0);
            yLength = array.GetLength(1);
            Debug.Log(xLength);

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

            if(xLength < 40)
            {
                loadingTextText.text = "Not enough size wall length, Sorry";
                return; 
            }

            if (xLength > 500)
            {
                loadingTextText.text = "Very high wall length, Sorry";
                return;
            }

            if (yLength < 20)
            {
                loadingTextText.text = "Not enough wall height, Sorry";
                return;
            }

            loadingText.gameObject.SetActive(false);
            playButton.gameObject.SetActive(true);
            highScoresText.gameObject.SetActive(true);
            userUI.SetActive(true);

            gameWall.GetFloor().GetComponent<MeshRenderer>().material = floorMaterial;

            // Create a game object with a MeshFilter and MeshRenderer to display the mesh
            GameObject polygonMesh = new GameObject("PolygonMesh");
            MeshFilter meshFilter = polygonMesh.AddComponent<MeshFilter>();
            MeshRenderer meshRenderer = polygonMesh.AddComponent<MeshRenderer>();
            meshFilter.mesh = gameWall.GetFloor().GetComponent<MeshFilter>().mesh;
            polygonMesh.transform.position = new Vector3(transform.position.x, gameWall.GetWallHeight(), transform.position.z);
            meshRenderer.material = ceilingMaterial;

            CheckAndLoadGameIfSaved();
        }
    }

    private void MoveInteractor()
    {
        SetInteractorPosition(currentPiece.values[0, 0], currentPiece.values[0, 1]);
    }

    //Get centre of each tetris piece type 
    private string GetPieceIndicator()
    {
        string i = "I";
        switch (currentPiece.pieceType)
        {
            case PieceType.IBlock1:
            case PieceType.IBlock2:
            case PieceType.IBlock3:
            case PieceType.IBlock4:
                i = "I";
                break;
            case PieceType.OBlock1:
            case PieceType.OBlock2:
            case PieceType.OBlock3:
            case PieceType.OBlock4:
                i = "O";
                break;
            case PieceType.TBlock1:
            case PieceType.TBlock2:
            case PieceType.TBlock3:
            case PieceType.TBlock4:
                i = "T";
                break;
            case PieceType.ZBlock1:
            case PieceType.ZBlock2:
            case PieceType.ZBlock3:
            case PieceType.ZBlock4:
                i = "Z";
                break;
            case PieceType.SBlock1:
            case PieceType.SBlock2:
            case PieceType.SBlock3:
            case PieceType.SBlock4:
                i = "S";
                break;
            case PieceType.LBlock1:
            case PieceType.LBlock2:
            case PieceType.LBlock3:
            case PieceType.LBlock4:
                i = "L";
                break;
            case PieceType.JBlock1:
            case PieceType.JBlock2:
            case PieceType.JBlock3:
            case PieceType.JBlock4:
                i = "J"; 
                break;

        }

        return i;
    }

    private void GameOver()
    {
        gameOverAudio.Play();
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
        /*savedGame = await saveLoadHandler.LoadGameAsync();
        if (savedGame != null && 
            savedGame.gameBlockProperties.GetLength(0) == array.GetLength(0) &&
            savedGame.gameBlockProperties.GetLength(1) == array.GetLength(1))
        {
            resumeGameButton.SetActive(true);
        }*/
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

    public void FillBox(int[,] values, bool lockIt)
    {
        for (int i = 0; i < values.GetLength(0); i++)
        {
            bool centre = i == 0 ? true : false; 
            if (values[i, 0] != -1 && values[i, 1] != -1)
                FillBox(values[i, 0], values[i, 1], lockIt, currentPiece.color, centre);
        }
    }

    private void FillBox(int x, int y, bool lockIt, Color color, bool centre)
    {
        array[x, y].FillBox(color, lockIt, centre);
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

    private Color GetBoxColor(int x, int y)
    {
        return array[x, y].GetColor();
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
        piecesLeftUntilPuzzleExplodes = 0; 

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

        piecesLeftToSolvePuzzleText.SetActive(false);

        interactor.gameObject.SetActive(true);

        gameStartAudio.Play(); 
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
        piecesLeftToSolvePuzzleText.SetActive(false);

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
            case "leftten":
                moveType = MoveType.leftten;
                break;
            case "right":
                moveType = MoveType.right;
                break;
            case "rightten":
                moveType = MoveType.rightten;
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