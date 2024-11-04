using System;

public class Grid
{
    private readonly int size;
    private readonly StoneType[,] board;

    public Grid(int size)
    {
        this.size = size;
        board = new StoneType[size, size];
        ResetBoard();
    }

    public void ResetBoard()
    {
        Array.Clear(board, 0, board.Length);
    }

    public bool IsPositionValid(int x, int y)
    {
        return x >= 0 && y >= 0 && x < size && y < size;
    }

    public StoneType GetStoneAt(int x, int y)
    {
        return IsPositionValid(x, y) ? board[x, y] : StoneType.None;
    }

    public bool PlaceStone(int x, int y, StoneType stone)
    {
        if (!IsPositionValid(x, y) || board[x, y] != StoneType.None)
        {
            return false;
        }

        board[x, y] = stone;
        return true;
    }

    public int GetSize() => size;

    public void SetStone(int x, int y, StoneType stone)
    {
        board[x, y] = stone;
    }

    // 현재 Grid의 상태를 복사하여 새로운 Grid 객체를 생성하는 메서드
    public Grid Clone()
    {
        Grid clone = new Grid(size);
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                clone.SetStone(x, y, board[x, y]);
            }
        }
        return clone;
    }

    public StoneType[,] GetBoard()
    {
        return board;
    }

    // 새로운 보드 상태를 설정하는 메서드 (리플레이 기능을 위한 메서드)
    public void SetBoard(StoneType[,] newBoard)
    {
        if (newBoard.GetLength(0) == size && newBoard.GetLength(1) == size)
        {
            Array.Copy(newBoard, board, newBoard.Length);
        }
        else
        {
            throw new ArgumentException("The provided board size does not match the grid size.");
        }
    }
}
