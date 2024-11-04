using UnityEditor;
using UnityEngine;

public class OmokWindow : EditorWindow
{
    private GameManager gameManager;
    private bool isAIMode = true;
    private bool isPlayer1AI = true;
    private bool isGameStarted = false;
    private bool isReplaying = false;
    private int replayMoveNumber = 0;
    private GUIStyle blackStoneStyle;
    private GUIStyle whiteStoneStyle;
    private GUIStyle emptyStyle;
    private GUIStyle forbiddenStyle;
    private bool stylesInitialized = false;

    [MenuItem("Window/Omok Game")]
    public static void ShowWindow()
    {
        GetWindow<OmokWindow>("Omok Game");
    }

    private void OnEnable()
    {
        GameObject gameManagerObject = GameObject.Find("GameManager");
        if (gameManagerObject != null)
        {
            gameManager = gameManagerObject.GetComponent<GameManager>();
        }

        if (gameManager == null)
        {
            Debug.LogError("No GameManager found in the scene with the name 'GameManager'. Please ensure that the GameManager object exists.");
        }
    }

    private void InitializeStyles()
    {
        blackStoneStyle = new GUIStyle(GUI.skin.button)
        {
            normal = { background = MakeTex(2, 2, Color.black) },
            border = new RectOffset(0, 0, 0, 0)
        };

        whiteStoneStyle = new GUIStyle(GUI.skin.button)
        {
            normal = { background = MakeTex(2, 2, Color.white) },
            border = new RectOffset(0, 0, 0, 0)
        };

        emptyStyle = new GUIStyle(GUI.skin.button)
        {
            normal = { background = MakeTex(2, 2, new Color(0.9f, 0.8f, 0.7f)) },
            border = new RectOffset(0, 0, 0, 0)
        };

        forbiddenStyle = new GUIStyle(GUI.skin.button)
        {
            normal = { background = MakeTex(2, 2, new Color(0.8f, 0.3f, 0.3f)), textColor = Color.white },
            border = new RectOffset(0, 0, 0, 0)
        };

        stylesInitialized = true;
    }

    private Texture2D MakeTex(int width, int height, Color col)
    {
        Color[] pix = new Color[width * height];
        for (int i = 0; i < pix.Length; i++)
            pix[i] = col;

        Texture2D result = new Texture2D(width, height);
        result.SetPixels(pix);
        result.Apply();
        return result;
    }

    private void OnGUI()
    {
        if (!stylesInitialized)
        {
            InitializeStyles(); // 스타일 초기화
        }

        if (gameManager == null)
        {
            GUILayout.Label("No GameManager found in the scene.");
            return;
        }

        // 게임 시작 전 설정
        if (!isGameStarted)
        {
            GUILayout.Label("Select Game Mode:");
            isAIMode = GUILayout.Toggle(isAIMode, "Play against AI");

            if (isAIMode)
            {
                GUILayout.Label("AI Plays as:");
                isPlayer1AI = GUILayout.Toggle(isPlayer1AI, "Black (AI goes first)");
            }

            if (GUILayout.Button("Start Game"))
            {
                StartGame();
            }
        }
        else
        {
            GUILayout.Label($"Current Player: {gameManager.GetCurrentPlayerStone()}");
            GUILayout.Label($"Move Number: {gameManager.currentMoveNumber}");

            if (GUILayout.Button("Reset Game"))
            {
                ResetGame();
            }

            if (GUILayout.Button(isReplaying ? "Resume Game" : "Replay"))
            {
                ToggleReplayMode();
            }
            if (isReplaying)
            {
                int newReplayMoveNumber = replayMoveNumber;
                newReplayMoveNumber = EditorGUILayout.IntSlider("Replay Move:", replayMoveNumber, 0, gameManager.MaxHisyory);
                if (newReplayMoveNumber != replayMoveNumber)
                {
                    replayMoveNumber = newReplayMoveNumber;
                    gameManager.LoadState(replayMoveNumber);
                    Repaint();
                }
            }


            DrawBoard();
        }
    }

    private void StartGame()
    {
        isReplaying = false; // 게임 시작 시 리플레이 모드 비활성화
        gameManager.SetPause(false); // 게임 시작 시 게임을 활성화

        if (isAIMode)
        {
            gameManager.InitializeGame(isPlayer1AI, !isPlayer1AI);
        }
        else
        {
            gameManager.InitializeGame(false, false); // 플레이어 대 플레이어 모드
        }
        isGameStarted = true;
    }

    private void ResetGame()
    {
        isGameStarted = false;
        isReplaying = false;
        gameManager.ResetGame();
    }

    private void ToggleReplayMode()
    {
        isReplaying = !isReplaying;
        replayMoveNumber = gameManager.MaxHisyory;
        gameManager.LoadState(replayMoveNumber);

        gameManager.SetPause(isReplaying);
    }

    private void DrawBoard()
    {
        int gridSize = gameManager.gridSize;
        StoneType[,] boardState = gameManager.BoardState;

        for (int y = 0; y < gridSize; y++)
        {
            GUILayout.BeginHorizontal();
            for (int x = 0; x < gridSize; x++)
            {
                StoneType stone = boardState[x, y];
                GUIStyle style;

                if (stone == StoneType.None)
                {
                    if (gameManager.IsGameOver() == false && gameManager.IsForbiddenMove(x, y))
                    {
                        style = forbiddenStyle; // 금수 위치에 스타일 적용
                        if (GUILayout.Button("X", style, GUILayout.Width(25), GUILayout.Height(25)))
                        {
                            if (!isReplaying && gameManager.OnBoardClick(x, y))
                            {
                                Repaint(); // 상태 변경 후 UI를 새로 고침
                            }
                        }
                    }
                    else
                    {
                        style = emptyStyle; // 빈 칸 스타일
                        if (GUILayout.Button("", style, GUILayout.Width(25), GUILayout.Height(25)))
                        {
                            if (!isReplaying && gameManager.OnBoardClick(x, y))
                            {
                                Repaint(); // 상태 변경 후 UI를 새로 고침
                            }
                        }
                    }
                }
                else
                {
                    style = stone == StoneType.Black ? blackStoneStyle : whiteStoneStyle;
                    if (GUILayout.Button("", style, GUILayout.Width(25), GUILayout.Height(25)))
                    {
                        if (!isReplaying && gameManager.OnBoardClick(x, y))
                        {
                            Repaint(); // 상태 변경 후 UI를 새로 고침
                        }
                    }
                }
            }
            GUILayout.EndHorizontal();
        }
    }
}
