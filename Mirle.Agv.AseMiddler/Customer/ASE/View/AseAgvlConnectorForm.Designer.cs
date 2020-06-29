namespace Mirle.Agv.AseMiddler.View
{
    partial class AseAgvlConnectorForm
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
            this.components = new System.ComponentModel.Container();
            this.btnHide = new System.Windows.Forms.Button();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.InfoPage = new System.Windows.Forms.TabPage();
            this.SingleCommandPage = new System.Windows.Forms.TabPage();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.numPsMessageNumber = new System.Windows.Forms.NumericUpDown();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.btnSingleMessageSend = new System.Windows.Forms.Button();
            this.cbPsMessageType = new System.Windows.Forms.ComboBox();
            this.txtPsMessageText = new System.Windows.Forms.TextBox();
            this.btnSaveAutoReplyMessage = new System.Windows.Forms.Button();
            this.cbPspMessageList = new System.Windows.Forms.ComboBox();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.tabControl1.SuspendLayout();
            this.SingleCommandPage.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numPsMessageNumber)).BeginInit();
            this.SuspendLayout();
            // 
            // btnHide
            // 
            this.btnHide.Location = new System.Drawing.Point(1050, 12);
            this.btnHide.Name = "btnHide";
            this.btnHide.Size = new System.Drawing.Size(117, 46);
            this.btnHide.TabIndex = 0;
            this.btnHide.Text = "X";
            this.btnHide.UseVisualStyleBackColor = true;
            this.btnHide.Click += new System.EventHandler(this.btnHide_Click);
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.InfoPage);
            this.tabControl1.Controls.Add(this.SingleCommandPage);
            this.tabControl1.Location = new System.Drawing.Point(12, 12);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(1032, 389);
            this.tabControl1.TabIndex = 1;
            // 
            // InfoPage
            // 
            this.InfoPage.Location = new System.Drawing.Point(4, 22);
            this.InfoPage.Name = "InfoPage";
            this.InfoPage.Padding = new System.Windows.Forms.Padding(3);
            this.InfoPage.Size = new System.Drawing.Size(1024, 363);
            this.InfoPage.TabIndex = 0;
            this.InfoPage.Text = "Info";
            this.InfoPage.UseVisualStyleBackColor = true;
            // 
            // SingleCommandPage
            // 
            this.SingleCommandPage.Controls.Add(this.groupBox2);
            this.SingleCommandPage.Location = new System.Drawing.Point(4, 22);
            this.SingleCommandPage.Name = "SingleCommandPage";
            this.SingleCommandPage.Padding = new System.Windows.Forms.Padding(3);
            this.SingleCommandPage.Size = new System.Drawing.Size(1024, 363);
            this.SingleCommandPage.TabIndex = 1;
            this.SingleCommandPage.Text = "SingleCommand";
            this.SingleCommandPage.UseVisualStyleBackColor = true;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.tableLayoutPanel2);
            this.groupBox2.Location = new System.Drawing.Point(6, 8);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(710, 345);
            this.groupBox2.TabIndex = 7;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Single Message";
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.ColumnCount = 2;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 30F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 70F));
            this.tableLayoutPanel2.Controls.Add(this.numPsMessageNumber, 1, 2);
            this.tableLayoutPanel2.Controls.Add(this.label2, 0, 1);
            this.tableLayoutPanel2.Controls.Add(this.label3, 0, 2);
            this.tableLayoutPanel2.Controls.Add(this.label4, 0, 3);
            this.tableLayoutPanel2.Controls.Add(this.btnSingleMessageSend, 1, 4);
            this.tableLayoutPanel2.Controls.Add(this.cbPsMessageType, 1, 1);
            this.tableLayoutPanel2.Controls.Add(this.txtPsMessageText, 1, 3);
            this.tableLayoutPanel2.Controls.Add(this.btnSaveAutoReplyMessage, 0, 4);
            this.tableLayoutPanel2.Controls.Add(this.cbPspMessageList, 1, 0);
            this.tableLayoutPanel2.Location = new System.Drawing.Point(6, 21);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 5;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(698, 318);
            this.tableLayoutPanel2.TabIndex = 0;
            // 
            // numPsMessageNumber
            // 
            this.numPsMessageNumber.Dock = System.Windows.Forms.DockStyle.Fill;
            this.numPsMessageNumber.Font = new System.Drawing.Font("新細明體", 20.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.numPsMessageNumber.Location = new System.Drawing.Point(212, 129);
            this.numPsMessageNumber.Maximum = new decimal(new int[] {
            99,
            0,
            0,
            0});
            this.numPsMessageNumber.Name = "numPsMessageNumber";
            this.numPsMessageNumber.Size = new System.Drawing.Size(483, 40);
            this.numPsMessageNumber.TabIndex = 8;
            this.numPsMessageNumber.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label2.Location = new System.Drawing.Point(3, 63);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(203, 63);
            this.label2.TabIndex = 1;
            this.label2.Text = "Type";
            this.label2.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label3.Location = new System.Drawing.Point(3, 126);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(203, 63);
            this.label3.TabIndex = 2;
            this.label3.Text = "Number";
            this.label3.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label4.Location = new System.Drawing.Point(3, 189);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(203, 63);
            this.label4.TabIndex = 3;
            this.label4.Text = "PSMessage";
            this.label4.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // btnSingleMessageSend
            // 
            this.btnSingleMessageSend.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnSingleMessageSend.Location = new System.Drawing.Point(212, 255);
            this.btnSingleMessageSend.Name = "btnSingleMessageSend";
            this.btnSingleMessageSend.Size = new System.Drawing.Size(483, 60);
            this.btnSingleMessageSend.TabIndex = 5;
            this.btnSingleMessageSend.Text = "Send";
            this.btnSingleMessageSend.UseVisualStyleBackColor = true;
            this.btnSingleMessageSend.Click += new System.EventHandler(this.btnSingleMessageSend_Click);
            // 
            // cbPsMessageType
            // 
            this.cbPsMessageType.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cbPsMessageType.Cursor = System.Windows.Forms.Cursors.Default;
            this.cbPsMessageType.Font = new System.Drawing.Font("微軟正黑體", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.cbPsMessageType.FormattingEnabled = true;
            this.cbPsMessageType.Location = new System.Drawing.Point(212, 66);
            this.cbPsMessageType.Name = "cbPsMessageType";
            this.cbPsMessageType.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.cbPsMessageType.Size = new System.Drawing.Size(483, 35);
            this.cbPsMessageType.Sorted = true;
            this.cbPsMessageType.TabIndex = 7;
            // 
            // txtPsMessageText
            // 
            this.txtPsMessageText.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtPsMessageText.Font = new System.Drawing.Font("微軟正黑體", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.txtPsMessageText.Location = new System.Drawing.Point(212, 192);
            this.txtPsMessageText.Name = "txtPsMessageText";
            this.txtPsMessageText.Size = new System.Drawing.Size(483, 35);
            this.txtPsMessageText.TabIndex = 9;
            this.txtPsMessageText.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // btnSaveAutoReplyMessage
            // 
            this.btnSaveAutoReplyMessage.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnSaveAutoReplyMessage.Location = new System.Drawing.Point(3, 255);
            this.btnSaveAutoReplyMessage.Name = "btnSaveAutoReplyMessage";
            this.btnSaveAutoReplyMessage.Size = new System.Drawing.Size(203, 60);
            this.btnSaveAutoReplyMessage.TabIndex = 10;
            this.btnSaveAutoReplyMessage.Text = "Save";
            this.btnSaveAutoReplyMessage.UseVisualStyleBackColor = true;
            this.btnSaveAutoReplyMessage.Click += new System.EventHandler(this.btnSaveAutoReplyMessage_Click);
            // 
            // cbPspMessageList
            // 
            this.cbPspMessageList.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cbPspMessageList.Font = new System.Drawing.Font("微軟正黑體", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.cbPspMessageList.FormattingEnabled = true;
            this.cbPspMessageList.Location = new System.Drawing.Point(212, 3);
            this.cbPspMessageList.Name = "cbPspMessageList";
            this.cbPspMessageList.Size = new System.Drawing.Size(483, 35);
            this.cbPspMessageList.TabIndex = 11;
            this.cbPspMessageList.SelectedIndexChanged += new System.EventHandler(this.cbPspMessageList_SelectedIndexChanged);
            // 
            // textBox1
            // 
            this.textBox1.Font = new System.Drawing.Font("微軟正黑體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.textBox1.Location = new System.Drawing.Point(12, 407);
            this.textBox1.Multiline = true;
            this.textBox1.Name = "textBox1";
            this.textBox1.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.textBox1.Size = new System.Drawing.Size(1032, 348);
            this.textBox1.TabIndex = 2;
            // 
            // timer1
            // 
            this.timer1.Enabled = true;
            this.timer1.Interval = 500;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // AseAgvlConnectorForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1179, 767);
            this.ControlBox = false;
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.btnHide);
            this.Name = "AseAgvlConnectorForm";
            this.Text = "AsePspConnectorForm";
            this.tabControl1.ResumeLayout(false);
            this.SingleCommandPage.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tableLayoutPanel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numPsMessageNumber)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnHide;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage InfoPage;
        private System.Windows.Forms.TabPage SingleCommandPage;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.NumericUpDown numPsMessageNumber;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button btnSingleMessageSend;
        private System.Windows.Forms.ComboBox cbPsMessageType;
        private System.Windows.Forms.TextBox txtPsMessageText;
        private System.Windows.Forms.Button btnSaveAutoReplyMessage;
        private System.Windows.Forms.ComboBox cbPspMessageList;
        private System.Windows.Forms.Timer timer1;
    }
}