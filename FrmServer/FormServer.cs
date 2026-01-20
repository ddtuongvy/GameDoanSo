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

        //=== XỬ LÝ LOGIC NGHIỆP VỤ ===
        //Xử lý logic Tạo phòng
        private void ProcessRoomCreation(Socket client, string roomId)
        {
            if (!gameRooms.ContainsKey(roomId))
            {
                gameRooms[roomId] = new List<Socket> { client }; //Sinh mã phòng
                roomSecrets[roomId] = randomGenerator.Next(1, 101); // Sinh số bí mật 1-100
                clientRoomMapping[client] = roomId;
				guessCounter[roomId] = 0;
				SendResponse(client, $"ROOM_OK|{roomId}");
				// Gửi thông báo số lượng người chơi hiện tại (1/2)
				string name = clientNames.ContainsKey(client) ? clientNames[client] : "Ẩn danh";
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

                string name = clientNames.ContainsKey(client) ? clientNames[client] : "Ẩn danh";
				
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
			if (readyClients.Contains(client)) return;
			string roomId = clientRoomMapping[client];
			readyClients.Add(client);

			//Thông báo sẵn sàng
			string name = clientNames.ContainsKey(client) ? clientNames[client] : "Ẩn danh";
			BroadcastToRoom(roomId, $"INFO|{name} đã sẵn sàng.");

			// Kiểm tra nếu đủ 2 người và cả 2 đều sẵn sàng
			if (gameRooms[roomId].Count == 2 && gameRooms[roomId].All(c => readyClients.Contains(c)))
			{
				// Chọn người đi trước (người đầu tiên trong danh sách phòng)
				Socket firstPlayer = gameRooms[roomId][0];
				currentTurn[roomId] = firstPlayer;

				// Gửi thông báo bắt đầu ván đấu
				foreach (var s in gameRooms[roomId])
				{
					if (s == firstPlayer)
						SendResponse(s, "INFO|Tất cả đã sẵn sàng! Bắt đầu chơi. Lượt của BẠN đoán.");
					else
						SendResponse(s, $"INFO|Tất cả đã sẵn sàng! Bắt đầu chơi. Lượt của {clientNames[firstPlayer]} đoán.");
				}
				UpdateServerLogs($"Phòng {roomId}: Trận đấu bắt đầu. Lượt đầu: {clientNames[firstPlayer]}");
			}
		}

		//Xử lý logic dự đoán số
		// Xử lý logic dự đoán số
private void ProcessGuessLogic(Socket client, string payload)
		{
			if (!clientRoomMapping.ContainsKey(client)) return;
			string roomId = clientRoomMapping[client];

			// KIỂM TRA PHÒNG ĐỦ NGƯỜI
			if (gameRooms[roomId].Count < 2)
			{
				SendResponse(client, "ERROR|Phòng chưa đủ 2 người chơi!");
				return;
			}

			// KIỂM TRA READY
			if (!readyClients.Contains(client))
			{
				SendResponse(client, "ERROR|Bạn chưa bấm Sẵn sàng!");
				return;
			}

			// KIỂM TRA LƯỢT
			if (!currentTurn.ContainsKey(roomId) || currentTurn[roomId] != client)
			{
				SendResponse(client, "ERROR|Chưa tới lượt của bạn!");
				return;
			}

			// KIỂM TRA SỐ ĐOÁN
			if (!int.TryParse(payload, out int guess))
			{
				SendResponse(client, "ERROR|Giá trị đoán không hợp lệ!");
				return;
			}

			int secret = roomSecrets[roomId];
			string playerName = clientNames.ContainsKey(client) ? clientNames[client] : "Ẩn danh";

			string compare = guess < secret ? "nhỏ hơn số bí mật"
						   : guess > secret ? "lớn hơn số bí mật"
						   : "CHÍNH XÁC!";

			// Gửi cho tất cả người chơi kết quả đoán
			foreach (var p in gameRooms[roomId])
			{
				if (guess == secret)
					SendResponse(p, $"GUESS_RESULT|{playerName} đoán {guess} → {compare}");
				else if (p == client)
					SendResponse(p, $"GUESS_RESULT|Bạn đoán {guess} → {compare}");
				else
					SendResponse(p, $"GUESS_RESULT|{playerName} đoán {guess} → {compare}");
			}

			// THẮNG CUỘC
			if (guess == secret)
			{
				BroadcastToRoom(roomId, $"WINNER|{playerName} đã chiến thắng! Số bí mật là {secret}.");
				UpdateServerLogs($"Phòng {roomId}: {playerName} thắng.");

				// RESET
				foreach (var s in gameRooms[roomId]) readyClients.Remove(s);
				currentTurn.Remove(roomId);
				return;
			}

			// CHUYỂN LƯỢT
			SwitchTurn(roomId, client);
		}

		// Hàm phụ để đổi lượt 
		private void SwitchTurn(string roomId, Socket currentClient)
		{
			int index = gameRooms[roomId].IndexOf(currentClient);
			int nextIndex = (index + 1) % gameRooms[roomId].Count;
			Socket nextPlayer = gameRooms[roomId][nextIndex];

			currentTurn[roomId] = nextPlayer;

			SendResponse(currentClient, "INFO|Bạn đã đoán sai, chờ lượt tiếp theo.");
			SendResponse(nextPlayer, "INFO|Tới lượt bạn đoán!");
		}

		//Xử lý logic Chơi lại
		private void ProcessRestartGame(Socket client)
        {
            if (!clientRoomMapping.ContainsKey(client)) return; 
            string roomId = clientRoomMapping[client];
			//Reset trạng thái sẵn sàng cho toàn bộ người trong phòng
			foreach (var s in gameRooms[roomId].ToList())
			{
				readyClients.Remove(s);
			}
			//Reset các thông số game
			currentTurn.Remove(roomId);
			roomSecrets[roomId] = randomGenerator.Next(1, 101);
			guessCounter[roomId] = 0; // Reset số lần đoán nếu cần

			//Thông báo cho Client biết để mở lại nút "Sẵn sàng"
			BroadcastToRoom(roomId, "RESTART_READY");
			BroadcastToRoom(roomId, "INFO|Ván mới đã sẵn sàng. Vui lòng bấm Sẵn sàng để bắt đầu!");

			UpdateServerLogs($"Phòng {roomId} đã reset ván mới.");
		}

        //Xử lý logic Thoát phòng
        private void ProcessLeaveRoom(Socket client)
        {
            if (clientRoomMapping.ContainsKey(client))
            {
                string roomId = clientRoomMapping[client];
                string name = clientNames.ContainsKey(client) ? clientNames[client] : "Ẩn danh";
                gameRooms[roomId].Remove(client);
                clientRoomMapping.Remove(client);
                readyClients.Remove(client);
                if (gameRooms[roomId].Count == 0) { gameRooms.Remove(roomId); roomSecrets.Remove(roomId); }
                else BroadcastToRoom(roomId, $"INFO|{name} đã thoát phòng.");
                UpdateServerLogs($"{name} đã thoát phòng {roomId}.Còn lại {gameRooms[roomId].Count} người.");//18.1.2026

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
                        SendResponse(client, "INFO|Server đang dừng dịch vụ...");
                        client.Shutdown(SocketShutdown.Both);
                        client.Close();
                    }
                    catch { }
                }

                clientNames.Clear();
                gameRooms.Clear();
                clientRoomMapping.Clear();
                readyClients.Clear();

                if (serverSocket != null)
                {
                    serverSocket.Close();
                }

                UpdateServerLogs("Server đã dừng.");
                btnStart.Enabled = true; // Cho phép bấm Start lại
            }
            catch (Exception ex)
            {
                UpdateServerLogs("Lỗi khi dừng server: " + ex.Message);
            }
        }
    }
}
