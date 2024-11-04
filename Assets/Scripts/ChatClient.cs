using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json;
using ParrelSync;
using PimDeWitte.UnityMainThreadDispatcher;

public class UnityTcpClient : MonoBehaviour
{
    private Socket clientSocket;
    private CancellationTokenSource cancellationTokenSource;
    private bool isConnected = false;

    public Button button1;
    public Button button2;
    public Button button3;
    public TMP_InputField inputField;

    public string MyNickName;

    private string NickName;
    private string roomName;

    private const int IntSize = sizeof(int);

    void Start()
    {
        NickName = MyNickName;
        if (ClonesManager.IsClone())
        {
            NickName += ClonesManager.GetArgument();
        }

        // 서버에 연결
        ConnectToServer("127.0.0.1", 8888);

        // 버튼 리스너 설정
        button1.onClick.AddListener(OnButton);

        button2.onClick.AddListener(() =>
        {
            Packet dataPacket = new Packet { Type = PacketType.JoinRoom, Data = $"1" };
            SendDataToServer(dataPacket);
        });

        button3.onClick.AddListener(() =>
        {
            Packet dataPacket = new Packet { Type = PacketType.JoinRoom, Data = $"2" };
            SendDataToServer(dataPacket);
        });
    }

    private void OnButton()
    {
        if (inputField.text.Length > 0)
        {
            if (string.IsNullOrEmpty(roomName))
            {
                Debug.LogWarning("현재 룸에 참가하지 않았습니다.");
                return;
            }

            Packet dataPacket = new Packet { Type = PacketType.Text, Data = $"{roomName}:{inputField.text}" };
            SendDataToServer(dataPacket);
            inputField.text = "";
        }
    }

    public async void ConnectToServer(string ipAddress, int port)
    {
        try
        {
            clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            await clientSocket.ConnectAsync(new IPEndPoint(IPAddress.Parse(ipAddress), port));

            if (!clientSocket.Connected)
            {
                Debug.LogError("서버에 연결하지 못했습니다.");
                return;
            }

            isConnected = true;

            Debug.Log("서버에 연결되었습니다.");

            // 데이터 수신을 위한 비동기 작업 시작
            cancellationTokenSource = new CancellationTokenSource();
            _ = Task.Run(() => ReceiveMessagesAsync(cancellationTokenSource.Token));

            // 로그인 패킷 전송
            Packet loginPacket = new Packet { Type = PacketType.Login, Data = $"{NickName}" };
            await SendDataToServer(loginPacket);
        }
        catch (Exception ex)
        {
            Debug.LogError("서버 연결 실패: " + ex.Message);
        }
    }

    private async Task ReceiveMessagesAsync(CancellationToken cancellationToken)
    {
        byte[] buffer = new byte[1024];
        try
        {
            while (isConnected && !cancellationToken.IsCancellationRequested)
            {
                // 데이터 길이 읽기 (4바이트)
                byte[] lengthBuffer = new byte[4];
                int bytesRead = await ReceiveFullAsync(lengthBuffer, 0, 4);
                if (bytesRead == 0)
                {
                    Debug.Log("서버 연결이 종료되었습니다.");
                    break;
                }
                if (BitConverter.IsLittleEndian)
                {
                    Array.Reverse(lengthBuffer);
                }
                int dataLength = BitConverter.ToInt32(lengthBuffer, 0);

                // 실제 데이터 읽기
                byte[] dataBuffer = new byte[dataLength];
                bytesRead = await ReceiveFullAsync(dataBuffer, 0, dataLength);
                if (bytesRead == 0)
                {
                    Debug.Log("서버 연결이 종료되었습니다.");
                    break;
                }

                string receivedText = Encoding.UTF8.GetString(dataBuffer, 0, dataLength);


                Packet packet = PacketSerializer.Deserialize(receivedText);

                // 메인 스레드에서 패킷 처리
                UnityMainThreadDispatcher.Instance().Enqueue(() =>
                {
                    HandlePacket(packet);
                });
            }
        }
        catch (OperationCanceledException)
        {
            // 취소 시 아무것도 하지 않음
        }
        catch (Exception ex)
        {
            Debug.LogError("데이터 수신 오류: " + ex.Message);
            isConnected = false;

            // 메인 스레드에서 연결 해제 처리
            UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                OnDisconnected();
            });
        }
    }

    private async Task<int> ReceiveFullAsync(byte[] buffer, int offset, int size)
    {
        int totalRead = 0;
        while (totalRead < size)
        {
            int bytesRead = await clientSocket.ReceiveAsync(new ArraySegment<byte>(buffer, offset + totalRead, size - totalRead), SocketFlags.None);
            if (bytesRead == 0)
            {
                // 연결이 종료됨
                return 0;
            }
            totalRead += bytesRead;
        }
        return totalRead;
    }

    private async Task<byte[]> ReceiveExactAsync(int length, CancellationToken cancellationToken)
    {
        byte[] buffer = new byte[length];
        int totalRead = 0;
            Debug.Log($"체크 : {totalRead}");

        while (totalRead < length)
        {
            int read = await clientSocket.ReceiveAsync(new ArraySegment<byte>(buffer, totalRead, length - totalRead), SocketFlags.None);
            Debug.Log($"{read}");
            if (read == 0)
            {
                // 연결이 끊어짐
                return null;
            }
            totalRead += read;
        }

        return buffer;
    }

    private void HandlePacket(Packet packet)
    {
        switch (packet.Type)
        {
            case PacketType.Text:
                // Data 형식: "roomName:message"
                var splitData = packet.Data.Split(new char[] { ':' }, 2);
                if (splitData.Length == 2)
                {
                    string receivedRoomName = splitData[0];
                    string message = splitData[1];
                    Debug.Log($"[룸: {receivedRoomName}] {message}");
                    // UI 업데이트 또는 채팅 창에 메시지 표시
                }
                else
                {
                    Debug.LogWarning("잘못된 Text 패킷 데이터 형식.");
                }
                break;
            case PacketType.Login:
                Debug.Log("로그인 응답: " + packet.Data);
                // 로그인 응답 처리, 예: UI 업데이트
                break;
            case PacketType.JoinRoom:
                roomName = packet.Data;
                Debug.Log("룸 참여 응답: " + packet.Data);
                // 룸 참여 응답 처리, 예: UI 업데이트
                break;
            case PacketType.LeaveRoom:
                Debug.Log("룸 탈퇴 응답: " + packet.Data);
                roomName = null;
                // 룸 탈퇴 응답 처리, 예: UI 업데이트
                break;
            default:
                Debug.LogWarning("알 수 없는 패킷 타입: " + packet.Type);
                break;
        }
    }

    public async Task SendDataToServer(Packet packet)
    {
        if (clientSocket == null || !clientSocket.Connected)
        {
            Debug.LogError("서버에 연결되어 있지 않습니다.");
            return;
        }

        try
        {
            // 패킷을 JSON으로 직렬화
            string json = PacketSerializer.Serialize(packet);
            byte[] data = Encoding.UTF8.GetBytes(json);

            // 데이터 길이 계산 및 전송 (4 바이트)
            byte[] dataLength = BitConverter.GetBytes(data.Length);

            // 데이터 전송
            await clientSocket.SendAsync(new ArraySegment<byte>(dataLength), SocketFlags.None);
            await clientSocket.SendAsync(new ArraySegment<byte>(data), SocketFlags.None);

            Debug.Log("서버로 패킷 전송: " + data.Length);
        }
        catch (Exception ex)
        {
            Debug.LogError("데이터 전송 실패: " + ex.Message);
        }
    }

    private void OnApplicationQuit()
    {
        CloseConnection();
    }

    public void CloseConnection()
    {
        if (!isConnected)
            return;

        isConnected = false;

        cancellationTokenSource?.Cancel();

        try
        {
            clientSocket.Shutdown(SocketShutdown.Both);
        }
        catch (Exception ex)
        {
            Debug.LogWarning("소켓 종료 중 오류 발생: " + ex.Message);
        }

        clientSocket.Close();

        Debug.Log("서버 연결 종료.");
    }

    private void OnDisconnected()
    {
        Debug.Log("서버와의 연결이 끊어졌습니다.");
        // UI 업데이트 또는 상태 변경 처리
    }

    public enum PacketType
    {
        Login,
        JoinRoom,
        LeaveRoom,
        Text
    }

    public class Packet
    {
        public PacketType Type { get; set; }
        public string Data { get; set; }
    }

    public static class PacketSerializer
    {
        public static string Serialize(Packet packet)
        {
            return JsonConvert.SerializeObject(packet);
        }

        public static Packet Deserialize(string json)
        {
            return JsonConvert.DeserializeObject<Packet>(json);
        }
    }
}
