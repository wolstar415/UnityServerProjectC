using UnityEngine;
using static UnityEditor.Experimental.GraphView.GraphView;

public class AIRobot
{
    private Player player;
    private StoneType aiStone;
    private StoneType opponentStone;
    private System.Random random = new System.Random();

    public AIRobot(Player player)
    {
        this.player = player;
        this.aiStone = player.Stone;
        this.opponentStone = aiStone == StoneType.Black ? StoneType.White : StoneType.Black;
    }

    public Vector2Int FindBestMove(Grid grid)
    {
        // 블랙이 첫 수를 둘 경우, 중앙 근처에 랜덤으로 돌을 두기
        if (aiStone == StoneType.Black && IsFirstMove(grid))
        {
            return GetRandomCentralMove(grid);
        }

        int bestValue = int.MinValue;
        Vector2Int bestMove = new Vector2Int(-1, -1);

        for (int y = 0; y < grid.GetSize(); y++)
        {
            for (int x = 0; x < grid.GetSize(); x++)
            {
                if (grid.GetStoneAt(x, y) == StoneType.None)
                {
                    // 금수 체크
                    if (player.IsForbiddenMove(grid, x, y))
                    {
                        continue;
                    }

                    // 이 위치에 돌을 두었을 때의 점수를 계산
                    int moveValue = EvaluateMove(grid, x, y);

                    if (moveValue > bestValue)
                    {
                        bestMove = new Vector2Int(x, y);
                        bestValue = moveValue;
                    }
                }
            }
        }

        return bestMove;
    }

    private bool IsFirstMove(Grid grid)
    {
        // 보드가 비어있는지 확인
        for (int y = 0; y < grid.GetSize(); y++)
        {
            for (int x = 0; x < grid.GetSize(); x++)
            {
                if (grid.GetStoneAt(x, y) != StoneType.None)
                {
                    return false;
                }
            }
        }
        return true;
    }

    private Vector2Int GetRandomCentralMove(Grid grid)
    {
        int centerX = grid.GetSize() / 2;
        int centerY = grid.GetSize() / 2;

        // 중앙 근처의 위치에서 랜덤하게 선택
        int offsetX = random.Next(-1, 2); // -1, 0, 1 중 하나를 선택
        int offsetY = random.Next(-1, 2);

        int x = Mathf.Clamp(centerX + offsetX, 0, grid.GetSize() - 1);
        int y = Mathf.Clamp(centerY + offsetY, 0, grid.GetSize() - 1);

        return new Vector2Int(x, y);
    }

    private int EvaluateMove(Grid grid, int x, int y)
    {
        int score = 0;

        // AI가 이길 수 있는지를 확인합니다.
        if (CheckIfWinningMove(grid, x, y, aiStone))
        {
            return int.MaxValue; // 즉시 이길 수 있는 자리는 최고 점수
        }

        // 상대방이 이길 수 있는지를 확인합니다.
        if (CheckIfWinningMove(grid, x, y, opponentStone))
        {
            return int.MaxValue - 1; // 상대방의 승리를 막는 것은 거의 최고 점수
        }

        // 가로, 세로, 대각선 방향으로 점수 계산
        score = EvaluateDirection(grid, x, y, 1, 0, aiStone);   // 가로
        score = Mathf.Max(score, EvaluateDirection(grid, x, y, 0, 1, aiStone));   // 세로
        score = Mathf.Max(score, EvaluateDirection(grid, x, y, 1, 1, aiStone));   // 대각선 (\)
        score = Mathf.Max(score, EvaluateDirection(grid, x, y, 1, -1, aiStone));  // 대각선 (/)

        // 상대방의 돌을 막는 것도 고려
        score = Mathf.Max(score, EvaluateDirection(grid, x, y, 1, 0, opponentStone));   // 가로
        score = Mathf.Max(score, EvaluateDirection(grid, x, y, 0, 1, opponentStone));   // 세로
        score = Mathf.Max(score, EvaluateDirection(grid, x, y, 1, 1, opponentStone));   // 대각선 (\)
        score = Mathf.Max(score, EvaluateDirection(grid, x, y, 1, -1, opponentStone));  // 대각선 (/)

        // 막히지 않은 33, 44, 34 패턴에 대한 추가 점수
        score = Mathf.Max(score, CheckAdvancedPatterns(grid, x, y, opponentStone));

        return score;
    }
    private bool CheckIfWinningMove(Grid grid, int x, int y, StoneType stone)
    {
        return CheckDirection(grid, x, y, 1, 0, stone) >= 5 ||  // 가로 체크
               CheckDirection(grid, x, y, 0, 1, stone) >= 5 ||  // 세로 체크
               CheckDirection(grid, x, y, 1, 1, stone) >= 5 ||  // 대각선 (\) 체크
               CheckDirection(grid, x, y, 1, -1, stone) >= 5;   // 대각선 (/) 체크
    }

    private int CheckDirection(Grid grid, int x, int y, int dx, int dy, StoneType stone)
    {
        int count = 1;

        // 앞으로 체크
        int nx = x + dx;
        int ny = y + dy;
        while (grid.IsPositionValid(nx, ny) && grid.GetStoneAt(nx, ny) == stone)
        {
            count++;
            nx += dx;
            ny += dy;
        }

        // 뒤로 체크
        nx = x - dx;
        ny = y - dy;
        while (grid.IsPositionValid(nx, ny) && grid.GetStoneAt(nx, ny) == stone)
        {
            count++;
            nx -= dx;
            ny -= dy;
        }

        return count;
    }
    private int EvaluateDirection(Grid grid, int x, int y, int dx, int dy, StoneType stone)
    {
        if (stone == StoneType.Black && player.IsBlackCheck(grid, x, y))
            return 0; // 금수인 경우 점수 0 반환

        int score = 0;
        int consecutive = 0;
        int openEnds = 0;

        // 앞으로 체크
        for (int i = 1; i <= 5; i++)
        {
            int nx = x + i * dx;
            int ny = y + i * dy;

            if (!grid.IsPositionValid(nx, ny))
                break;

            if (grid.GetStoneAt(nx, ny) == stone)
            {
                consecutive++;
            }
            else if (grid.GetStoneAt(nx, ny) == StoneType.None)
            {
                openEnds++;
                break;
            }
            else
            {
                break;
            }
        }

        score = GetScore(consecutive, openEnds);

        consecutive = 0;
        openEnds = 0;

        // 뒤로 체크
        for (int i = 1; i < 5; i++)
        {
            int nx = x - i * dx;
            int ny = y - i * dy;

            if (!grid.IsPositionValid(nx, ny))
                break;

            if (grid.GetStoneAt(nx, ny) == stone)
            {
                consecutive++;
            }
            else if (grid.GetStoneAt(nx, ny) == StoneType.None)
            {
                openEnds++;
                break;
            }
            else
            {
                break;
            }
        }

        score = Mathf.Max(score, GetScore(consecutive, openEnds));

        return score;
    }

    private int GetScore(int consecutive, int openEnds)
    {
        // 점수 계산
        if (consecutive >= 5)
        {
            return 10000;
        }
        else if (consecutive == 4 && openEnds > 0) // 열린 4
        {
            return 6;
        }
        else if (consecutive == 4 && openEnds == 0) // 막힌 4도 우선 처리
        {
            return 5;
        }
        else if (consecutive == 3 && openEnds == 2) // 양쪽이 열린 3
        {
            return 4;
        }
        else if (consecutive == 3 && openEnds == 1) // 한 쪽이 막힌 3
        {
            return 3;
        }
        else if (consecutive == 2 && openEnds > 0)
        {
            return 2;
        }
        else if (consecutive == 1 && openEnds > 0)
        {
            return 1;
        }

        return 0;
    }

    private int CheckAdvancedPatterns(Grid grid, int x, int y, StoneType stone)
    {
        int score = 0;

        // 33, 44, 34 패턴 체크
        int openThrees = 0;
        int openFours = 0;

        openThrees += CountOpenPatterns(grid, x, y, 1, 0, 3);
        openThrees += CountOpenPatterns(grid, x, y, 0, 1, 3);
        openThrees += CountOpenPatterns(grid, x, y, 1, 1, 3);
        openThrees += CountOpenPatterns(grid, x, y, 1, -1, 3);

        openFours += CountOpenPatterns(grid, x, y, 1, 0, 4);
        openFours += CountOpenPatterns(grid, x, y, 0, 1, 4);
        openFours += CountOpenPatterns(grid, x, y, 1, 1, 4);
        openFours += CountOpenPatterns(grid, x, y, 1, -1, 4);

        if (openThrees >= 2) // 33 패턴
        {
            score += 7;
        }
        if (openFours >= 2) // 44 패턴
        {
            score += 8;
        }
        if (openThrees >= 1 && openFours >= 1) // 34 패턴
        {
            score += 9;
        }

        return score;
    }

    private int CountOpenPatterns(Grid grid, int x, int y, int dx, int dy, int length)
    {
        int count = 0;

        int consecutiveStones = CountStonesInDirection(grid, x, y, dx, dy, aiStone);
        int reverseStones = CountStonesInDirection(grid, x, y, -dx, -dy, aiStone);

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

    private int CountStonesInDirection(Grid grid, int x, int y, int dx, int dy, StoneType stone)
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
}
