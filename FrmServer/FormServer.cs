using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;

namespace FrmServer
{
    public partial class FormServer : Form
    {
        //Dữ liệu cấu trúc hệ thống
        private Socket serverSocket; 
        private bool isServerRunning = false;
        private readonly Random randomGenerator = new Random();

        //Khai báo các Dictionary quản lý trạng thái Client và Room (Chế độ Multiplayer)
        private Dictionary<Socket, string> clientNames = new Dictionary<Socket, string>();
        private Dictionary<string, List<Socket>> gameRooms = new Dictionary<string, List<Socket>>();
        private Dictionary<string, int> roomSecrets = new Dictionary<string, int>();
        private Dictionary<Socket, string> clientRoomMapping = new Dictionary<Socket, string>();
        private HashSet<Socket> readyClients = new HashSet<Socket>(); //Quản lý trạng thái sẵn sàng
		private Dictionary<string, int> guessCounter = new Dictionary<string, int>();//18.1.2026
		private Dictionary<string, Socket> currentTurn = new Dictionary<string, Socket>();
		public FormServer()
        {
            InitializeComponent();

            btnStop.Enabled = false;
            btnStart.Enabled = true;
        }

        //=== Quản lý dịch vụ Server ===
        private void btnStart_Click(object sender, EventArgs e)
        {
            try
            {
                int port = (int)numServerPort.Value;
                //Tạo socket
                serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                serverSocket.Bind(new IPEndPoint(IPAddress.Any, port)); //Bind kết nối
                serverSocket.Listen(10); //Listen

                isServerRunning = true;
                Task.Run(() => AcceptClients()); //Khởi chạy tiến trình AcceptClients() trong một luồng (Thread/Task) riêng biệt

                UpdateServerLogs($"Server khởi động trên port {port}...");
                btnStart.Enabled = false;
                btnStop.Enabled = true;

            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khởi động: " + ex.Message);
            }
        }
        
        private void AcceptClients()
        {
            while (isServerRunning)
            {
                try
                {
                    Socket clientSocket = serverSocket.Accept(); //Nhận kết nối từ CLient
                    UpdateServerLogs($"Client mới kết nối: {clientSocket.RemoteEndPoint}");

                    // Kích hoạt luồng nhận dữ liệu cho Client này
                    Task.Run(() => ReceiveDataPayload(clientSocket));
                }
                catch { break; }
            }
        }

        //=== Xử lý giao giức truyền tải Protocol ===
        private void ReceiveDataPayload(Socket clientSocket)
        {
            byte[] buffer = new byte[1024];
            while (clientSocket.Connected)
            {
                try
                {
                    int size = clientSocket.Receive(buffer);
                    if (size == 0) break;

                    string payload = Encoding.UTF8.GetString(buffer, 0, size).Trim();
                    string[] parts = payload.Split('|');

                    switch (parts[0])
                    {
                        case "LOGIN": // Format: LOGIN|TenNguoiChoi
                            clientNames[clientSocket] = parts[1];
                            UpdateServerLogs($"{parts[1]} đã đăng nhập.");
                            break;

                        case "CREATE_ROOM": // Format: CREATE_ROOM|MaPhong
                            ProcessRoomCreation(clientSocket, parts[1]); //Gọi phương thức xử lý tạo phòng
                            break;

                        case "JOIN_ROOM": // Format: JOIN_ROOM|MaPhong
                            ProcessJoinRoom(clientSocket, parts[1]); //Gọi phương thức xử lý tham gia
                            break;

                        case "READY": // Format: READY
                            ProcessReady(clientSocket); //Gọi phương thức xử lý sẵn sàng
                            break;

                        case "GUESS": // Format: GUESS|SoDuDoan
                            ProcessGuessLogic(clientSocket, parts[1]); //Gọi phương thức xử lý đoán số
                            break;

                        case "LEAVE_ROOM":
                            ProcessLeaveRoom(clientSocket); //Gọi phương thức xử lý thoát phòng
                            break;

                        case "PLAY_AGAIN":
                            ProcessRestartGame(clientSocket); //Gọi phương thức xử lý chơi lại
                            break;
                    }
                }
                catch { break; }
            }
            CleanupClient(clientSocket);
        }
        private void SendResponse(Socket clientSocket, string message)
        {
            try
            {
                //Chuyển đổi thông điệp sang định dạng Byte và truyền tải qua Socket.
                if (clientSocket != null && clientSocket.Connected)
                {
                    byte[] data = Encoding.UTF8.GetBytes(message + "\n");
                    clientSocket.Send(data);
                }
            }
            catch { }
        }

        private void BroadcastToRoom(string roomId, string message)
        {
            if (gameRooms.ContainsKey(roomId))
            {
                foreach (var socket in gameRooms[roomId].ToList())
                {
                    SendResponse(socket, message);
                }
            }
        }

        //Hàm kiểm tra nhập tên
        private bool IsLoggedIn(Socket client)
        {
            return clientNames.ContainsKey(client) && !string.IsNullOrWhiteSpace(clientNames[client]);
        }

        //=== XỬ LÝ LOGIC NGHIỆP VỤ ===
        //Xử lý logic Tạo phòng
        private void ProcessRoomCreation(Socket client, string roomId)
        {
            if (!IsLoggedIn(client))
            {
                SendResponse(client, "ERROR|Bạn chưa đăng nhập tên!");
                return;
            }

            if (!gameRooms.ContainsKey(roomId))
            {
                gameRooms[roomId] = new List<Socket> { client }; //Sinh mã phòng
                roomSecrets[roomId] = randomGenerator.Next(1, 101); // Sinh số bí mật 1-100
                clientRoomMapping[client] = roomId;
				guessCounter[roomId] = 0;
				SendResponse(client, $"ROOM_OK|{roomId}");
                // Gửi thông báo số lượng người chơi hiện tại (1/2)
                string name = clientNames[client]; 
                BroadcastToRoom(roomId, $"INFO|{name} đã tạo phòng. Hiện có 1/2 người chơi.");
                //////////18.1.2026//////////
                UpdateServerLogs($"Phòng {roomId} được tạo. Số bí mật: {roomSecrets[roomId]}");
            }
            else
            {
                SendResponse(client, "ERROR|Mã phòng đã tồn tại!");
            }
        }

        //Xử lý logic Tham gia vào phòng chơi
        private void ProcessJoinRoom(Socket client, string roomId)
        {
            if (!IsLoggedIn(client))
            {
                SendResponse(client, "ERROR|Bạn chưa đăng nhập tên!");
                return;
            }

            if (gameRooms.ContainsKey(roomId))
            {
				if (gameRooms[roomId].Count >= 2)
				{
					SendResponse(client, "ERROR|Phòng đã đầy (2/2)!");
					return;
				}
				gameRooms[roomId].Add(client);
                clientRoomMapping[client] = roomId;
                SendResponse(client, $"ROOM_OK|{roomId}");

                string name = clientNames[client];

                int current = gameRooms[roomId].Count;
				// Thông báo cho cả phòng biết số lượng người (2/2)
				BroadcastToRoom(roomId, $"INFO|{name} đã tham gia. Hiện có {current}/2 người chơi. Ấn Sẵn sàng để bắt đầu!");
				UpdateServerLogs($"{name} vào phòng {roomId}. Hiện có {current}/2 người chơi.");
			}
			else
            {
                SendResponse(client, "ERROR|Phòng không tồn tại!");
            }
        }

		//Xử lý logic sẵn sàng 
		private void ProcessReady(Socket client)
		{
            if (!IsLoggedIn(client))
            {
                SendResponse(client, "ERROR|Bạn chưa nhập tên!");
                return;
            }

            if (!clientRoomMapping.ContainsKey(client)) return;
            string roomId = clientRoomMapping[client];
            readyClients.Add(client);

            //Thông báo sẵn sàng
            string name = clientNames[client];
            BroadcastToRoom(roomId, $"INFO|{name} đã sẵn sàng.");

            int count = gameRooms[roomId].Count;
            // Chế độ 1 người: Sẵn sàng là chơi luôn
            if (count == 1)
            {
                SendResponse(client, "INFO|Chế độ chơi đơn: Bắt đầu đoán số!");
                // Mở khóa nút gửi cho client
                SendResponse(client, "INFO|Đến lượt của bạn");
            }
            // Chế độ 2 người
            else if (count == 2 && gameRooms[roomId].All(c => readyClients.Contains(c)))
            {
                // Chọn người đi trước (người đầu tiên trong danh sách phòng)
                Socket firstPlayer = gameRooms[roomId][0];
                currentTurn[roomId] = firstPlayer;

                // Gửi thông báo bắt đầu ván đấu
                foreach (var s in gameRooms[roomId])
                {
                    if (s == firstPlayer)
                        SendResponse(s, "INFO|Tất cả đã sẵn sàng! Bắt đầu chơi. Đến lượt của bạn đoán.");
                    else
                        SendResponse(s, $"INFO|Tất cả đã sẵn sàng! Bắt đầu chơi. Lượt của {clientNames[firstPlayer]} đoán.");
                }
                UpdateServerLogs($"Phòng {roomId}: Trận đấu bắt đầu. Lượt đầu: {clientNames[firstPlayer]}");
            }
        }

		//Xử lý logic dự đoán số
        private void ProcessGuessLogic(Socket client, string payload)
		{
			if (!clientRoomMapping.ContainsKey(client)) return;
            if (!readyClients.Contains(client)) { SendResponse(client, "ERROR|Bạn chưa Sẵn sàng!"); return; }

            string roomId = clientRoomMapping[client];

            // Nếu 2 người, phải kiểm tra lượt
            if (gameRooms[roomId].Count == 2)
            {
                if (!currentTurn.ContainsKey(roomId) || currentTurn[roomId] != client)
                {
                    SendResponse(client, "ERROR|Chưa tới lượt của bạn!");
                    return;
                }
            }

            if (!int.TryParse(payload, out int guess)) return;
            int secret = roomSecrets[roomId];
            string playerName = clientNames[client];

            if (guess < secret)
            {
                BroadcastToRoom(roomId, $"INFO|{playerName} đoán {guess} -> Nhỏ hơn số bí mật");
                if (gameRooms[roomId].Count == 2) SwitchTurn(roomId, client);
            }
            else if (guess > secret)
            {
                BroadcastToRoom(roomId, $"INFO|{playerName} đoán {guess} -> Lớn hơn số bí mật");
                if (gameRooms[roomId].Count == 2) SwitchTurn(roomId, client);
            }
            else
            {
                BroadcastToRoom(roomId, $"WINNER|{playerName} thắng! Số bí mật là {secret}");
                foreach (var s in gameRooms[roomId]) readyClients.Remove(s);
                currentTurn.Remove(roomId);
            }
        }

		// Hàm phụ để đổi lượt 
		private void SwitchTurn(string roomId, Socket currentClient)
		{
			int index = gameRooms[roomId].IndexOf(currentClient);
			int nextIndex = (index + 1) % gameRooms[roomId].Count;
			Socket nextPlayer = gameRooms[roomId][nextIndex];

			currentTurn[roomId] = nextPlayer;

            SendResponse(currentClient, "INFO|Bạn đã đoán sai, chờ lượt tiếp theo.");
            SendResponse(nextPlayer, "INFO|Đến lượt của bạn đoán!");
        }

		//Xử lý logic Chơi lại
		private void ProcessRestartGame(Socket client)
        {
            if (!clientRoomMapping.ContainsKey(client)) return;
            string roomId = clientRoomMapping[client];

            // Reset trạng thái
            foreach (var s in gameRooms[roomId])
                readyClients.Remove(s);

            currentTurn.Remove(roomId);
            roomSecrets[roomId] = randomGenerator.Next(1, 101);
            BroadcastToRoom(roomId, "INFO|Trận đấu mới đã bắt đầu! Hãy đoán số mới.");
            BroadcastToRoom(roomId, "RESTART_READY");
            UpdateServerLogs($"Phòng {roomId} bắt đầu ván mới. Số bí mật: {roomSecrets[roomId]}");
        }

        //Xử lý logic Thoát phòng
        private void ProcessLeaveRoom(Socket client)
        {
            if (clientRoomMapping.ContainsKey(client))
            {
                string roomId = clientRoomMapping[client];
                string name = clientNames[client];
                gameRooms[roomId].Remove(client);
                clientRoomMapping.Remove(client);
                readyClients.Remove(client);
                if (gameRooms[roomId].Count == 0) { gameRooms.Remove(roomId); roomSecrets.Remove(roomId); }
                else BroadcastToRoom(roomId, $"INFO|{name} đã thoát phòng.");
                UpdateServerLogs($"{name} đã thoát phòng {roomId}.Còn lại {gameRooms[roomId].Count} người.");//18.1.2026
                if (gameRooms[roomId].Count == 1)
                {
                    Socket remain = gameRooms[roomId][0];
                    readyClients.Remove(remain);
                    currentTurn.Remove(roomId);

                    SendResponse(remain, "INFO|Đối thủ đã thoát. Chuyển sang chế độ chơi 1 người.");
                }
            }
        }

        //Ngắt kết nối Server
        private void CleanupClient(Socket client)
        {
            ProcessLeaveRoom(client);
            if (clientNames.ContainsKey(client))
            {
                UpdateServerLogs($"{clientNames[client]} đã ngắt kết nối.");
                clientNames.Remove(client);
            }
            client.Close();
        }

        // Cập nhật nhật ký hệ thống lên giao diện người dùng
        private void UpdateServerLogs(string logMessage)
        {
            if (listBox.InvokeRequired)
                listBox.Invoke(new Action(() => listBox.Items.Add($"[{DateTime.Now:HH:mm:ss}] {logMessage}")));
            else
                listBox.Items.Add($"[{DateTime.Now:HH:mm:ss}] {logMessage}");
        }
        private void btnStop_Click(object sender, EventArgs e)
        {
            try
            {
                isServerRunning = false;
                // Gửi thông báo cho tất cả client trước khi đóng
                foreach (var client in clientNames.Keys.ToList())
                {
                    try
                    {
                        if (client.Connected)
                        {
                            
                            byte[] data = Encoding.UTF8.GetBytes("SERVER_STOPPED|Hệ thống: Server đã dừng dịch vụ.\n");
                            client.Send(data);

                            // Đợi một chút xíu để tin nhắn kịp truyền đi
                            client.Shutdown(SocketShutdown.Both);
                            client.Close();
                        }
                    }
                    catch { }
                }

                clientNames.Clear();
                gameRooms.Clear();
                clientRoomMapping.Clear();
                readyClients.Clear();

                if (serverSocket != null) //Đóng socket tổng
                {
                    serverSocket.Close();
                }

                UpdateServerLogs("Server đã dừng.");
                btnStart.Enabled = true; 
                btnStop.Enabled = false;
            }
            catch (Exception ex)
            {
                UpdateServerLogs("Lỗi khi dừng server: " + ex.Message);
            }
        }
    }
}
