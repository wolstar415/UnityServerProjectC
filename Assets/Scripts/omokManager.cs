using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum ePlayType
{
    None,
    BLACK,
    WHITE
}

public class omokManager : MonoBehaviour
{
    public GameObject ob;
    public GameObject ob2;
    public static omokManager inst;

    public Sprite[] dollImg; // 이미지 입니다. 0이 흑돌 이미지,1 흰돌 이미지

    public ePlayType myPlayType; // 자신이 흰색인지 흑돌인지 
    public bool IsPlay = false; // 플레이중

    public readonly int SIZE = 15; // 바둑판 사이즈
    public bool IsBlackTurn = false; // 턴

    public List<GameObject> noObs;
    //X표시할 때 담았다가 지울 때 리스트안에 있는 X자를 다 지우면 되니까 

    public int[,] ball = new int[,]
        //좌표 1 흑돌 ,2 흰돌 , 3 X자 표시
        {
            { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
            { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
            { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
            { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
            { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
            { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
            { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
            { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
            { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
            { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
            { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
            { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
            { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
            { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
            { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
        };

    public omokClick[] ballOb; //돌을 담습니다. 미리 담습니다 15x15 225개

    public omokClick ballScan(int row, int column)
        //해당 돌 GameObject를 찾는 함수
    {
        return ballOb[row + (column * SIZE)];
    }

    private void Awake()
    {
        inst = this;
    }

    public void OnGameEnd(bool black,string roomName)
    {
        string blackName = roomName.Split(',')[0];
        string whiteName = roomName.Split(',')[1];

        if (black)
        {
            Debug.Log($"{blackName}님께서 승리 했습니다.");
        }
        else
        {
            Debug.Log($"{whiteName}님께서 승리 했습니다.");
        }

        


        IsPlay = false;
        myPlayType = ePlayType.None;


        ob.SetActive(false);
        ob2.SetActive(true);
    }

    public void OnTurn()
    {
        //int row = data.GetValue(1).GetInt32();
        //int column = data.GetValue(2).GetInt32();

        //if (data.GetValue(0).GetString() == "Black")
        //{
        //    ball[column, row] = 1;
        //    ballScan(row, column).GetComponent<omokClick>().ballClick(1);
        //}
        //else
        //{
        //    ball[column, row] = 2;
        //    ballScan(row, column).GetComponent<omokClick>().ballClick(2);
        //}

        //GameManager.inst.chatManager.gameInfo.text = $"{data.GetValue(3).GetString()}님의 차례입니다.";
        //TurnChange();
    }

    public void OnGameStart(bool isBlack)
    {
        GameStart(isBlack);
        ob.SetActive(true);
        ob2.SetActive(false);
    }

    public void StartBtn()
        //게임시작 버튼을 누르면 발생하는 함수
    {
        //if (GameManager.inst.Player1 == GameManager.inst.nickName && GameManager.inst.Player2 != "" && IsPlay == false)
        //{
        //    GameManager.inst.chatManager.startBtn.SetActive(false);
        //    //버튼을 숨깁니다.

        //    int ran = Random.Range(0, 2);
        //    //0이 나오면 1번이 흑 2번이 백
        //    //1이 나오면 1번이 백 2번이 흑
        //    SocketManager.inst.socket.Emit("GameStart", ran);
        //}
    }

    public void StoneSetting()
        //중간에 관전자가 들어오면 현재 돌 좌표들을 전부 받아서 셋팅하도록 합니다.

    {
        for (int i = 0; i < SIZE; i++)
        {
            for (int j = 0; j < SIZE; j++)
            {
                if (ball[i, j] == 1 || ball[i, j] == 2)
                {
                    ballScan(j, i).GetComponent<omokClick>().ballClick(ball[i, j]);
                }
            }
        }
    }


    void GameStart(bool isBlack)
        //게임 시작시 초기화를 전부 시키고 게임 시작합니다.
    {
        for (int i = 0; i < SIZE; i++)
        {
            for (int j = 0; j < SIZE; j++)
            {
                if (ball[i, j] == 0)
                {
                    continue;
                }

                ballScan(j, i).GetComponent<omokClick>().Clear();
            }
        }

        IsPlay = true;

        IsBlackTurn = true;


        if (isBlack)
        {
            myPlayType = ePlayType.BLACK;

            //GameManager.inst.chatManager.gameInfo.text = $"{GameManager.inst.Player1}님의 차례입니다.";
        }
        else
        {
            myPlayType = ePlayType.WHITE;

            //GameManager.inst.chatManager.gameInfo.text = $"{GameManager.inst.Player2}님의 차례입니다.";
        }
    }

    public void BallClick(int row, int column,bool black)
        //돌 클릭 이벤트
    {
        if (IsBlackTurn)
        {
            ball[column, row] = 1;
            //해당 좌표 설정
        }
        else
        {
            ball[column, row] = 2;
            //해당 좌표 설정
        }

        for (int i = 0; i < SIZE; i++)
        {
            for (int j = 0; j < SIZE; j++)
            {
                if (ball[i, j] == 1 || ball[i, j] == 2)
                {
                    ballScan(j, i).StoneHelopOb.SetActive(false);
                }
            }
        }

        var _ball = ballScan(row, column);

        var color = _ball.transform.GetComponent<Image>().color;
        color.a = 1f;
        _ball.GetComponent<Image>().color = color;
        //알파값을 1로 조절해서 보이도록 합니다.

        if (black)
        {
            _ball.icon.sprite = omokManager.inst.dollImg[0];
        }
        else
        {
            _ball.icon.sprite = omokManager.inst.dollImg[1];
        }

        if (VictoryCheck(row, column))
        //돌 클릭하고 게임이 끝났는지 파악합니다
        {
            if (myPlayType==ePlayType.BLACK)
            {
                UnityTcpClient.inst.OnGameEnd(IsBlackTurn);
            }
        }
        else
        {
            ////게임이 끝난게 아니면 턴을 넘깁니다.
            //string enemyName = GameManager.inst.Player1 == GameManager.inst.nickName
            //    ? GameManager.inst.Player2
            //    : GameManager.inst.Player1;

            //if (IsBlackTurn)
            //{
                
            //}
            //else
            //{
            //    SocketManager.inst.socket.Emit("Turn", "White", row, column, enemyName);
            //}

            //UnityTcpClient.inst.OnGameEnd();

            //GameManager.inst.chatManager.gameInfo.text = $"{enemyName}님의 차례입니다.";
            TurnChange();
        }
    }

    bool VictoryCheck(int row, int column)
    {
        int ballNum = 1;
        //흑은 1번
        if (!IsBlackTurn)
        {
            ballNum = 2;
            //흰돌은 2번
        }

        if (FiveCheck(ballNum))
            //5개가 있는지 파악합니다.
        {
            return true;
        }

        return false;
    }

    bool InRange(params int[] v)
        //범위가 벗어나면 오류가 뜨기때문에 검사합니다.
    {
        for (int i = 0; i < v.Length; i++)
            if (!(v[i] >= 0 && v[i] < SIZE))
                return false;

        return true;
    }

    bool FiveCheck(int ballNum)
    {
        //완전 탐색합니다.
        for (int i = 0; i < SIZE; i++)
        {
            for (int j = 0; j < SIZE; j++)
            {
                if (ball[i, j] != ballNum)
                {
                    continue;
                }
                //자신의 돌

                //→
                //왼쪽으로 돌을 확인합니다.
                if (InRange(j + 4) && ball[i, j + 1] == ballNum && ball[i, j + 2] == ballNum &&
                    ball[i, j + 3] == ballNum && ball[i, j + 4] == ballNum)
                {
                    return true;
                }
                //↓
                //아래칸으로 돌을 확인합니다.
                else if (InRange(i + 4) && ball[i + 1, j] == ballNum && ball[i + 2, j] == ballNum &&
                         ball[i + 3, j] == ballNum && ball[i + 4, j] == ballNum)
                {
                    return true;
                }
                //↘
                //대각선아래로 돌을 확인합니다.
                else if (InRange(i + 4, j + 4) && ball[i + 1, j + 1] == ballNum && ball[i + 2, j + 2] == ballNum &&
                         ball[i + 3, j + 3] == ballNum && ball[i + 4, j + 4] == ballNum)
                {
                    return true;
                }
                //↙
                //왼쪽대각선아래로 돌을 확인합니다.
                else if (InRange(i + 4, j - 4) && ball[i + 1, j - 1] == ballNum && ball[i + 2, j - 2] == ballNum &&
                         ball[i + 3, j - 3] == ballNum && ball[i + 4, j - 4] == ballNum)
                {
                    return true;
                }
            }
        }

        return false;
    }

    public void omokCheck()
    {
        //렌주룰 이기때문에
        //흑만 검사합니다
        if (myPlayType != ePlayType.BLACK)
        {
            return;
        }

        for (int i = 0; i < SIZE; i++)
        {
            for (int j = 0; j < SIZE; j++)
            {
                SixCheck(i, j);
            }
        }


        for (int i = 0; i < SIZE; i++)
        {
            for (int j = 0; j < SIZE; j++)
            {
                if (ball[i, j] != 0)
                {
                    continue;
                }

                ThreeThreeCheck(i, j);
            }
        }
    }

    void SixCheck(int column, int row)
        //해당 좌표가 6목이라면 X표시를 합니다.
    {
        if (ball[column, row] != 0)
        {
            return;
        }
        //0번만 확인합니다.

        ball[column, row] = 1;
        //미리 흑돌을 깔고 6목인지 확인합니다.

        //완전탐색을 합니다.
        int ballNum = 1;
        for (int i = 0; i < SIZE; i++)
        {
            for (int j = 0; j < SIZE; j++)
            {
                if (ball[i, j] != 1)
                {
                    continue;
                }

                //→
                if (InRange(j + 5) && ball[i, j + 1] == ballNum && ball[i, j + 2] == ballNum &&
                    ball[i, j + 3] == ballNum && ball[i, j + 4] == ballNum && ball[i, j + 5] == ballNum)
                {
                    //오목이 맞다면 X를 표시합니다.
                    ball[column, row] = 3;
                    ballScan(row, column).GetComponent<omokClick>().ballClick(3);
                    return;
                }
                //↓
                else if (InRange(i + 5) && ball[i + 1, j] == ballNum && ball[i + 2, j] == ballNum &&
                         ball[i + 3, j] == ballNum && ball[i + 4, j] == ballNum && ball[i + 5, j] == ballNum)
                {
                    //오목이 맞다면 X를 표시합니다.
                    ball[column, row] = 3;
                    ballScan(row, column).GetComponent<omokClick>().ballClick(3);
                    return;
                }
                //↘
                else if (InRange(i + 5, j + 5) && ball[i + 1, j + 1] == ballNum && ball[i + 2, j + 2] == ballNum &&
                         ball[i + 3, j + 3] == ballNum && ball[i + 4, j + 4] == ballNum &&
                         ball[i + 5, j + 5] == ballNum)
                {
                    //오목이 맞다면 X를 표시합니다.
                    ball[column, row] = 3;
                    ballScan(row, column).GetComponent<omokClick>().ballClick(3);
                    return;
                }
                //↙
                else if (InRange(i + 5, j - 5) && ball[i + 1, j - 1] == ballNum && ball[i + 2, j - 2] == ballNum &&
                         ball[i + 3, j - 3] == ballNum && ball[i + 4, j - 4] == ballNum &&
                         ball[i + 5, j - 5] == ballNum)
                {
                    //오목이 맞다면 X를 표시합니다.
                    ball[column, row] = 3;
                    ballScan(row, column).GetComponent<omokClick>().ballClick(3);
                    return;
                }
            }
        }

        ball[column, row] = 0;
        //아무것도 아니라면 다시 0으로 돌림.
    }


    public void NoClear()
        //X표시를 전부 지웁니다.
    {
        if (myPlayType != ePlayType.BLACK)
        {
            return;
        }

        for (int i = 0; i < noObs.Count; i++)
        {
            if (noObs[i].TryGetComponent(out omokClick click))
            {
                click.NoOff();
            }
        }

        noObs.Clear();
    }

    public void TurnChange()
    {
        IsBlackTurn = !IsBlackTurn;
        if (IsBlackTurn)
        {
            //흑턴
            omokCheck();
            //착수 금지 표시를 합니다.
        }
        else
        {
            //백턴
            NoClear();
            //본인의 X표시를 전부 지웁니다.
        }
    }


    void ThreeThreeCheck(int i, int j)
    {
        if (ball[i, j] != 0)
        {
            return;
        }
        //다른 돌이 있다면 넘어감

        ball[i, j] = 1;
        int ThreeValue = 0;

        if (FiveCheck(1))
            //해당자리가 5오목이라면 착수금지가 사라짐.
        {
            ball[i, j] = 0;
            return;
        }


        //→
        if (ThreeCheck(i, j - 4, i, j - 3, i, j - 2, i, j - 1, i, j + 1, i, j + 2, i, j + 3, i, j + 4)) ++ThreeValue;

        //↓
        if (ThreeCheck(i - 4, j, i - 3, j, i - 2, j, i - 1, j, i + 1, j, i + 2, j, i + 3, j, i + 4, j)) ++ThreeValue;

        //↘
        if (ThreeCheck(i - 4, j - 4, i - 3, j - 3, i - 2, j - 2, i - 1, j - 1, i + 1, j + 1, i + 2, j + 2, i + 3, j + 3,
                i + 4, j + 4)) ++ThreeValue;

        //↙
        if (ThreeCheck(i + 4, j - 4, i + 3, j - 3, i + 2, j - 2, i + 1, j - 1, i - 1, j + 1, i - 2, j + 2, i - 3, j + 3,
                i - 4, j + 4)) ++ThreeValue;


        if (ThreeValue >= 2)
        {
            //착수금지라면 X표시를합니다.
            ball[i, j] = 3;
            ballScan(j, i).GetComponent<omokClick>().ballClick(3);
        }
        else
        {
            //아니라면 원래대로 돌림
            ball[i, j] = 0;
        }
    }


    bool ThreeCheck(int im4, int jm4, int im3, int jm3, int im2, int jm2, int im1, int jm1, int ip1, int jp1, int ip2,
        int jp2, int ip3, int jp3, int ip4, int jp4)
    {
        if (InRange(im4, jm4, ip1, jp1))
            if (ball[im4, jm4] == 0 && ball[im3, jm3] == 1 && ball[ip1, jp1] == 0)
            {
                if (ball[im2, jm2] == 1 && ball[im1, jm1] == 0) return true;
                if (ball[im2, jm2] == 0 && ball[im1, jm1] == 1) return true;
            }

        if (InRange(im3, jm3, ip1, jp1))
            if (ball[im3, jm3] == 0 && ball[im2, jm2] == 1 && ball[im1, jm1] == 1 && ball[ip1, jp1] == 0)
                return true;

        if (InRange(im3, jm3, ip2, jp2))
            if (ball[im3, jm3] == 0 && ball[im2, jm2] == 1 && ball[im1, jm1] == 0 && ball[ip1, jp1] == 1 &&
                ball[ip2, jp2] == 0)
                return true;

        if (InRange(im2, jm2, ip2, jp2)) // 중앙
            if (ball[im2, jm2] == 0 && ball[im1, jm1] == 1 && ball[ip1, jp1] == 1 && ball[ip2, jp2] == 0)
                return true;

        if (InRange(im2, jm2, ip3, jp3))
            if (ball[im2, jm2] == 0 && ball[im1, jm1] == 1 && ball[ip1, jp1] == 0 && ball[ip2, jp2] == 1 &&
                ball[ip3, jp3] == 0)
                return true;

        if (InRange(im1, jm1, ip3, jp3))
            if (ball[im1, jm1] == 0 && ball[ip1, jp1] == 1 && ball[ip2, jp2] == 1 && ball[ip3, jp3] == 0)
                return true;

        if (InRange(im1, jm1, ip4, jp4))
            if (ball[im1, jm1] == 0 && ball[ip3, jp3] == 1 && ball[ip4, jp4] == 0)
            {
                if (ball[ip1, jp1] == 1 && ball[ip2, jp2] == 0) return true;
                if (ball[ip1, jp1] == 0 && ball[ip2, jp2] == 1) return true;
            }

        return false;
    }
}