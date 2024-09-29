using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Windows.Forms;


namespace GameServer.Scripts
{

    public class TcpServer
    {
        public static TcpServer? Instance;
        private TcpListener? server;
        private bool isRunning;
        //private Dictionary<string, Room> rooms = new Dictionary<string, Room>();
        private Dictionary<IPEndPoint, TcpClient> clientMap = new Dictionary<IPEndPoint, TcpClient>();
        private const int IntSize = sizeof(int);
        public string ipAddress;
        public int port;

        public static void Init(string ipAddress, int port)
        {
            Instance = new TcpServer();
            Instance.server = new TcpListener(IPAddress.Parse(ipAddress), port);
            Instance.isRunning = false;
            Instance.ipAddress = ipAddress;
            Instance.port = port;
            Instance.Start();


        }

        private void ServerLoop()
        {
            try
            {
                while (isRunning)
                {
                    TcpClient client = server.AcceptTcpClient();
                    IPEndPoint? clientEndPoint = client.Client.RemoteEndPoint as IPEndPoint;
                    if (clientEndPoint != null)
                    {
                        clientMap.Add(clientEndPoint, client);
                        Form.Inst.AddLog("Client connected: " + clientEndPoint);


                        try
                        {
                            NetworkStream stream = client.GetStream();
                            string json = "Hello";
                            byte[] data = Encoding.UTF8.GetBytes(json);

                            byte[] dataLength = BitConverter.GetBytes(data.Length);
                            stream.Write(dataLength, 0, dataLength.Length);
                            stream.Write(data, 0, data.Length);
                            Form.Inst.AddLog($"Send : Client : {clientEndPoint}  message: Hello");

                        }
                        catch (Exception ex)
                        {
                            Form.Inst.AddLog($"Failed to send data to client: {ex.Message}");
                        }
                    }
                    Thread clientThread = new Thread(() => HandleClientAsync(client).Wait());
                    clientThread.Start();
                }
            }
            catch (Exception)
            {

            }
            
        }

        private async Task HandleClientAsync(TcpClient client)
        {
            NetworkStream stream = client.GetStream();

            try
            {
                while (true)
                {
                    byte[] lengthBuffer = new byte[IntSize];
                    int bytesRead = await stream.ReadAsync(lengthBuffer, 0, lengthBuffer.Length);
                    if (bytesRead != IntSize)
                    {
                        throw new Exception("Failed to read data length.");
                    }

                    int dataLength = BitConverter.ToInt32(lengthBuffer, 0);

                    byte[] dataBuffer = new byte[dataLength];
                    bytesRead = 0;
                    while (bytesRead < dataLength)
                    {
                        bytesRead += await stream.ReadAsync(dataBuffer, bytesRead, dataLength - bytesRead);
                    }

                    string json = Encoding.UTF8.GetString(dataBuffer);

                    BroadcastMessage(json, client);

                    Form.Inst.AddLog("Message : " + json);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
            }
            finally
            {
                DisconnectClient(client);
                client.Close();
                Form.Inst.AddLog("Client disconnected.");
            }
        }

        public void BroadcastMessage(string message, TcpClient sender)
        {
            IPEndPoint? senderEndpoint = sender.Client.RemoteEndPoint as IPEndPoint;

            List<Task> sendTasks = new List<Task>();


            foreach (var item in clientMap)
            {
                if (item.Key == senderEndpoint)
                    continue;

                try
                {
                    NetworkStream stream = item.Value.GetStream();
                    string json = message;
                    byte[] data = Encoding.UTF8.GetBytes(json);

                    byte[] dataLength = BitConverter.GetBytes(data.Length);
                    stream.Write(dataLength, 0, dataLength.Length);
                    stream.Write(data, 0, data.Length);
                    Form.Inst.AddLog($"Send : Client : {item.Key}  message:{message}");

                }
                catch (Exception ex)
                {
                    Form.Inst.AddLog($"Failed to send data to client: {ex.Message}");
                }
            }

        }

        private void DisconnectClient(TcpClient client)
        {
            IPEndPoint endpoint = client.Client.RemoteEndPoint as IPEndPoint;
            if (endpoint != null)
            {
                clientMap.Remove(endpoint);
            }
        }

        public void Start()
        {
            if (isRunning)
                return;
            isRunning = true;
            
            if (server != null) server.Start();
            Form.Inst.AddLog("Server started on " + ipAddress + ":" + port);
            Thread serverThread = new Thread(ServerLoop);
            serverThread.Start();
        }

        public void Stop()
        {
            if (isRunning==false)
                return;
            isRunning = false;
            if (server != null) server.Stop();
            Form.Inst.AddLog("Server stopped.");
        }
    }

}



