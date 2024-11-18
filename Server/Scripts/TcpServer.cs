
using GameServer.Scripts;
using Newtonsoft.Json;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace GameServer.Scripts
{
    public enum PacketType
    {
        Login,
        JoinRoom,
        LeaveRoom,
        Text,
        Turn,
        Match,
        
    }
    public class Packet
    {
        public PacketType Type { get; set; }
        public string Data { get; set; }
    }

    public class TcpServer
    {
        public static TcpServer? Instance;
        //private TcpListener? server;
        private Socket? serverSocket;
        private bool isRunning;
        private ConcurrentDictionary<string, Room> rooms = new ConcurrentDictionary<string, Room>();
        private Dictionary<EndPoint, Socket> clientMap = new Dictionary<EndPoint, Socket>();
        private Dictionary<EndPoint, string> clientNames = new();
        private const int IntSize = sizeof(int);
        public string ipAddress;
        public int port;
        private static readonly object lockObj = new object();
        private List<EndPoint> waitingPlayers = new List<EndPoint>();
        public Action<Socket>? OnClientConnected;
        public Action<Socket>? OnClientDisconnected;
        public Action<Packet, Socket>? OnPacketReceived;
        public void AddPlayerToMatchmaking(EndPoint point)
        {
            lock (lockObj)
            {
                waitingPlayers.Add(point);
            }
        }
        public async Task StartMatchmaking()
        {
           
            while (isRunning)
            {
                try
                {
                    await Task.Delay(1000); // 매치메이킹 주기를 1초마다 수행
                    List<(EndPoint, EndPoint)> matchedPlayers = new List<(EndPoint, EndPoint)>();

                    lock (lockObj)
                    {
                        foreach (var player in waitingPlayers.ToList()) // 대기 중인 플레이어 목록 복사
                        {
                            EndPoint? opponent = FindBestMatch(player);

                            if (opponent != null)
                            {
                                matchedPlayers.Add((player, opponent));
                                waitingPlayers.Remove(player);
                                waitingPlayers.Remove(opponent);
                            }
                        }
                    }

                    // 매칭된 플레이어들을 처리
                    foreach (var match in matchedPlayers)
                    {
                        if (clientMap.ContainsKey(match.Item1) && clientMap.ContainsKey(match.Item2))
                        {
                            var dd = clientMap[match.Item1];
                            var dd_name = clientNames[match.Item1];
                            var dd2 = clientMap[match.Item2];
                            var dd2_name = clientNames[match.Item2];

                            JoinRoom($"{dd_name},{dd2_name}", dd);
                            JoinRoom($"{dd_name},{dd2_name}", dd2);

                            await SendDataToClientAsync(dd, new Packet { Type = PacketType.JoinRoom, Data = $"{dd_name},{dd2_name}" });
                            await SendDataToClientAsync(dd2, new Packet { Type = PacketType.JoinRoom, Data = $"{dd_name},{dd2_name}" });

                            Form.Inst.AddLog($"Matching  : {dd_name} / {dd2_name}");
                        }
                    }
                }
                catch (Exception)
                {

                    
                }
                
            }
        }


        private EndPoint? FindBestMatch(EndPoint player)
        {
            int playerScore = GetPlayerScore(player); // 플레이어 점수를 가져오는 메서드
            int initialScoreRange = 50; // 초기 매칭 범위
            int maxScoreDifference = 1500; // 최대 허용 매칭 범위
            TimeSpan maxWaitTime = TimeSpan.FromSeconds(30); // 대기 시간이 30초가 넘으면 매칭 범위 확장

            DateTime playerEnqueueTime = GetPlayerEnqueueTime(player); // 플레이어가 대기열에 추가된 시간을 가져오는 메서드
            int scoreRange = (DateTime.Now - playerEnqueueTime) > maxWaitTime ? maxScoreDifference : initialScoreRange;

            var potentialMatches = waitingPlayers
                .Where(p => p != player)
                .Select(p => new { Player = p, Score = GetPlayerScore(p), EnqueueTime = GetPlayerEnqueueTime(p) })
                .Where(p => Math.Abs(p.Score - playerScore) <= scoreRange)
                .OrderBy(p => Math.Abs(p.Score - playerScore)) // 점수 차이가 적은 순서대로 정렬
                .ThenBy(p => p.EnqueueTime) // 대기 시간이 오래된 순서대로 정렬
                .ToList();

            return potentialMatches.FirstOrDefault()?.Player;
        }

        private int GetPlayerScore(EndPoint player)
        {
            if (!playerScores.ContainsKey(player))
            {
                Random random = new Random();
                playerScores[player] = random.Next(1000, 1000); // 0부터 1000 사이의 무작위 점수 설정
            }
            return playerScores[player];
        }

        private DateTime GetPlayerEnqueueTime(EndPoint player)
        {
            // 플레이어가 대기열에 추가된 시간을 반환하는 로직을 여기에 추가합니다.
            if (!playerEnqueueTimes.ContainsKey(player))
            {
                playerEnqueueTimes[player] = DateTime.Now;
            }
            return playerEnqueueTimes[player];
        }

        private Dictionary<EndPoint, int> playerScores = new Dictionary<EndPoint, int>();
        private Dictionary<EndPoint, DateTime> playerEnqueueTimes = new Dictionary<EndPoint, DateTime>();


        //private static void CreateMatch(Player player1, Player player2)
        //{
        //    Console.WriteLine($"Match created between {player1.PlayerId} (Score: {player1.Score}) and {player2.PlayerId} (Score: {player2.Score})");
        //    // 매치 생성 후, 게임 시작 등의 로직을 추가할 수 있습니다.
        //}
        public static void Init(string ipAddress, int port)
        {
            Instance = new TcpServer();
            //Instance.server = new TcpListener(IPAddress.Parse(ipAddress), port);
            Instance.serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            Instance.isRunning = false;
            Instance.ipAddress = ipAddress;
            Instance.port = port;
            Instance.Start();

            _ = Task.Run(() => Instance.StartMatchmaking()); // 비동기로 매치메이킹 시작
        }


        //private async void ServerLoop()
        //{
        //    try
        //    {
        //        //while (isRunning)
        //        //{
        //        //    TcpClient client = server.AcceptTcpClient();
        //        //    IPEndPoint? clientEndPoint = client.Client.RemoteEndPoint as IPEndPoint;

        //        //    if (clientEndPoint != null)
        //        //    {
        //        //        clientMap.Add(clientEndPoint, client);
        //        //        Form.Inst.AddLog("Client connected: " + clientEndPoint);

        //        //    }
        //        //    Thread clientThread = new Thread(() => HandleClientAsync(client).Wait());
        //        //    clientThread.Start();
        //        //}

        //        while (isRunning)
        //        {
        //            TcpClient client = await server.AcceptTcpClientAsync();
        //            IPEndPoint? clientEndPoint = client.Client.RemoteEndPoint as IPEndPoint;

        //            if (clientEndPoint != null)
        //            {
        //                clientMap.Add(clientEndPoint, client);
        //                Form.Inst.AddLog("Client connected: " + clientEndPoint);
        //            }
        //            _ = HandleClientAsync(client); // 비동기로 클라이언트 처리 시작
        //        }
        //    }
        //    catch (Exception)
        //    {

        //    }

        //}

        //private async Task HandleClientAsync(TcpClient client)
        //{
        //    NetworkStream stream = client.GetStream();

        //    try
        //    {
        //        while (true)
        //        {
        //            byte[] lengthBuffer = new byte[IntSize];

        //            int bytesRead = await stream.ReadAsync(lengthBuffer, 0, lengthBuffer.Length);

        //            if (bytesRead != IntSize)
        //            {
        //                throw new Exception("Failed to read data length.");
        //            }

        //            int dataLength = BitConverter.ToInt32(lengthBuffer, 0);

        //            byte[] dataBuffer = new byte[dataLength];
        //            bytesRead = 0;
        //            while (bytesRead < dataLength)
        //            {
        //                bytesRead += await stream.ReadAsync(dataBuffer, bytesRead, dataLength - bytesRead);
        //            }

        //            string json = Encoding.UTF8.GetString(dataBuffer);
        //            Packet packet = PacketSerializer.Deserialize(json);
        //            HandlePacket(packet, client);

        //            //BroadcastMessage(json, client);

        //            Form.Inst.AddLog("Message : " + json);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine($"Exception: {ex.Message}");
        //    }
        //    finally
        //    {
        //        DisconnectClient(client);
        //        client.Close();
        //        Form.Inst.AddLog("Client disconnected.");
        //    }
        //}

        //public void BroadcastMessage(string message, TcpClient sender)
        //{
        //    IPEndPoint? senderEndpoint = sender.Client.RemoteEndPoint as IPEndPoint;

        //    List<Task> sendTasks = new List<Task>();


        //    foreach (var item in clientMap)
        //    {
        //        //if (item.Key == senderEndpoint)
        //        //    continue;

        //        try
        //        {
        //            NetworkStream stream = item.Value.GetStream();
        //            string json = message;
        //            byte[] data = Encoding.UTF8.GetBytes(json);

        //            byte[] dataLength = BitConverter.GetBytes(data.Length);
        //            stream.Write(dataLength, 0, dataLength.Length);
        //            stream.Write(data, 0, data.Length);
        //            Form.Inst.AddLog($"Send : Client : {item.Key}  message:{message}");

        //        }
        //        catch (Exception ex)
        //        {
        //            Form.Inst.AddLog($"Failed to send data to client: {ex.Message}");
        //        }
        //    }

        //}
        private void HandleLogin(string data, Socket sender)
        {
            Database.AccountUpdate(data, "Login");
            var e = sender.RemoteEndPoint;
            
            clientNames[e] = data;
            Console.WriteLine($"Client logged Id: {data}");
        }
        private void HandlePacket(Packet packet, Socket sender)

        {
            switch (packet.Type)
            {
                case PacketType.Text:
                    string roomName = ExtractRoomNameFromPacket(packet);
                    HandleChatMessage(roomName, packet.Data, sender);
                    break;

                case PacketType.Login:
                    HandleLogin(packet.Data, sender);
                    break;

                case PacketType.JoinRoom:
                    string joinRoomName = packet.Data;
                    JoinRoom(joinRoomName, sender);
                    break;

                case PacketType.LeaveRoom:
                    string roomName3 = ExtractRoomNameFromPacket(packet);
                    HandleChatMessage3(roomName3, packet.Data, sender);
                    LeaveRoom(roomName3, sender);

                    break;
                case PacketType.Turn:
                    string roomName2 = ExtractRoomNameFromPacket(packet);
                    HandleChatMessage2(roomName2, packet.Data, sender);
                    break;
                case PacketType.Match:
                    AddPlayerToMatchmaking(sender.RemoteEndPoint);
                    //string roomName2 = ExtractRoomNameFromPacket(packet);
                    //HandleChatMessage2(roomName2, packet.Data, sender);
                    break;
                default:
                    Console.WriteLine("Unknown packet type: " + packet.Type);
                    break;
            }
        }

        private void OnTurn(string roomName, Socket client)
        {
            EndPoint endpoint = client.RemoteEndPoint;
            if (endpoint != null)
            {
                if (rooms.ContainsKey(roomName))
                {
                    rooms[roomName].RemoveClient(client);
                    clientMap.Remove(endpoint);
                    Console.WriteLine($"Client left room: {roomName}");
                }
            }
        }

        private string ExtractRoomNameFromPacket(Packet packet)
        {
            string[] parts = packet.Data.Split(new[] { ':' }, 2);
            return parts.Length > 1 ? parts[0] : "DefaultRoom";
        }

        private void HandleChatMessage(string roomName, string message, Socket sender)
        {
            if (rooms.ContainsKey(roomName))
            {
                rooms[roomName].BroadcastMessage(message, sender, clientMap);
            }
            else
            {
                Console.WriteLine("Room not found: " + roomName);
            }
        }
        private void HandleChatMessage2(string roomName, string message, Socket sender)
        {
            if (rooms.ContainsKey(roomName))
            {
                rooms[roomName].BroadcastMessage2(message, sender, clientMap);
            }
            else
            {
                Console.WriteLine("Room not found: " + roomName);
            }
        }

        private void HandleChatMessage3(string roomName, string message, Socket sender)
        {
            if (rooms.ContainsKey(roomName))
            {
                rooms[roomName].BroadcastMessage3(message, sender, clientMap);
            }
            else
            {
                Console.WriteLine("Room not found: " + roomName);
            }
        }
        private void JoinRoom(string roomName, Socket client)
        {
            EndPoint endpoint = client.RemoteEndPoint;
            if (endpoint != null)
            {
                if (!rooms.ContainsKey(roomName))
                {
                    rooms[roomName] = new Room(roomName);
                }

                rooms[roomName].AddClient(client);
                clientMap[endpoint] = client;
                Form.Inst.AddLog($"Client joined room: {roomName}//{endpoint}");
            }
        }

        private void LeaveRoom(string roomName, Socket client)
        {
            EndPoint endpoint = client.RemoteEndPoint;
            if (endpoint != null)
            {
                if (rooms.ContainsKey(roomName))
                {
                    rooms[roomName].RemoveClient(client);
                    clientMap.Remove(endpoint);
                    Console.WriteLine($"Client left room: {roomName}");
                }
            }
        }
        //private void LeaveRoom(string roomName, TcpClient client)
        //{
        //    IPEndPoint endpoint = client.Client.RemoteEndPoint as IPEndPoint;
        //    if (endpoint != null)
        //    {
        //        if (rooms.ContainsKey(roomName))
        //        {
        //            rooms[roomName].RemoveClient(client);
        //            clientMap.Remove(endpoint);
        //            Console.WriteLine($"Client left room: {roomName}");
        //        }
        //    }
        //}
        private void DisconnectClient(Socket client)
        {
            EndPoint endpoint = client.RemoteEndPoint;
            if (endpoint != null)
            {
                clientMap.Remove(endpoint);
                client.Close();
            }
        }
        //private void DisconnectClient(TcpClient client)
        //{
        //    //foreach (var room in rooms.Values)
        //    //{
        //    //    room.RemoveClient(client);
        //    //}

        //    IPEndPoint endpoint = client.Client.RemoteEndPoint as IPEndPoint;
        //    if (endpoint != null)
        //    {
        //        clientMap.Remove(endpoint);
        //    }
        //}

        //private string ExtractRoomNameFromPacket(Packet packet)
        //{
        //    string[] parts = packet.Data.Split(new[] { ':' }, 2);
        //    return parts.Length > 1 ? parts[0] : "DefaultRoom";
        //}

        //public void Start()
        //{
        //    if (isRunning)
        //        return;
        //    isRunning = true;

        //    if (server != null) server.Start();
        //    Form.Inst.AddLog("Server started on " + ipAddress + ":" + port);
        //    Thread serverThread = new Thread(ServerLoop);
        //    serverThread.Start();
        //}

        public void Start()
        {
            if (isRunning)
                return;
            isRunning = true;

            serverSocket.Bind(new IPEndPoint(IPAddress.Parse(ipAddress), port));
            serverSocket.Listen(100);
            Form.Inst.AddLog("Server started on " + ipAddress + ":" + port);
            StartAccept();
        }

        public void Stop()
        {
            if (!isRunning)
                return;
            isRunning = false;
            serverSocket?.Close();
            foreach (var client in clientMap.Values)
            {
                client.Close();
            }
            clientMap.Clear();
            Form.Inst.AddLog("Server stopped.");
        }

        private void StartAccept()
        {
            if (!isRunning) return;

            SocketAsyncEventArgs acceptEventArg = new SocketAsyncEventArgs();
            acceptEventArg.Completed += OnAcceptCompleted;

            try
            {
                if (!serverSocket.AcceptAsync(acceptEventArg))
                {
                    OnAcceptCompleted(this, acceptEventArg);
                }
            }
            catch (ObjectDisposedException)
            {
                // 서버 소켓이 이미 닫힌 경우 예외 처리
            }
        }

        private void OnAcceptCompleted(object? sender, SocketAsyncEventArgs e)
        {
            if (e.SocketError == SocketError.Success)
            {
                Socket clientSocket = e.AcceptSocket;
                EndPoint clientEndPoint = clientSocket.RemoteEndPoint;
                clientMap[clientEndPoint] = clientSocket;
                Form.Inst.AddLog("Client connected: " + clientEndPoint);
                OnClientConnected?.Invoke(clientSocket);

                StartReceive(clientSocket);
            }
            StartAccept();
        }

        private void StartReceive(Socket clientSocket)
        {
            SocketAsyncEventArgs receiveEventArg = new SocketAsyncEventArgs();
            receiveEventArg.SetBuffer(new byte[IntSize], 0, IntSize);
            receiveEventArg.UserToken = clientSocket;
            receiveEventArg.Completed += OnReceiveCompleted;

            if (!clientSocket.ReceiveAsync(receiveEventArg))
            {
                OnReceiveCompleted(this, receiveEventArg);
            }
        }

        private void OnReceiveCompleted(object? sender, SocketAsyncEventArgs e)
        {
            Socket clientSocket = (Socket)e.UserToken;

            if (e.SocketError == SocketError.Success && e.BytesTransferred > 0)
            {
                if (e.Buffer.Length == IntSize)
                {
                    int dataLength = BitConverter.ToInt32(e.Buffer, 0);
                    SocketAsyncEventArgs receiveDataEventArg = new SocketAsyncEventArgs();
                    receiveDataEventArg.SetBuffer(new byte[dataLength], 0, dataLength);
                    receiveDataEventArg.UserToken = clientSocket;
                    receiveDataEventArg.Completed += OnReceiveDataCompleted;

                    if (!clientSocket.ReceiveAsync(receiveDataEventArg))
                    {
                        OnReceiveDataCompleted(this, receiveDataEventArg);
                    }
                }
            }
            else
            {
                DisconnectClient(clientSocket);
                OnClientDisconnected?.Invoke(clientSocket);
            }
        }

        private void OnReceiveDataCompleted(object? sender, SocketAsyncEventArgs e)
        {
            Socket clientSocket = (Socket)e.UserToken;

            if (e.SocketError == SocketError.Success && e.BytesTransferred > 0)
            {
                string json = Encoding.UTF8.GetString(e.Buffer, 0, e.BytesTransferred);
                Packet packet = PacketSerializer.Deserialize(json);
                HandlePacket(packet, clientSocket);

                OnPacketReceived?.Invoke(packet, clientSocket);
                Form.Inst.AddLog("Message : " + json);

                StartReceive(clientSocket);
            }
            else
            {
                DisconnectClient(clientSocket);
                OnClientDisconnected?.Invoke(clientSocket);
            }
        }


        public static void SendDataToClient(TcpClient client, Packet packet)
        {
            try
            {
                IPEndPoint endpoint = client.Client.RemoteEndPoint as IPEndPoint;

                NetworkStream stream = client.GetStream();
                string json = PacketSerializer.Serialize(packet);
                byte[] data = Encoding.UTF8.GetBytes(json);

                byte[] dataLength = BitConverter.GetBytes(data.Length);
                stream.Write(dataLength, 0, dataLength.Length);
                stream.Write(data, 0, data.Length);

                Form.Inst.AddLog($"Server SendData {endpoint}//{packet.Type} // {packet.Data}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to send data to client: {ex.Message}");
            }
        }

        public static Task SendDataToClientAsync(Socket client, Packet packet)
        {
            var tcs = new TaskCompletionSource<bool>();

            try
            {
                // 클라이언트의 엔드포인트 정보 가져오기
                EndPoint endpoint = client.RemoteEndPoint;

                // 패킷을 JSON 형식으로 직렬화
                string json = PacketSerializer.Serialize(packet);
                byte[] data = Encoding.UTF8.GetBytes(json);

                // 데이터 길이를 바이트 배열로 변환 (네트워크 바이트 오더 사용 권장)
                byte[] dataLength = BitConverter.GetBytes(data.Length);
                if (BitConverter.IsLittleEndian)
                {
                    Array.Reverse(dataLength); // 빅 엔디안으로 변환
                }

                // 프로토콜에 따라 데이터 길이와 데이터를 결합
                byte[] sendBuffer = new byte[dataLength.Length + data.Length];
                Buffer.BlockCopy(dataLength, 0, sendBuffer, 0, dataLength.Length);
                Buffer.BlockCopy(data, 0, sendBuffer, dataLength.Length, data.Length);

                // SocketAsyncEventArgs 객체 생성 및 설정
                SocketAsyncEventArgs sendEventArg = new SocketAsyncEventArgs();
                sendEventArg.SetBuffer(sendBuffer, 0, sendBuffer.Length);
                sendEventArg.UserToken = client;

                // Completed 이벤트 핸들러 설정
                sendEventArg.Completed += (s, e) =>
                {
                    if (e.SocketError == SocketError.Success)
                    {
                        // 데이터 전송 성공 로그
                        Form.Inst.AddLog($"Server SendData {endpoint}//{packet.Type} // {packet.Data}");
                        tcs.SetResult(true);
                    }
                    else
                    {
                        // 전송 실패 로그 및 예외 설정
                        Form.Inst.AddLog($"Send failed: {e.SocketError}");
                        tcs.SetException(new SocketException((int)e.SocketError));
                    }

                    // 이벤트 핸들러 제거 (메모리 누수 방지)
                    e.Completed -= null;
                };

                // 비동기 전송 시작
                bool willRaiseEvent = client.SendAsync(sendEventArg);
                if (!willRaiseEvent)
                {
                    // 비동기 이벤트가 발생하지 않고 동기적으로 완료된 경우 처리
                    if (sendEventArg.SocketError == SocketError.Success)
                    {
                        Form.Inst.AddLog($"Server SendData {endpoint}//{packet.Type} // {packet.Data}");
                        tcs.SetResult(true);
                    }
                    else
                    {
                        Form.Inst.AddLog($"Send failed: {sendEventArg.SocketError}");
                        tcs.SetException(new SocketException((int)sendEventArg.SocketError));
                    }
                }
            }
            catch (Exception ex)
            {
                // 예외 발생 시 로그 기록 및 예외 설정
                Form.Inst.AddLog($"Failed to send data to client: {ex.Message}");
                tcs.SetException(ex);
            }

            return tcs.Task;
        }
    }

}

public class Room
{
    public string RoomName { get; private set; }
    private HashSet<EndPoint> clientEndpoints;

    public Room(string roomName)
    {
        RoomName = roomName;
        clientEndpoints = new HashSet<EndPoint>();
    }

    public void AddClient(Socket client)
    {
        EndPoint endpoint = client.RemoteEndPoint;
        if (endpoint != null)
        {
            clientEndpoints.Add(endpoint);
        }
    }

    public void RemoveClient(Socket client)
    {
        EndPoint endpoint = client.RemoteEndPoint;
        if (endpoint != null)
        {
            clientEndpoints.Remove(endpoint);
        }
    }

    //public void BroadcastMessage(string message, TcpClient sender, Dictionary<IPEndPoint, TcpClient> clientMap)
    //{
    //    IPEndPoint senderEndpoint = sender.Client.RemoteEndPoint as IPEndPoint;

    //    List<Task> sendTasks = new List<Task>();

    //    foreach (var endpoint in clientEndpoints)
    //    {
    //        if (!endpoint.Equals(senderEndpoint) && clientMap.TryGetValue(endpoint, out Socket client))
    //        {
    //            sendTasks.Add(Task.Run(() => TcpServer.SendDataToClientAsync(client, new Packet { Type = PacketType.Text, Data = message })));
    //        }
    //    }

    //    Task.WhenAll(sendTasks).Wait();
    //}
    public void BroadcastMessage(string message, Socket sender, Dictionary<EndPoint, Socket> clientMap)
    {
        foreach (var endpoint in clientEndpoints)
        {
            if (clientMap.TryGetValue(endpoint, out Socket client))
            {
                TcpServer.SendDataToClientAsync(client, new Packet { Type = PacketType.Text, Data = message });
            }
        }
    }

    public void BroadcastMessage2(string message, Socket sender, Dictionary<EndPoint, Socket> clientMap)
    {
        foreach (var endpoint in clientEndpoints)
        {
            if (clientMap.TryGetValue(endpoint, out Socket client))
            {
                TcpServer.SendDataToClientAsync(client, new Packet { Type = PacketType.Turn, Data = message });
            }
        }
    }

    public void BroadcastMessage3(string message, Socket sender, Dictionary<EndPoint, Socket> clientMap)
    {
        foreach (var endpoint in clientEndpoints)
        {
            if (clientMap.TryGetValue(endpoint, out Socket client))
            {
                TcpServer.SendDataToClientAsync(client, new Packet { Type = PacketType.LeaveRoom, Data = message });
            }
        }
    }

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





