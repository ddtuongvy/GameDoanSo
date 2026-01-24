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
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ProgressBar;

namespace FrmServer
{
    public partial class FormServer : Form
    {
        //===== KHAI BÁO BIẾN TOÀN CỤC =====
        private Socket serverSocket; //Socket chính để listen client kết nối
        private bool isServerRunning = false; // Cờ kiểm soát server đang chạy hay không
        private readonly Random randomGenerator = new Random(); // Sinh số bí mật ngẫu nhiên cho mỗi phòng

        //===== CẤU TRÚC DỮ LIỆU QUẢN LÝ GAME =====
        private Dictionary<Socket, string> clientNames = new Dictionary<Socket, string>(); //Quản lý lưu tên người chơi theo socket
        private Dictionary<string, List<Socket>> gameRooms = new Dictionary<string, List<Socket>>(); //Quản lý phòng và các socket trong phòng
        private Dictionary<string, int> roomSecrets = new Dictionary<string, int>(); //Lưu số bí mật mỗi phòng
        private Dictionary<Socket, string> clientRoomMapping = new Dictionary<Socket, string>(); //Quản lý người chơi ở phòng nào
        private HashSet<Socket> readyClients = new HashSet<Socket>(); //Lưu ds client đã sẵn sàng
		private Dictionary<string, Socket> currentTurn = new Dictionary<string, Socket>(); //Quản lý lượt đoán chế độ 2 người
		
        public FormServer()
        {
            InitializeComponent();

            btnStop.Enabled = false;
            btnStart.Enabled = true;
        }

        //===== KHỞI ĐỘNG SERVER =====
        private void btnStart_Click(object sender, EventArgs e)
        {
            try
            {
                int port = (int)numServerPort.Value;
                //Tạo socket
                serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                //Gắn kết socket đến port
                serverSocket.Bind(new IPEndPoint(IPAddress.Any, port)); 
                //Tạo hàng đợi
                serverSocket.Listen(10); 

                isServerRunning = true;
                //Chạy luồng (Thread/Task) phụ để chấp nhận client kết nối, tránh treo UI
                Task.Run(() => AcceptClients()); 

                UpdateServerLogs($"Server khởi động trên port {port}...");
                btnStart.Enabled = false;
                btnStop.Enabled = true;

            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khởi động: " + ex.Message);
            }
        }
        
        //===== CHẤP NHẬN CLIENT =====
        private void AcceptClients()
        {
            while (isServerRunning)
            {
                try
                {
                    Socket clientSocket = serverSocket.Accept(); //Nhận kết nối từ CLient
                    UpdateServerLogs($"Client mới kết nối: {clientSocket.RemoteEndPoint}");

                    //Mỗi client có 1 luồng nhận dữ liệu riêng
                    Task.Run(() => ReceiveDataPayload(clientSocket));
                }
                catch { break; }
            }
        }

        //===== NHẬN VÀ XỬ LÝ GIAO THỨC TRUYỂN TẢI PROTOCOL =====
        private void ReceiveDataPayload(Socket clientSocket)
        {
            byte[] buffer = new byte[1024]; //buffer nhận 1KB
            while (clientSocket.Connected)
            {
                try
                {
                    //Server nhận dữ liệu từ client
                    int size = clientSocket.Receive(buffer);
                    if (size == 0) break;
                    //Chuyển byte -> chuỗi UTF8
                    string payload = Encoding.UTF8.GetString(buffer, 0, size).Trim();
                    string[] parts = payload.Split('|'); //Protocol: COMMAND|DATA

                    switch (parts[0])
                    {
                        case "LOGIN":  //LOGIN|TenNguoiChoi
                            string playerName = parts.Length > 1 ? parts[1] : string.Empty;
                            clientNames[clientSocket] = playerName;

                            UpdateServerLogs($"{playerName} đã đăng nhập.");
                            SendResponse(clientSocket, $"LOGIN_OK|{playerName}");
                            SendResponse(clientSocket, $"INFO|Bạn đã đăng nhập thành công, chào {playerName}.");                   
                            break;

                        case "CREATE_ROOM": //CREATE_ROOM|MaPhong
                            ProcessRoomCreation(clientSocket, parts[1]); //Gọi phương thức xử lý tạo phòng
                            break;

                        case "JOIN_ROOM": //JOIN_ROOM|MaPhong
                            ProcessJoinRoom(clientSocket, parts[1]); //Gọi phương thức xử lý tham gia
                            break;

                        case "READY": //READY
                            ProcessReady(clientSocket); //Gọi phương thức xử lý sẵn sàng
                            break;

                        case "GUESS": //GUESS|SoDuDoan
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

        //===== GỬI DỮ LIỆU CHO CLIENT =====
        private void SendResponse(Socket clientSocket, string message)
        {
            try
            {
                if (clientSocket != null && clientSocket.Connected)
                {
                    //Mỗi message kết thúc bằng \n để client tách gói tin
                    byte[] data = Encoding.UTF8.GetBytes(message + "\n");
                    clientSocket.Send(data);
                }
            }
            catch { }
        }

        // Gửi thông báo cho tất cả client trong phòng
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

        //===== XỬ LÝ LOGIC NGHIỆP VỤ =====
        //=== LOGIC TẠO PHÒNG ===
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
				SendResponse(client, $"ROOM_OK|{roomId}");
                // Gửi thông báo số lượng người chơi hiện tại (1/2)
                string name = clientNames[client]; 
                BroadcastToRoom(roomId, $"INFO|{name} đã tạo phòng. Hiện có 1/2 người chơi.");
                UpdateServerLogs($"Phòng {roomId} được tạo. Số bí mật: {roomSecrets[roomId]}");
            }
            else
            {
                SendResponse(client, "ERROR|Mã phòng đã tồn tại!");
            }
        }

        //=== LOGIC VÀO PHÒNG ===
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

        //=== LOGIC SẴN SÀNG ===
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

        //=== LOGIC ĐOÁN SỐ ===
        private void ProcessGuessLogic(Socket client, string payload)
        {
            if (!clientRoomMapping.ContainsKey(client)) return;
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
            //Tìm vị trí (index) của người chơi hiện tại trong danh sách phòng
            int index = gameRooms[roomId].IndexOf(currentClient);
            // Công thức vòng tròn: (vị trí hiện tại + 1) chia dư cho tổng số người
            int nextIndex = (index + 1) % gameRooms[roomId].Count;
            Socket nextPlayer = gameRooms[roomId][nextIndex];

            currentTurn[roomId] = nextPlayer; //Cập nhật lượt mới vào Dictionary

            SendResponse(currentClient, "INFO|Bạn đã đoán sai, chờ lượt tiếp theo.");
            SendResponse(nextPlayer, "INFO|Đến lượt của bạn đoán!");
        }

        //=== LOGIC CHƠI LẠI ===
        private void ProcessRestartGame(Socket client)
        {
            if (!clientRoomMapping.ContainsKey(client)) return;
            string roomId = clientRoomMapping[client];
            // Reset trạng thái
            foreach (var s in gameRooms[roomId])
                readyClients.Remove(s);

            currentTurn.Remove(roomId); //Reset lượt đoán
            roomSecrets[roomId] = randomGenerator.Next(1, 101); //Tạo số bí mật mới

            BroadcastToRoom(roomId, "INFO|Trận đấu mới đã bắt đầu! Hãy đoán số mới.");
            BroadcastToRoom(roomId, "RESTART_READY");
            UpdateServerLogs($"Phòng {roomId} bắt đầu ván mới. Số bí mật: {roomSecrets[roomId]}");
        }

        //=== LOGIC THOÁT PHÒNG ===
        private void ProcessLeaveRoom(Socket client)
        {
            if (clientRoomMapping.ContainsKey(client))
            {
                string roomId = clientRoomMapping[client];
                string name = clientNames[client];

                if (gameRooms.ContainsKey(roomId))
                {
                    //Xóa người chơi ra khỏi danh sách phòng
                    gameRooms[roomId].Remove(client);
                    clientRoomMapping.Remove(client);
                    readyClients.Remove(client);

                    //Kiểm tra xem phòng còn ai không
                    if (gameRooms[roomId].Count == 0)
                    {
                        // Nếu không còn ai, xóa sạch dữ liệu phòng và kết thúc hàm ở đây
                        gameRooms.Remove(roomId);
                        roomSecrets.Remove(roomId);
                        if (currentTurn.ContainsKey(roomId)) currentTurn.Remove(roomId);

                        UpdateServerLogs($"{name} đã thoát. Phòng {roomId} đã được xóa.");
                        return;
                    }

                    else 
                    {
                        //Nếu còn 1 người sẽ chuyển sang chơi 1 người
                        BroadcastToRoom(roomId, $"INFO|{name} đã thoát phòng.");
                        UpdateServerLogs($"{name} đã thoát phòng {roomId}.Còn lại {gameRooms[roomId].Count} người.");

                        if (gameRooms[roomId].Count == 1)
                        {
                            Socket remain = gameRooms[roomId][0];
                            readyClients.Remove(remain);
                            currentTurn.Remove(roomId);

                            SendResponse(remain, "INFO|Đối thủ đã thoát. Chuyển sang chế độ chơi 1 người.");
                        }
                    }
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

        //Cập nhật log server 
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

                            // Đợi để tin nhắn kịp truyền đi
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

                if (serverSocket != null) 
                {
                    serverSocket.Close(); //Đóng socket tổng
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
