namespace CourtIntrigue
{
    partial class MainWindow
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
            this.permanentSplit = new System.Windows.Forms.SplitContainer();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.nextButton = new System.Windows.Forms.Button();
            this.debugBox = new System.Windows.Forms.TextBox();
            this.dateLabel = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.permanentSplit)).BeginInit();
            this.permanentSplit.Panel1.SuspendLayout();
            this.permanentSplit.Panel2.SuspendLayout();
            this.permanentSplit.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            this.SuspendLayout();
            // 
            // permanentSplit
            // 
            this.permanentSplit.Dock = System.Windows.Forms.DockStyle.Fill;
            this.permanentSplit.Location = new System.Drawing.Point(0, 0);
            this.permanentSplit.Name = "permanentSplit";
            // 
            // permanentSplit.Panel1
            // 
            this.permanentSplit.Panel1.Controls.Add(this.dateLabel);
            this.permanentSplit.Panel1.Controls.Add(this.nextButton);
            // 
            // permanentSplit.Panel2
            // 
            this.permanentSplit.Panel2.Controls.Add(this.splitContainer2);
            this.permanentSplit.Size = new System.Drawing.Size(1117, 640);
            this.permanentSplit.SplitterDistance = 203;
            this.permanentSplit.TabIndex = 0;
            // 
            // splitContainer2
            // 
            this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer2.Location = new System.Drawing.Point(0, 0);
            this.splitContainer2.Name = "splitContainer2";
            this.splitContainer2.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer2.Panel1
            // 
            this.splitContainer2.Panel1.BackColor = System.Drawing.SystemColors.Control;
            // 
            // splitContainer2.Panel2
            // 
            this.splitContainer2.Panel2.Controls.Add(this.debugBox);
            this.splitContainer2.Size = new System.Drawing.Size(910, 640);
            this.splitContainer2.SplitterDistance = 229;
            this.splitContainer2.TabIndex = 0;
            // 
            // nextButton
            // 
            this.nextButton.Location = new System.Drawing.Point(30, 595);
            this.nextButton.Name = "nextButton";
            this.nextButton.Size = new System.Drawing.Size(141, 33);
            this.nextButton.TabIndex = 0;
            this.nextButton.Text = "Step";
            this.nextButton.UseVisualStyleBackColor = true;
            this.nextButton.Click += new System.EventHandler(this.nextButton_Click);
            // 
            // debugBox
            // 
            this.debugBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.debugBox.Location = new System.Drawing.Point(0, 0);
            this.debugBox.Multiline = true;
            this.debugBox.Name = "debugBox";
            this.debugBox.ReadOnly = true;
            this.debugBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.debugBox.Size = new System.Drawing.Size(910, 407);
            this.debugBox.TabIndex = 0;
            // 
            // dateLabel
            // 
            this.dateLabel.AutoSize = true;
            this.dateLabel.Location = new System.Drawing.Point(12, 9);
            this.dateLabel.Name = "dateLabel";
            this.dateLabel.Size = new System.Drawing.Size(33, 13);
            this.dateLabel.TabIndex = 1;
            this.dateLabel.Text = "Date:";
            // 
            // MainWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1117, 640);
            this.Controls.Add(this.permanentSplit);
            this.Name = "MainWindow";
            this.Text = "Form1";
            this.permanentSplit.Panel1.ResumeLayout(false);
            this.permanentSplit.Panel1.PerformLayout();
            this.permanentSplit.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.permanentSplit)).EndInit();
            this.permanentSplit.ResumeLayout(false);
            this.splitContainer2.Panel2.ResumeLayout(false);
            this.splitContainer2.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
            this.splitContainer2.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer permanentSplit;
        private System.Windows.Forms.SplitContainer splitContainer2;
        private System.Windows.Forms.Button nextButton;
        private System.Windows.Forms.TextBox debugBox;
        private System.Windows.Forms.Label dateLabel;
    }
}

