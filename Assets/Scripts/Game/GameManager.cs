using UnityEngine;
using System.Collections.Generic;
using UnityEditor;

public class GameManager : MonoBehaviour
{
    private Grid grid;
    private Player player1;
    private Player player2;
    private Player currentPlayer;

    public int gridSize = 19;
    public int currentMoveNumber { get; private set; }
    private bool isGamePaused = false;
    private bool isGameOver = false; // 게임 종료 상태를 나타내는 변수

    private List<MoveRecord> moveHistory; // 각 턴의 움직임을 기록하는 리스트
    public int MaxHisyory => moveHistory.Count; // 각 턴의 움직임을 기록하는 리스트

    public StoneType[,] BoardState => grid == null ? null : grid.GetBoard();

    void Start()
    {
        InitializeGame(false, true); // 예: player1이 흑돌 AI, player2가 백돌 인간
    }

    public void InitializeGame(bool isPlayer1AI, bool isPlayer2AI)
    {
        grid = new Grid(gridSize);
        currentMoveNumber = 1;
        isGameOver = false; // 게임을 초기화할 때 종료 상태를 false로 설정
        moveHistory = new List<MoveRecord>();

        player1 = isPlayer1AI ? new BlackPlayer(isAI: true) : new BlackPlayer(isAI: false);
        player2 = isPlayer2AI ? new Player(StoneType.White, isAI: true) : new Player(StoneType.White, isAI: false);

        currentPlayer = player1; // 흑돌부터 시작
        if (currentPlayer.IsAIPlayer)
        {
            MakeAIMove(); // AI가 첫 턴일 경우 바로 실행
        }
    }

    public bool OnBoardClick(int x, int y)
    {
        if (isGamePaused || isGameOver) return false; // 게임이 일시 중지 또는 종료 상태에서는 돌을 놓을 수 없음

        if (!currentPlayer.IsAIPlayer && currentPlayer.MakeMove(grid, x, y))
        {
            RecordMove(x, y, currentPlayer.Stone);
            if (currentPlayer.CheckForWin(grid, x, y))
            {
                isGameOver = true; // 게임이 끝났음을 설정
                DisplayWinMessage(currentPlayer.Stone);
            }
            else
            {
                SwitchTurn();
            }
            return true;
        }
        return false;
    }

    private void SwitchTurn()
    {
        currentPlayer = currentPlayer == player1 ? player2 : player1;
        currentMoveNumber++;

        if (currentPlayer.IsAIPlayer)
        {
            MakeAIMove(); // AI의 턴이면 수를 두게 함
        }
    }

    private void MakeAIMove()
    {
        if (isGameOver) return; // 게임이 종료된 경우 AI는 더 이상 수를 두지 않음

        Vector2Int aiMove = currentPlayer.MakeAIMove(grid, player1.Stone == currentPlayer.Stone ? player2.Stone : player1.Stone);

        if (aiMove != new Vector2Int(-1, -1))
        {
            currentPlayer.MakeMove(grid, aiMove.x, aiMove.y);
            RecordMove(aiMove.x, aiMove.y, currentPlayer.Stone);

            if (currentPlayer.CheckForWin(grid, aiMove.x, aiMove.y))
            {
                isGameOver = true; // 게임이 끝났음을 설정
                DisplayWinMessage(currentPlayer.Stone);
                return;
            }
        }

        SwitchTurn(); // AI가 수를 두었으면 턴을 넘김
    }

    private void DisplayWinMessage(StoneType winningStone)
    {
        string message = $"{winningStone} wins the game!";
        Debug.Log(message);

        // 게임 종료 후 결과창 표시
        EditorUtility.DisplayDialog("Game Over", message, "OK");

        // 추가적인 게임 종료 처리 로직 (예: 게임을 리셋하거나 종료할지 물어보기 등)
    }

    public void ResetGame()
    {
        InitializeGame(player1.IsAIPlayer, player2.IsAIPlayer); // 게임을 초기화합니다.
    }

    public StoneType GetStoneAt(int x, int y)
    {
        return grid?.GetStoneAt(x, y) ?? StoneType.None;
    }

    public StoneType GetCurrentPlayerStone()
    {
        return currentPlayer.Stone;
    }

    public bool IsForbiddenMove(int x, int y)
    {
        return currentPlayer.IsForbiddenMove(grid, x, y);
    }

    private void RecordMove(int x, int y, StoneType stone)
    {
        moveHistory.Add(new MoveRecord(x, y, stone));
    }

    public void LoadState(int moveNumber)
    {
        grid.ResetBoard();
        moveNumber -= 1;
        for (int i = 0; i <= moveNumber; i++)
        {
            var move = moveHistory[i];
            grid.SetStone(move.X, move.Y, move.Stone);
        }
        currentMoveNumber = moveNumber + 1;
    }

    public void SetPause(bool pause)
    {
        isGamePaused = pause;
    }

    public bool IsGameOver()
    {
        return isGameOver;
    }

    public bool IsGamePaused => isGamePaused;

    private struct MoveRecord
    {
        public int X;
        public int Y;
        public StoneType Stone;

        public MoveRecord(int x, int y, StoneType stone)
        {
            X = x;
            Y = y;
            Stone = stone;
        }
    }
}
