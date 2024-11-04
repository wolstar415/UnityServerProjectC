using UnityEngine;
public class BlackPlayer : Player
{
    public BlackPlayer(bool isAI = false) : base(StoneType.Black, isAI) { }

    public override bool IsForbiddenMove(Grid grid, int x, int y)
    {
        return IsBlackCheck(grid, x, y);
    }

}
