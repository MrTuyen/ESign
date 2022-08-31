namespace Position
{
    partial class Form1
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
            this.txtllX = new System.Windows.Forms.TextBox();
            this.txtllY = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.txturX = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.txturY = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.btnPosition = new System.Windows.Forms.Button();
            this.txtFile = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(313, 202);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(26, 20);
            this.label1.TabIndex = 0;
            this.label1.Text = "llX";
            // 
            // txtllX
            // 
            this.txtllX.Location = new System.Drawing.Point(357, 200);
            this.txtllX.Name = "txtllX";
            this.txtllX.Size = new System.Drawing.Size(400, 26);
            this.txtllX.TabIndex = 1;
            // 
            // txtllY
            // 
            this.txtllY.Location = new System.Drawing.Point(357, 255);
            this.txtllY.Name = "txtllY";
            this.txtllY.Size = new System.Drawing.Size(400, 26);
            this.txtllY.TabIndex = 3;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(313, 257);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(26, 20);
            this.label2.TabIndex = 2;
            this.label2.Text = "llY";
            // 
            // txturX
            // 
            this.txturX.Location = new System.Drawing.Point(357, 304);
            this.txturX.Name = "txturX";
            this.txturX.Size = new System.Drawing.Size(400, 26);
            this.txturX.TabIndex = 5;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(313, 306);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(34, 20);
            this.label3.TabIndex = 4;
            this.label3.Text = "urX";
            // 
            // txturY
            // 
            this.txturY.Location = new System.Drawing.Point(357, 353);
            this.txturY.Name = "txturY";
            this.txturY.Size = new System.Drawing.Size(400, 26);
            this.txturY.TabIndex = 7;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(313, 355);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(34, 20);
            this.label4.TabIndex = 6;
            this.label4.Text = "urY";
            // 
            // btnPosition
            // 
            this.btnPosition.Location = new System.Drawing.Point(413, 115);
            this.btnPosition.Name = "btnPosition";
            this.btnPosition.Size = new System.Drawing.Size(247, 49);
            this.btnPosition.TabIndex = 8;
            this.btnPosition.Text = "Xác định tọa độ";
            this.btnPosition.UseVisualStyleBackColor = true;
            this.btnPosition.Click += new System.EventHandler(this.btnPosition_Click);
            // 
            // txtFile
            // 
            this.txtFile.Location = new System.Drawing.Point(360, 64);
            this.txtFile.Name = "txtFile";
            this.txtFile.Size = new System.Drawing.Size(400, 26);
            this.txtFile.TabIndex = 10;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(162, 67);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(185, 20);
            this.label5.TabIndex = 9;
            this.label5.Text = "Đường dẫn File pdf đã ký";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1167, 490);
            this.Controls.Add(this.txtFile);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.btnPosition);
            this.Controls.Add(this.txturY);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.txturX);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.txtllY);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.txtllX);
            this.Controls.Add(this.label1);
            this.Name = "Form1";
            this.Text = "Tọa độ chữ ký";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtllX;
        private System.Windows.Forms.TextBox txtllY;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txturX;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox txturY;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button btnPosition;
        private System.Windows.Forms.TextBox txtFile;
        private System.Windows.Forms.Label label5;
    }
}

