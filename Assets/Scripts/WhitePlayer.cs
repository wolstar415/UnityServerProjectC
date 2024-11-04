public class WhitePlayer : Player
{
    public WhitePlayer() : base(StoneType.White)
    {
    }

    // 백돌은 금수 규칙이 없으므로, 기본 Player 로직을 그대로 사용
}