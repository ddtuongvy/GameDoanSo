namespace FrmClient
{
    partial class FormClient
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
			this.label1 = new System.Windows.Forms.Label();
			this.grbConnect = new System.Windows.Forms.GroupBox();
			this.btnNgat = new System.Windows.Forms.Button();
			this.btnKetnoi = new System.Windows.Forms.Button();
			this.txtIP = new System.Windows.Forms.TextBox();
			this.label3 = new System.Windows.Forms.Label();
			this.numPort = new System.Windows.Forms.NumericUpDown();
			this.label2 = new System.Windows.Forms.Label();
			this.grbPhong = new System.Windows.Forms.GroupBox();
			this.btnThoat = new System.Windows.Forms.Button();
			this.btnNhap = new System.Windows.Forms.Button();
			this.txtTen = new System.Windows.Forms.TextBox();
			this.btnVao = new System.Windows.Forms.Button();
			this.label6 = new System.Windows.Forms.Label();
			this.btnTao = new System.Windows.Forms.Button();
			this.txtMaphong = new System.Windows.Forms.TextBox();
			this.label4 = new System.Windows.Forms.Label();
			this.grbChoi = new System.Windows.Forms.GroupBox();
			this.btnSansang = new System.Windows.Forms.Button();
			this.btnChoiLai = new System.Windows.Forms.Button();
			this.btnGui = new System.Windows.Forms.Button();
			this.label5 = new System.Windows.Forms.Label();
			this.listBoxKq = new System.Windows.Forms.ListBox();
			this.numericUpDownSo = new System.Windows.Forms.NumericUpDown();
			this.grbConnect.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.numPort)).BeginInit();
			this.grbPhong.SuspendLayout();
			this.grbChoi.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.numericUpDownSo)).BeginInit();
			this.SuspendLayout();
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 28.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(128)))), ((int)(((byte)(0)))));
			this.label1.Location = new System.Drawing.Point(254, 22);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(401, 54);
			this.label1.TabIndex = 0;
			this.label1.Text = "GAME ĐOÁN SỐ";
			// 
			// grbConnect
			// 
			this.grbConnect.BackColor = System.Drawing.SystemColors.Window;
			this.grbConnect.Controls.Add(this.btnNgat);
			this.grbConnect.Controls.Add(this.btnKetnoi);
			this.grbConnect.Controls.Add(this.txtIP);
			this.grbConnect.Controls.Add(this.label3);
			this.grbConnect.Controls.Add(this.numPort);
			this.grbConnect.Controls.Add(this.label2);
			this.grbConnect.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
			this.grbConnect.Location = new System.Drawing.Point(22, 99);
			this.grbConnect.Name = "grbConnect";
			this.grbConnect.Size = new System.Drawing.Size(420, 174);
			this.grbConnect.TabIndex = 1;
			this.grbConnect.TabStop = false;
			this.grbConnect.Text = "KẾT NỐI SERVER";
			// 
			// btnNgat
			// 
			this.btnNgat.BackColor = System.Drawing.Color.Red;
			this.btnNgat.ForeColor = System.Drawing.SystemColors.ButtonHighlight;
			this.btnNgat.Location = new System.Drawing.Point(214, 113);
			this.btnNgat.Name = "btnNgat";
			this.btnNgat.Size = new System.Drawing.Size(153, 46);
			this.btnNgat.TabIndex = 7;
			this.btnNgat.Text = "Ngắt kết nối";
			this.btnNgat.UseVisualStyleBackColor = false;
			this.btnNgat.Click += new System.EventHandler(this.btnNgat_Click);
			// 
			// btnKetnoi
			// 
			this.btnKetnoi.BackColor = System.Drawing.Color.LimeGreen;
			this.btnKetnoi.ForeColor = System.Drawing.SystemColors.ButtonHighlight;
			this.btnKetnoi.Location = new System.Drawing.Point(49, 114);
			this.btnKetnoi.Name = "btnKetnoi";
			this.btnKetnoi.Size = new System.Drawing.Size(126, 43);
			this.btnKetnoi.TabIndex = 6;
			this.btnKetnoi.Text = "Kết nối";
			this.btnKetnoi.UseVisualStyleBackColor = false;
			this.btnKetnoi.Click += new System.EventHandler(this.btnKetnoi_Click);
			// 
			// txtIP
			// 
			this.txtIP.Location = new System.Drawing.Point(243, 55);
			this.txtIP.Name = "txtIP";
			this.txtIP.Size = new System.Drawing.Size(150, 30);
			this.txtIP.TabIndex = 5;
			this.txtIP.Text = "127.0.0.1";
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(196, 58);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(30, 25);
			this.label3.TabIndex = 4;
			this.label3.Text = "IP";
			// 
			// numPort
			// 
			this.numPort.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
			this.numPort.Location = new System.Drawing.Point(70, 56);
			this.numPort.Maximum = new decimal(new int[] {
            65535,
            0,
            0,
            0});
			this.numPort.Name = "numPort";
			this.numPort.Size = new System.Drawing.Size(105, 30);
			this.numPort.TabIndex = 3;
			this.numPort.Value = new decimal(new int[] {
            9000,
            0,
            0,
            0});
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
			this.label2.Location = new System.Drawing.Point(17, 56);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(47, 25);
			this.label2.TabIndex = 2;
			this.label2.Text = "Port";
			// 
			// grbPhong
			// 
			this.grbPhong.BackColor = System.Drawing.SystemColors.Window;
			this.grbPhong.Controls.Add(this.btnThoat);
			this.grbPhong.Controls.Add(this.btnNhap);
			this.grbPhong.Controls.Add(this.txtTen);
			this.grbPhong.Controls.Add(this.btnVao);
			this.grbPhong.Controls.Add(this.label6);
			this.grbPhong.Controls.Add(this.btnTao);
			this.grbPhong.Controls.Add(this.txtMaphong);
			this.grbPhong.Controls.Add(this.label4);
			this.grbPhong.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
			this.grbPhong.Location = new System.Drawing.Point(481, 99);
			this.grbPhong.Name = "grbPhong";
			this.grbPhong.Size = new System.Drawing.Size(516, 174);
			this.grbPhong.TabIndex = 2;
			this.grbPhong.TabStop = false;
			this.grbPhong.Text = "PHÒNG CHƠI";
			// 
			// btnThoat
			// 
			this.btnThoat.BackColor = System.Drawing.SystemColors.Highlight;
			this.btnThoat.ForeColor = System.Drawing.SystemColors.ButtonHighlight;
			this.btnThoat.Location = new System.Drawing.Point(348, 121);
			this.btnThoat.Name = "btnThoat";
			this.btnThoat.Size = new System.Drawing.Size(140, 40);
			this.btnThoat.TabIndex = 4;
			this.btnThoat.Text = "Thoát phòng";
			this.btnThoat.UseVisualStyleBackColor = false;
			this.btnThoat.Click += new System.EventHandler(this.btnThoat_Click);
			// 
			// btnNhap
			// 
			this.btnNhap.BackColor = System.Drawing.SystemColors.ActiveBorder;
			this.btnNhap.Location = new System.Drawing.Point(392, 29);
			this.btnNhap.Name = "btnNhap";
			this.btnNhap.Size = new System.Drawing.Size(91, 37);
			this.btnNhap.TabIndex = 3;
			this.btnNhap.Text = "Nhập";
			this.btnNhap.UseVisualStyleBackColor = false;
			this.btnNhap.Click += new System.EventHandler(this.btnNhap_Click);
			// 
			// txtTen
			// 
			this.txtTen.Location = new System.Drawing.Point(125, 34);
			this.txtTen.Name = "txtTen";
			this.txtTen.Size = new System.Drawing.Size(252, 30);
			this.txtTen.TabIndex = 5;
			// 
			// btnVao
			// 
			this.btnVao.BackColor = System.Drawing.SystemColors.Highlight;
			this.btnVao.ForeColor = System.Drawing.SystemColors.ButtonHighlight;
			this.btnVao.Location = new System.Drawing.Point(175, 123);
			this.btnVao.Name = "btnVao";
			this.btnVao.Size = new System.Drawing.Size(148, 38);
			this.btnVao.TabIndex = 3;
			this.btnVao.Text = "Vào phòng";
			this.btnVao.UseVisualStyleBackColor = false;
			this.btnVao.Click += new System.EventHandler(this.btnVao_Click);
			// 
			// label6
			// 
			this.label6.AutoSize = true;
			this.label6.Location = new System.Drawing.Point(23, 39);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(96, 25);
			this.label6.TabIndex = 4;
			this.label6.Text = "Nhập tên ";
			// 
			// btnTao
			// 
			this.btnTao.BackColor = System.Drawing.SystemColors.Highlight;
			this.btnTao.ForeColor = System.Drawing.SystemColors.ButtonHighlight;
			this.btnTao.Location = new System.Drawing.Point(28, 121);
			this.btnTao.Name = "btnTao";
			this.btnTao.Size = new System.Drawing.Size(120, 38);
			this.btnTao.TabIndex = 2;
			this.btnTao.Text = "Tạo phòng";
			this.btnTao.UseVisualStyleBackColor = false;
			this.btnTao.Click += new System.EventHandler(this.btnTao_Click);
			// 
			// txtMaphong
			// 
			this.txtMaphong.Location = new System.Drawing.Point(197, 83);
			this.txtMaphong.Name = "txtMaphong";
			this.txtMaphong.Size = new System.Drawing.Size(214, 30);
			this.txtMaphong.TabIndex = 1;
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.Location = new System.Drawing.Point(23, 86);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(151, 25);
			this.label4.TabIndex = 0;
			this.label4.Text = "Nhập mã phòng";
			// 
			// grbChoi
			// 
			this.grbChoi.BackColor = System.Drawing.SystemColors.Window;
			this.grbChoi.Controls.Add(this.numericUpDownSo);
			this.grbChoi.Controls.Add(this.btnSansang);
			this.grbChoi.Controls.Add(this.btnChoiLai);
			this.grbChoi.Controls.Add(this.btnGui);
			this.grbChoi.Controls.Add(this.label5);
			this.grbChoi.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
			this.grbChoi.Location = new System.Drawing.Point(22, 279);
			this.grbChoi.Name = "grbChoi";
			this.grbChoi.Size = new System.Drawing.Size(975, 99);
			this.grbChoi.TabIndex = 3;
			this.grbChoi.TabStop = false;
			this.grbChoi.Text = "CHƠI GAME";
			// 
			// btnSansang
			// 
			this.btnSansang.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(128)))), ((int)(((byte)(128)))));
			this.btnSansang.Location = new System.Drawing.Point(807, 34);
			this.btnSansang.Name = "btnSansang";
			this.btnSansang.Size = new System.Drawing.Size(125, 47);
			this.btnSansang.TabIndex = 7;
			this.btnSansang.Text = "Sẵn sàng";
			this.btnSansang.UseVisualStyleBackColor = false;
			this.btnSansang.Click += new System.EventHandler(this.btnSansang_Click);
			// 
			// btnChoiLai
			// 
			this.btnChoiLai.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(128)))), ((int)(((byte)(128)))));
			this.btnChoiLai.Location = new System.Drawing.Point(643, 34);
			this.btnChoiLai.Name = "btnChoiLai";
			this.btnChoiLai.Size = new System.Drawing.Size(125, 47);
			this.btnChoiLai.TabIndex = 6;
			this.btnChoiLai.Text = "Chơi lại";
			this.btnChoiLai.UseVisualStyleBackColor = false;
			this.btnChoiLai.Click += new System.EventHandler(this.btnChoiLai_Click);
			// 
			// btnGui
			// 
			this.btnGui.BackColor = System.Drawing.SystemColors.ActiveBorder;
			this.btnGui.Location = new System.Drawing.Point(495, 37);
			this.btnGui.Name = "btnGui";
			this.btnGui.Size = new System.Drawing.Size(82, 40);
			this.btnGui.TabIndex = 2;
			this.btnGui.Text = "Gửi";
			this.btnGui.UseVisualStyleBackColor = false;
			this.btnGui.Click += new System.EventHandler(this.btnGui_Click);
			// 
			// label5
			// 
			this.label5.AutoSize = true;
			this.label5.Location = new System.Drawing.Point(9, 45);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(199, 25);
			this.label5.TabIndex = 0;
			this.label5.Text = "Nhập số bạn dự đoán";
			// 
			// listBoxKq
			// 
			this.listBoxKq.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
			this.listBoxKq.FormattingEnabled = true;
			this.listBoxKq.ItemHeight = 25;
			this.listBoxKq.Items.AddRange(new object[] {
            "KẾT QUẢ TRÒ CHƠI"});
			this.listBoxKq.Location = new System.Drawing.Point(22, 393);
			this.listBoxKq.Name = "listBoxKq";
			this.listBoxKq.Size = new System.Drawing.Size(976, 179);
			this.listBoxKq.TabIndex = 4;
			// 
			// numericUpDownSo
			// 
			this.numericUpDownSo.Location = new System.Drawing.Point(241, 47);
			this.numericUpDownSo.Name = "numericUpDownSo";
			this.numericUpDownSo.Size = new System.Drawing.Size(218, 30);
			this.numericUpDownSo.TabIndex = 8;
			// 
			// FormClient
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.Color.White;
			this.ClientSize = new System.Drawing.Size(1025, 584);
			this.Controls.Add(this.listBoxKq);
			this.Controls.Add(this.grbChoi);
			this.Controls.Add(this.grbPhong);
			this.Controls.Add(this.grbConnect);
			this.Controls.Add(this.label1);
			this.Name = "FormClient";
			this.Text = "Client";
			this.grbConnect.ResumeLayout(false);
			this.grbConnect.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.numPort)).EndInit();
			this.grbPhong.ResumeLayout(false);
			this.grbPhong.PerformLayout();
			this.grbChoi.ResumeLayout(false);
			this.grbChoi.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.numericUpDownSo)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.GroupBox grbConnect;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button btnNgat;
        private System.Windows.Forms.Button btnKetnoi;
        private System.Windows.Forms.TextBox txtIP;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.NumericUpDown numPort;
        private System.Windows.Forms.GroupBox grbPhong;
        private System.Windows.Forms.Button btnThoat;
        private System.Windows.Forms.Button btnVao;
        private System.Windows.Forms.Button btnTao;
        private System.Windows.Forms.TextBox txtMaphong;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.GroupBox grbChoi;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Button btnGui;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Button btnNhap;
        private System.Windows.Forms.Button btnChoiLai;
        private System.Windows.Forms.TextBox txtTen;
        private System.Windows.Forms.ListBox listBoxKq;
        private System.Windows.Forms.Button btnSansang;
        private System.Windows.Forms.NumericUpDown numericUpDownSo;
    }
}

