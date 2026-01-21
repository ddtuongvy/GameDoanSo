using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;

namespace FrmClient
{
    public partial class FormClient : Form
    {
        Socket clientSocket;
        bool isConnected = false;
        public FormClient()
        {
            InitializeComponent();
        }
        private void FormClient_Load(object sender, EventArgs e)
        {
            // Chỉ cho phép kết nối trước
            btnKetnoi.Enabled = true;

            // Khóa tất cả các nút khác
            btnNhap.Enabled = false;
            btnTao.Enabled = false;
            btnVao.Enabled = false;
            btnThoat.Enabled = false;
            btnSansang.Enabled = false;
            btnGui.Enabled = false;
            btnChoiLai.Enabled = false;
            btnNgat.Enabled = false;
        }

        private void btnKetnoi_Click(object sender, EventArgs e)
        {
            try
            {
                clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                clientSocket.Connect(txtIP.Text, (int)numPort.Value);
                isConnected = true;
                Task.Run(() => ReceiveFromServer());
                btnKetnoi.Enabled = false;
                btnTao.Enabled = true;
                btnVao.Enabled = true;
                btnNgat.Enabled = true;   
                btnNhap.Enabled = true;
                MessageBox.Show("Kết nối thành công!");
            }
            catch (Exception ex) { MessageBox.Show("Lỗi: " + ex.Message); }
        }

        private void btnNhap_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtTen.Text))
            {
                MessageBox.Show("Vui lòng nhập tên trước khi tham gia!");
                return;
            }
            Send("LOGIN|" + txtTen.Text);
        }

        private void btnTao_Click(object sender, EventArgs e) 
        {
            if (string.IsNullOrWhiteSpace(txtTen.Text))
            {
                MessageBox.Show("Bạn chưa nhập tên!");
                return;
            }
            Send("CREATE_ROOM|" + txtMaphong.Text); 
        }
        private void btnVao_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtTen.Text))
            {
                MessageBox.Show("Bạn chưa nhập tên!");
                return;
            }
            Send("JOIN_ROOM|" + txtMaphong.Text);
        }
        private void btnThoat_Click(object sender, EventArgs e)
        {
            Send("LEAVE_ROOM");
            btnTao.Enabled = true;
            btnVao.Enabled = true;
            btnThoat.Enabled = false;
            btnSansang.Enabled = false;
            btnGui.Enabled = false;
            btnChoiLai.Enabled = false;

            txtMaphong.Clear();
            listBoxKq.Items.Add("[Hệ thống]: Bạn đã rời phòng.");
        }
        private void btnSansang_Click(object sender, EventArgs e)
        {
            Send("READY");
            // Disable Ready immediately to prevent multiple clicks while waiting/playing
            btnSansang.Enabled = false;
        }
        private void btnGui_Click(object sender, EventArgs e) 
        { 
            Send("GUESS|" + numericUpDownSo.Text);
        }
        private void btnChoiLai_Click(object sender, EventArgs e)
        {
            listBoxKq.Items.Clear();
            Send("PLAY_AGAIN");
        }

        private void Send(string msg)
        {
            try
            {
                if (clientSocket != null && clientSocket.Connected)
                    clientSocket.Send(Encoding.UTF8.GetBytes(msg));
            }
            catch (SocketException)
            {
                MessageBox.Show("Mất kết nối tới server!", "Lỗi");
                isConnected = false;
            }
        }

        private void ReceiveFromServer()
        {
            byte[] buffer = new byte[1024];
            while (isConnected)
            {
                try
                {
                    int size = clientSocket.Receive(buffer);
                    if (size == 0) break;

                    string rawMsg = Encoding.UTF8.GetString(buffer, 0, size);

                    // Tách các tin nhắn bị dính nhau bởi dấu \n
                    string[] messages = rawMsg.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);

                    foreach (string msg in messages)
                    {
                        this.Invoke((Action)(() => ParseMessage(msg.Trim())));
                    }
                }
                catch { break; }
            }
        }

        private void ParseMessage(string msg)
        {
            string[] parts = msg.Split('|');
            switch (parts[0])
            {
                case "LOGIN_OK":
                    // đăng nhập thành công -> khóa tên và nút nhập cho đến khi ngắt kết nối
                    string name = parts.Length > 1 ? parts[1] : txtTen.Text;
                    MessageBox.Show($"Đăng nhập thành công: {name}", "Thông báo");
                    txtTen.Enabled = false;
                    btnNhap.Enabled = false;
                    break;

                case "ROOM_OK":
                    btnTao.Enabled = false;
                    btnVao.Enabled = false;
                    btnThoat.Enabled = true;   
                    btnSansang.Enabled = true; 
                    txtMaphong.Text = parts[1];

                    listBoxKq.Items.Add("Đã vào phòng: " + parts[1]);
                    listBoxKq.SelectedIndex = listBoxKq.Items.Count - 1;
                    break;

                case "WINNER":
                    MessageBox.Show(parts[1], "Kết thúc ván chơi");
                    btnGui.Enabled = false;      // Khóa nút gửi số
                    btnChoiLai.Enabled = true; //Hiện nút chơi lại
                    btnSansang.Enabled = false;// Khóa nút sẵn sàng
                    break;

                case "RESTART_READY":
                    btnSansang.Enabled = true;
                    btnChoiLai.Enabled = false;
                    btnGui.Enabled = false;
                    break;

                case "INFO":
                    string infoMsg = parts[1];
                    listBoxKq.Items.Add("[Hệ thống]: " + parts[1]);
                    listBoxKq.SelectedIndex = listBoxKq.Items.Count - 1;
                    string lowMsg = infoMsg.ToLower();

                    // Nếu đối thủ đã thoát và chuyển sang chế độ chơi đơn => mở lại nút Sẵn sàng để người ở lại có thể bắt đầu
                    if (lowMsg.Contains("đối thủ đã thoát") || lowMsg.Contains("chuyển sang chế độ chơi 1 người"))
                    {
                        btnSansang.Enabled = true;
                    }

                    if (lowMsg.Contains("đến lượt của bạn"))
                    {
                        btnGui.Enabled = true;
                        btnGui.BackColor = Color.LightGreen;
                        // Khóa nút Sẵn sàng khi đang đoán
                        btnSansang.Enabled = false;
                    }
                    else if (lowMsg.Contains("lượt của")|| lowMsg.Contains("chờ lượt"))
                    {
                        btnGui.Enabled = false;
                        btnGui.BackColor = SystemColors.Control;//Trả về màu mắc định
                        // Đối phương đang đoán -> cũng khóa Ready
                        btnSansang.Enabled = false;
                    }
                    else if (lowMsg.Contains("chế độ chơi đơn") || lowMsg.Contains("bắt đầu chơi"))
                    {
                        // Khi trận đấu bắt đầu (1 hoặc 2 người), khóa Ready
                        btnSansang.Enabled = false;
                    }

                    break;

                case "SERVER_STOPPED":
                    MessageBox.Show(parts[1], "Hệ thống");
                    Disconnect();
                    break;

                case "ERROR": MessageBox.Show(parts[1], "Lỗi"); break;
            }
        }

        // Nút Ngắt kết nối 
        private void btnNgat_Click(object sender, EventArgs e)
        {
            Disconnect();
        }

        private void Disconnect()
        {
            try
            {
                if (isConnected)
                {
                    isConnected = false;
                    if (clientSocket != null)
                    {
                        clientSocket.Shutdown(SocketShutdown.Both);
                        clientSocket.Close();
                    }
                    listBoxKq.Items.Add("Đã ngắt kết nối với Server.");
                    listBoxKq.SelectedIndex = listBoxKq.Items.Count - 1;
                    btnKetnoi.Enabled = true;
                    btnNgat.Enabled = false; 
                    btnTao.Enabled = btnVao.Enabled = btnThoat.Enabled = btnGui.Enabled = btnChoiLai.Enabled = btnSansang.Enabled = false;
                    // Khi ngắt kết nối: cho phép nhập tên lại
                    txtTen.Enabled = true;
                    btnNhap.Enabled = false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi ngắt kết nối: " + ex.Message);
            }
        }

        // Xử lý khi người dùng bấm dấu [X] trên cửa sổ
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            Disconnect();
            base.OnFormClosing(e);
        }
    }
}

