using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using TMPro;
using UnityEditor.Sprites;
using UnityEngine;
using UnityEngine.UI;

public class UnityTcpClient : MonoBehaviour
{
    private TcpClient client;
    private NetworkStream stream;
    private Thread receiveThread;
    private bool isConnected = false;

    public Button button1;
    public TMP_InputField inputField;

    void Start()
    {
        // 서버에 연결
        ConnectToServer("127.0.0.1", 8888);

        button1.onClick.AddListener(OnButton);
    }

    private void OnButton()
    {
        if(inputField.text.Length>0)
        {
            SendDataToServer(inputField.text);
            inputField.text = "";
        }
    }

    public void ConnectToServer(string ipAddress, int port)
    {
        try
        {
            client = new TcpClient(ipAddress, port);

            if (client == null)
            {
                Debug.LogError("Failed to create TcpClient.");
                return;
            }

            stream = client.GetStream();
            isConnected = true;

            // 데이터 수신을 위한 백그라운드 스레드 시작
            receiveThread = new Thread(ReceiveMessages);
            receiveThread.IsBackground = true;
            receiveThread.Start();

            Debug.Log("Connected to server.");
        }
        catch (Exception ex)
        {
            Debug.LogError("Failed to connect to server: " + ex.Message);
        }
    }

    private void ReceiveMessages()
    {
        try
        {
            while (isConnected)
            {
                // 1. 데이터 길이 읽기 (4 바이트, Int32)
                byte[] lengthBuffer = new byte[sizeof(int)];
                int bytesRead = stream.Read(lengthBuffer, 0, lengthBuffer.Length);
                if (bytesRead != sizeof(int))
                {
                    throw new Exception("Failed to read data length.");
                }

                int dataLength = BitConverter.ToInt32(lengthBuffer, 0);

                // 2. 실제 데이터 읽기
                byte[] dataBuffer = new byte[dataLength];
                bytesRead = 0;
                while (bytesRead < dataLength)
                {
                    bytesRead += stream.Read(dataBuffer, bytesRead, dataLength - bytesRead);
                }

                // 3. 데이터 처리
                string message = Encoding.UTF8.GetString(dataBuffer);
                //Packet packet = PacketSerializer.Deserialize(json);
                HandlePacket(message);
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("Error receiving data: " + ex.Message);
            isConnected = false;
        }
    }

    private void HandlePacket(string message)
    {
        Debug.Log("Received text: " + message);

        //// 패킷 타입에 따른 처리
        //switch (packet.Type)
        //{
        //    case PacketType.Text:
        //        Debug.Log("Received text: " + packet.Data);
        //        break;
        //    case PacketType.Login:
        //        Debug.Log("Login response: " + packet.Data);
        //        break;
        //    case PacketType.JoinRoom:
        //        Debug.Log("Join room response: " + packet.Data);
        //        break;
        //    case PacketType.LeaveRoom:
        //        Debug.Log("Leave room response: " + packet.Data);
        //        break;
        //    default:
        //        Debug.LogWarning("Unknown packet type: " + packet.Type);
        //        break;
        //}
    }

    public void SendDataToServer(string message)
    {
        if (client == null || !client.Connected)
            return;

        try
        {
            // 패킷을 JSON으로 직렬화
            //string json = PacketSerializer.Serialize(packet);
            byte[] data = Encoding.UTF8.GetBytes(message);

            // 데이터 길이 계산 및 전송 (4 바이트)
            byte[] dataLength = BitConverter.GetBytes(data.Length);
            stream.Write(dataLength, 0, dataLength.Length);

            // 실제 데이터 전송
            stream.Write(data, 0, data.Length);
            Debug.Log("Packet sent to server: " + message);
        }
        catch (Exception ex)
        {
            Debug.LogError("Failed to send data: " + ex.Message);
        }
    }

    private void OnApplicationQuit()
    {
        CloseConnection();
    }

    public void CloseConnection()
    {
        isConnected = false;

        receiveThread?.Abort();


        if (stream != null) stream.Close();
        if (client != null) client.Close();
        Debug.Log("Connection closed.");
    }
}
