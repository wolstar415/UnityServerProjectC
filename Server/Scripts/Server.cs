using GameServer.Scripts;


enum ServerStatus
{
    None,
    Initialized,
    Starting,
    Running,
}

public static class Game
{

}

class Server
{

    static ServerStatus mStatus;

    public static void Init()
    {
        SetStatus(ServerStatus.Initialized);

        Database.Init();
        
    }

    public static async Task Start()
    {
        if (mStatus != ServerStatus.Initialized)
            return;
        try
        {
            Form.Inst.AddLog("서버시작");
            TcpServer.Init("127.0.0.1", 8888);
            SetStatus(ServerStatus.Starting);
        }
        catch (Exception ex)
        {
        }
    }

    public static async Task ReStart()
    {
        try
        {
            TcpServer.Instance.Start();
        }
        catch (Exception ex)
        {
        }
    }

    public static void SetStatus(ServerStatus s)
    {
        mStatus = s;
    }

    public static void Stop()
    {
        TcpServer.Instance.Stop();
    }
}

