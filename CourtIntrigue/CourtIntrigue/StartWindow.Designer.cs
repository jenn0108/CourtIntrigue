namespace CourtIntrigue
{
    partial class StartWindow
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
            this.automatedButton = new System.Windows.Forms.RadioButton();
            this.singlePlayerButton = new System.Windows.Forms.RadioButton();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.nameBox = new System.Windows.Forms.TextBox();
            this.dynastyBox = new System.Windows.Forms.TextBox();
            this.startButton = new System.Windows.Forms.Button();
            this.quitButton = new System.Windows.Forms.Button();
            this.ageBox = new System.Windows.Forms.NumericUpDown();
            ((System.ComponentModel.ISupportInitialize)(this.ageBox)).BeginInit();
            this.SuspendLayout();
            // 
            // automatedButton
            // 
            this.automatedButton.AutoSize = true;
            this.automatedButton.Checked = true;
            this.automatedButton.Location = new System.Drawing.Point(12, 12);
            this.automatedButton.Name = "automatedButton";
            this.automatedButton.Size = new System.Drawing.Size(107, 17);
            this.automatedButton.TabIndex = 0;
            this.automatedButton.TabStop = true;
            this.automatedButton.Text = "Automated Game";
            this.automatedButton.UseVisualStyleBackColor = true;
            this.automatedButton.CheckedChanged += new System.EventHandler(this.automatedButton_CheckedChanged);
            // 
            // singlePlayerButton
            // 
            this.singlePlayerButton.AutoSize = true;
            this.singlePlayerButton.Location = new System.Drawing.Point(12, 35);
            this.singlePlayerButton.Name = "singlePlayerButton";
            this.singlePlayerButton.Size = new System.Drawing.Size(117, 17);
            this.singlePlayerButton.TabIndex = 1;
            this.singlePlayerButton.Text = "Single Player Game";
            this.singlePlayerButton.UseVisualStyleBackColor = true;
            this.singlePlayerButton.CheckedChanged += new System.EventHandler(this.singlePlayerButton_CheckedChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(22, 70);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(38, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "Name:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 96);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(48, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "Dynasty:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(31, 122);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(29, 13);
            this.label3.TabIndex = 4;
            this.label3.Text = "Age:";
            // 
            // nameBox
            // 
            this.nameBox.Enabled = false;
            this.nameBox.Location = new System.Drawing.Point(62, 67);
            this.nameBox.Name = "nameBox";
            this.nameBox.Size = new System.Drawing.Size(167, 20);
            this.nameBox.TabIndex = 5;
            // 
            // dynastyBox
            // 
            this.dynastyBox.Enabled = false;
            this.dynastyBox.Location = new System.Drawing.Point(62, 93);
            this.dynastyBox.Name = "dynastyBox";
            this.dynastyBox.Size = new System.Drawing.Size(167, 20);
            this.dynastyBox.TabIndex = 6;
            // 
            // startButton
            // 
            this.startButton.Location = new System.Drawing.Point(54, 145);
            this.startButton.Name = "startButton";
            this.startButton.Size = new System.Drawing.Size(75, 23);
            this.startButton.TabIndex = 8;
            this.startButton.Text = "Start Game";
            this.startButton.UseVisualStyleBackColor = true;
            this.startButton.Click += new System.EventHandler(this.startButton_Click);
            // 
            // quitButton
            // 
            this.quitButton.Location = new System.Drawing.Point(135, 145);
            this.quitButton.Name = "quitButton";
            this.quitButton.Size = new System.Drawing.Size(75, 23);
            this.quitButton.TabIndex = 9;
            this.quitButton.Text = "Quit";
            this.quitButton.UseVisualStyleBackColor = true;
            this.quitButton.Click += new System.EventHandler(this.quitButton_Click);
            // 
            // ageBox
            // 
            this.ageBox.Enabled = false;
            this.ageBox.Location = new System.Drawing.Point(62, 120);
            this.ageBox.Maximum = new decimal(new int[] {
            50,
            0,
            0,
            0});
            this.ageBox.Minimum = new decimal(new int[] {
            20,
            0,
            0,
            0});
            this.ageBox.Name = "ageBox";
            this.ageBox.Size = new System.Drawing.Size(44, 20);
            this.ageBox.TabIndex = 10;
            this.ageBox.Value = new decimal(new int[] {
            25,
            0,
            0,
            0});
            // 
            // StartWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(252, 178);
            this.ControlBox = false;
            this.Controls.Add(this.ageBox);
            this.Controls.Add(this.quitButton);
            this.Controls.Add(this.startButton);
            this.Controls.Add(this.dynastyBox);
            this.Controls.Add(this.nameBox);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.singlePlayerButton);
            this.Controls.Add(this.automatedButton);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Name = "StartWindow";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.Text = "Court Intrigue";
            ((System.ComponentModel.ISupportInitialize)(this.ageBox)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.RadioButton automatedButton;
        private System.Windows.Forms.RadioButton singlePlayerButton;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox nameBox;
        private System.Windows.Forms.TextBox dynastyBox;
        private System.Windows.Forms.Button startButton;
        private System.Windows.Forms.Button quitButton;
        private System.Windows.Forms.NumericUpDown ageBox;
    }
}