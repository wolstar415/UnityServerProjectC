using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class omokClick : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [Header("세로")] public int column; //세로 ↕

    [Header("가로")] public int row; //가로 ↔

    [Space(20)] [SerializeField] public Image icon; //본인 Image
    [SerializeField] private GameObject stoneHelopOb; //네모 흰색
    [SerializeField] private GameObject noOb; // x 표시

    public GameObject StoneHelopOb => stoneHelopOb;

    //public void OnValidate()
    //{
    //    icon = GetComponent<Image>();
    //    stoneHelopOb = transform.GetChild(0).gameObject;
    //    noOb = transform.GetChild(1).gameObject;

    //    string r = gameObject.name.Split(',')[0].Trim();
    //    string c = gameObject.name.Split(',')[1].Trim();
    //    row = int.Parse(c);
    //    column = int.Parse(r);
    //}

    public bool Check()
        //선택을 할 수 있는지 확인하는 스크립트
    {
        if (omokManager.inst.IsPlay == false)
            //플레이중이 아님
        {
            return false;
        }

        if (omokManager.inst.IsBlackTurn)
            //블랙턴
        {
            if (omokManager.inst.myPlayType != ePlayType.BLACK)
                //본인의 돌 타입이 블랙이 아니라면 false
            {
                return false;
            }
        }
        else
        {
            if (omokManager.inst.myPlayType != ePlayType.WHITE)
                //블랙턴이 아니고 본인의 돌 타입이 흰돌이 아니면 false
            {
                return false;
            }
        }

        if (omokManager.inst.ball[column, row] != 0)
            //해당 돌에 돌이 있다면 false
        {
            return false;
        }

        return true;
    }

    public void OnPointerEnter(PointerEventData eventData)
        //마우스가 들어오면
    {
        if (Check() == false)
        {
            return;
        }

        stoneHelopOb.gameObject.SetActive(true);
        //네모흰색이 표시되도록 합니다.
    }

    public void OnPointerExit(PointerEventData eventData)
        //마우스가 나가면
    {
        if (Check() == false)
        {
            return;
        }

        stoneHelopOb.gameObject.SetActive(false);
        //네모 흰색이 비활성화 시킵니다.
    }

    public void OnPointerClick(PointerEventData eventData)
        //클릭시
    {
        if (Check() == false)
        {
            return;
        }

        UnityTcpClient.inst.OnTurnSend(row, column, omokManager.inst.IsBlackTurn);

        ////omokManager.inst.BallClick(row, column,);
        //return;


        //stoneHelopOb.gameObject.SetActive(false);
        ////네모흰색 비활성화

        //var color = transform.GetComponent<Image>().color;
        //color.a = 1f;
        //GetComponent<Image>().color = color;
        ////알파값을 1로 조절해서 보이도록 합니다.

        //if (omokManager.inst.IsBlackTurn)
        //{
        //    icon.sprite = omokManager.inst.dollImg[0];
        //    //블랙턴이라면 이미지를 블랙으로
        //}
        //else
        //{
        //    icon.sprite = omokManager.inst.dollImg[1];
        //    //흰턴이라면 이미지를 흰색돌로
        //}

        //omokManager.inst.BallClick(row, column);
        //해당 좌표를 클릭했다고 해당 매니저에게 보냅니다.
    }

    public void ballClick(int num)
        //num이 1이라면 흑돌이 채워지고 2면 백돌이 3이라면 X표시가 표시됩니다.
    {
        if (num == 1)
        {
            var color = transform.GetComponent<Image>().color;
            color.a = 1f;
            GetComponent<Image>().color = color;
            icon.sprite = omokManager.inst.dollImg[0];
        }
        else if (num == 2)
        {
            var color = transform.GetComponent<Image>().color;
            color.a = 1f;
            GetComponent<Image>().color = color;
            icon.sprite = omokManager.inst.dollImg[1];
        }
        else if (num == 3)
        {
            noOb.SetActive(true);
            omokManager.inst.noObs.Add(gameObject);
        }
    }


    public void NoOff()
    {
        omokManager.inst.ball[column, row] = 0;
        noOb.SetActive(false);
    }

    public void Clear()
        //초기화 시킵니다.
    {
        var color = transform.GetComponent<Image>().color;
        color.a = 0f;
        GetComponent<Image>().color = color;
        omokManager.inst.ball[column, row] = 0;
        noOb.SetActive(false);
    }


    [ContextMenu("Do Something")]
    void DoSomething()
        //디버그용
    {
        Transform a = GameObject.Find("Grid").transform;
        for (int i = 0; i <= 14; i++)
        {
            for (int j = 0; j <= 14; j++)
            {
                GameObject ob = Instantiate(gameObject, a);
                ob.name = $"{i},{j}";
            }
        }
    }

    [ContextMenu("name")]
    void nameset()
        //디버그용
    {
        string[] s = gameObject.name.Split(',');
        row = int.Parse(s[1]);
        column = int.Parse(s[0]);
    }
}