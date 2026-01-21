# DỰ ÁN:GAME ĐOÁN SỐ TCP/IP CLIENT SERVER (C#)

## Giới thiệu
Đây là **ứng dụng game đoán số** được xây dựng theo **kiến trúc Client – Server** sử dụng **TCP/IP Socket** trong **C# WinForms** .
Người chơi kết nối tới Server thông qua **IP & Port**, tham gia phòng chơi và thực hiện đoán số theo lượt.

Dự án được thực hiện phục vụ **môn Lập trình mạng**.

## Hướng dẫn cài đặt & chạy game
### Bước 1: Tải project
Clone từ GitHub:
```bash
git clone https://github.com/ddtuongvy/GameDoanSo.git
```
Hoặc tải file ZIP và giải nén.
 ### Bước 2: Mở project
 - Mở file ```  Game_Doan_so.sln ``` bằng **Visual Studio**
 ### Bước 3: Chạy Server
 1. Chuột phải project **FrmServer**
2. Chọn **Set as Startup Project**
3. Nhấn **Run**
Server bắt đầu lắng nghe kết nối từ Client qua TCP/IP.
### Bước 4: Chạy Client
1. Chuột phải project **FrmClient**
2. Chọn **Set as Startup Project**
3. Nhấn **Run**
4. Nhập thông tin kết nối:
- **IP Server**: 127.0.0.1
- **Port**: 8888
5. Nhấn **Kết nối**
Có thể mở **nhiều Client** để chơi cùng lúc.

## Chức năng chính
### Server
- Lắng nghe kết nối từ nhiều Client qua TCP/IP
- Quản lý danh sách người chơi
- Tạo và quản lý phòng chơi
- Sinh số ngẫu nhiên cho mỗi ván
- Điều khiển lượt chơi
- Kiểm tra kết quả đoán số (đúng / sai)
- Xử lý chơi lại, thoát phòng, ngắt kết nối
### Client
- Kết nối Server bằng IP & Port
- Nhập tên người chơi
- Tạo phòng / vào phòng
- Sẵn sàng chơi
- Gửi số dự đoán
- Nhận kết quả (đúng / sai)
- Chơi lại
- Thoát phòng
- Ngắt kết nối Server

## KIến trúc & giao thức truyền thông
Ứng dụng được xây dựng theo **mô hình Client – Server**, sử dụng **TCP/IP Socket** để giao tiếp giữa các thành phần.

    Client (WinForms)
        |
        | TCP/IP Socket
        |
    Server (WinForms)

- Giao thức: **TCP**
- Mô hình: **Client – Server**
- Trao đổi dữ liệu dạng chuỗi (UTF-8)
- Cú pháp lệnh: ``` COMMAND|DATA ```

### Bảng mô tả các lệnh chính

| Lệnh | Dữ liệu | Ý nghĩa |
|-----|--------|--------|
| LOGIN | Tên người chơi | Client đăng nhập tên người chơi |
| LOGIN_OK | Tên người chơi | Server xác nhận đăng nhập thành công |
| CREATE_ROOM | Mã phòng | Tạo phòng chơi mới |
| JOIN_ROOM | Mã phòng | Tham gia phòng có sẵn |
| ROOM_OK | Mã phòng | Server xác nhận vào phòng thành công |
| READY | - | Báo trạng thái sẵn sàng |
| GUESS | Số dự đoán | Gửi số dự đoán |
| PLAY_AGAIN | - | Yêu cầu chơi lại |
| LEAVE_ROOM | - | Thoát phòng |
| INFO | Nội dung | Thông báo trạng thái từ Server |
| WINNER | Tên người thắng | Công bố người chiến thắng |
| ERROR | Nội dung lỗi | Thông báo lỗi |
| RESTART_READY | - | Chuẩn bị cho ván chơi mới |
| SERVER_STOPPED | Nội dung | Server dừng dịch vụ |

## 📂 Cấu trúc thư mục

```
Game_Doan_so/
├── FrmClient/                 # Project Client (WinForms)
│   ├── Properties/
│   ├── App.config
│   ├── FormClient.cs
│   ├── FormClient.Designer.cs
│   ├── FormClient.resx
│   ├── Program.cs
│   └── FrmClient.csproj
│
├── FrmServer/                 # Project Server (WinForms)
│   ├── Properties/
│   ├── App.config
│   ├── FormServer.cs
│   ├── FormServer.Designer.cs
│   ├── FormServer.resx
│   ├── Program.cs
│   └── FrmServer.csproj
│
├── Game_Doan_so.sln            # File solution Visual Studio
├── .gitignore                 # Loại trừ file không cần push Git
├── README.md                  # Mô tả dự án
└── fix.txt                    # Ghi chú chỉnh sửa
```

## Công nghệ sử dụng
- Ngôn ngữ: **C#**
- Framework: **.NET (WinForms)**
- Mạng: **System.Net.Sockets**
- IDE: **Visual Studio**
- Quản lý mã nguồn: **Git / GitHub**



## Luồng chơi game
1. Client kết nối Server
2. Nhập tên người chơi
3. Tạo phòng hoặc vào phòng
4. Nhấn **Sẵn sàng**
5. Server bắt đầu ván chơi
6. Người chơi đoán số theo lượt
7. Server thông báo đúng / sai
8. Kết thúc ván → Chơi lại hoặc thoát

## Phạm vi & giới hạn
- Game hỗ trợ tối đa **2 người chơi / phòng**
- Chưa hỗ trợ bảo mật dữ liệu
- Phù hợp cho mục đích học tập và demo kiến trúc mạng

## Tác giả
- Đào Đoàn Tường Vy - 052305007740
- Trần Khánh Ngân - 05230500