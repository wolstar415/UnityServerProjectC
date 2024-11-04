using UnityEngine;

public class Player
{
    public StoneType Stone { get; }
    public bool IsAIPlayer { get; }
    protected AIRobot aiRobot;

    public Player(StoneType stone, bool isAI = false)
    {
        Stone = stone;
        IsAIPlayer = isAI;

        if (IsAIPlayer)
        {
            aiRobot = new AIRobot(this);
        }
    }

    public bool MakeMove(Grid grid, int x, int y)
    {
        if (IsForbiddenMove(grid, x, y))
        {
            Debug.Log("Forbidden move! You cannot place a stone here.");
            return false;
        }

        if (!grid.PlaceStone(x, y, Stone))
        {
            return false; // 돌을 놓을 수 없는 위치
        }

        if (CheckForWin(grid, x, y))
        {
            Debug.Log($"{Stone} wins!");
        }

        return true;
    }

    public Vector2Int MakeAIMove(Grid grid, StoneType opponentStone)
    {
        if (IsAIPlayer)
        {
            return aiRobot.FindBestMove(grid);
        }
        return new Vector2Int(-1, -1); // AI가 아닌 경우 비정상 값 반환
    }

    public virtual bool IsForbiddenMove(Grid grid, int x, int y)
    {
        return false; // 기본적으로 금수 체크는 하지 않음
    }

    public bool CheckForWin(Grid grid, int x, int y)
    {
        return CheckDirection(grid, x, y, 1, 0) >= 5 ||  // 가로 체크
               CheckDirection(grid, x, y, 0, 1) >= 5 ||  // 세로 체크
               CheckDirection(grid, x, y, 1, 1) >= 5 ||  // 대각선 (\) 체크
               CheckDirection(grid, x, y, 1, -1) >= 5;   // 대각선 (/) 체크
    }

    private int CheckDirection(Grid grid, int x, int y, int dx, int dy)
    {
        int count = 1;
        StoneType stone = grid.GetStoneAt(x, y);

        count += CountStonesInDirection(grid, x, y, dx, dy, stone);
        count += CountStonesInDirection(grid, x, y, -dx, -dy, stone);

        return count;
    }

    protected int CountStonesInDirection(Grid grid, int x, int y, int dx, int dy, StoneType stone)
    {
        int count = 0;
        int nx = x + dx;
        int ny = y + dy;

        while (grid.IsPositionValid(nx, ny) && grid.GetStoneAt(nx, ny) == stone)
        {
            count++;
            nx += dx;
            ny += dy;
        }

        return count;
    }


    public bool IsBlackCheck(Grid grid, int x, int y)
    {
        // 제공해주신 금수 체크 로직
        if (grid.GetStoneAt(x, y) != StoneType.None)
        {
            return false; // 이미 돌이 있는 경우 금수가 아님
        }

        int openThrees = 0, openFours = 0;

        // 3-3 금수 체크
        openThrees += CountOpenPatterns(grid, x, y, 1, 0, 3);
        openThrees += CountOpenPatterns(grid, x, y, 0, 1, 3);
        openThrees += CountOpenPatterns(grid, x, y, 1, 1, 3);
        openThrees += CountOpenPatterns(grid, x, y, 1, -1, 3);

        if (openThrees >= 2)
        {
            return true; // 3-3 금수
        }

        // 4-4 금수 체크
        openFours += CountOpenPatterns(grid, x, y, 1, 0, 4);
        openFours += CountOpenPatterns(grid, x, y, 0, 1, 4);
        openFours += CountOpenPatterns(grid, x, y, 1, 1, 4);
        openFours += CountOpenPatterns(grid, x, y, 1, -1, 4);

        if (openFours >= 2)
        {
            return true; // 4-4 금수
        }

        // 장목 금수 체크
        if (CheckDirectionWithoutPlacing(grid, x, y, 1, 0) >= 6 ||
            CheckDirectionWithoutPlacing(grid, x, y, 0, 1) >= 6 ||
            CheckDirectionWithoutPlacing(grid, x, y, 1, 1) >= 6 ||
            CheckDirectionWithoutPlacing(grid, x, y, 1, -1) >= 6)
        {
            return true; // 장목 금수
        }

        return false; // 금수가 아닌 경우
    }

    private int CheckDirectionWithoutPlacing(Grid grid, int x, int y, int dx, int dy)
    {
        int count = 1;

        count += CountStonesInDirection(grid, x, y, dx, dy, StoneType.Black);
        count += CountStonesInDirection(grid, x, y, -dx, -dy, StoneType.Black);

        return count;
    }

    private int CountOpenPatterns(Grid grid, int x, int y, int dx, int dy, int length)
    {
        int count = 0;

        int consecutiveStones = CountStonesInDirection(grid, x, y, dx, dy, StoneType.Black);
        int reverseStones = CountStonesInDirection(grid, x, y, -dx, -dy, StoneType.Black);

        if (consecutiveStones + reverseStones + 1 == length)
        {
            bool isOpen = IsOpenEnd(grid, x + dx * consecutiveStones, y + dy * consecutiveStones, dx, dy) &&
                          IsOpenEnd(grid, x - dx * reverseStones, y - dy * reverseStones, -dx, -dy);

            if (isOpen)
            {
                count++;
            }
        }

        return count;
    }

    private bool IsOpenEnd(Grid grid, int x, int y, int dx, int dy)
    {
        int nx = x + dx;
        int ny = y + dy;

        return grid.IsPositionValid(nx, ny) && grid.GetStoneAt(nx, ny) == StoneType.None;
    }
}
