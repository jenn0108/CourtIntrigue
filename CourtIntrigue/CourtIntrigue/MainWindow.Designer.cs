﻿namespace CourtIntrigue
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
            this.logButton = new System.Windows.Forms.Button();
            this.calendarButton = new System.Windows.Forms.Button();
            this.journalButton = new System.Windows.Forms.Button();
            this.speedStep = new System.Windows.Forms.Button();
            this.restartButton = new System.Windows.Forms.Button();
            this.debugButton = new System.Windows.Forms.Button();
            this.dateLabel = new System.Windows.Forms.Label();
            this.nextButton = new System.Windows.Forms.Button();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            ((System.ComponentModel.ISupportInitialize)(this.permanentSplit)).BeginInit();
            this.permanentSplit.Panel1.SuspendLayout();
            this.permanentSplit.Panel2.SuspendLayout();
            this.permanentSplit.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
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
            this.permanentSplit.Panel1.Controls.Add(this.logButton);
            this.permanentSplit.Panel1.Controls.Add(this.calendarButton);
            this.permanentSplit.Panel1.Controls.Add(this.journalButton);
            this.permanentSplit.Panel1.Controls.Add(this.speedStep);
            this.permanentSplit.Panel1.Controls.Add(this.restartButton);
            this.permanentSplit.Panel1.Controls.Add(this.debugButton);
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
            // logButton
            // 
            this.logButton.Location = new System.Drawing.Point(30, 517);
            this.logButton.Name = "logButton";
            this.logButton.Size = new System.Drawing.Size(141, 33);
            this.logButton.TabIndex = 7;
            this.logButton.Text = "Log";
            this.logButton.UseVisualStyleBackColor = true;
            this.logButton.Click += new System.EventHandler(this.logButton_Click);
            // 
            // calendarButton
            // 
            this.calendarButton.Location = new System.Drawing.Point(31, 47);
            this.calendarButton.Name = "calendarButton";
            this.calendarButton.Size = new System.Drawing.Size(141, 33);
            this.calendarButton.TabIndex = 6;
            this.calendarButton.Text = "Calendar";
            this.calendarButton.UseVisualStyleBackColor = true;
            // 
            // journalButton
            // 
            this.journalButton.Location = new System.Drawing.Point(31, 106);
            this.journalButton.Name = "journalButton";
            this.journalButton.Size = new System.Drawing.Size(141, 33);
            this.journalButton.TabIndex = 5;
            this.journalButton.Text = "Journal";
            this.journalButton.UseVisualStyleBackColor = true;
            this.journalButton.Click += new System.EventHandler(this.journalButton_Click);
            // 
            // speedStep
            // 
            this.speedStep.Location = new System.Drawing.Point(30, 556);
            this.speedStep.Name = "speedStep";
            this.speedStep.Size = new System.Drawing.Size(141, 33);
            this.speedStep.TabIndex = 4;
            this.speedStep.Text = "Speed Step";
            this.speedStep.UseVisualStyleBackColor = true;
            this.speedStep.Click += new System.EventHandler(this.speedStep_Click);
            // 
            // restartButton
            // 
            this.restartButton.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.restartButton.Location = new System.Drawing.Point(174, 595);
            this.restartButton.Name = "restartButton";
            this.restartButton.Size = new System.Drawing.Size(26, 33);
            this.restartButton.TabIndex = 3;
            this.restartButton.Text = "↻";
            this.restartButton.UseVisualStyleBackColor = true;
            this.restartButton.Visible = false;
            this.restartButton.Click += new System.EventHandler(this.restartButton_Click);
            // 
            // debugButton
            // 
            this.debugButton.Location = new System.Drawing.Point(3, 595);
            this.debugButton.Name = "debugButton";
            this.debugButton.Size = new System.Drawing.Size(26, 33);
            this.debugButton.TabIndex = 2;
            this.debugButton.Text = "D";
            this.debugButton.UseVisualStyleBackColor = true;
            this.debugButton.Visible = false;
            this.debugButton.Click += new System.EventHandler(this.debugButton_Click);
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
            this.splitContainer2.Size = new System.Drawing.Size(910, 640);
            this.splitContainer2.SplitterDistance = 500;
            this.splitContainer2.TabIndex = 0;
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
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
            this.splitContainer2.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer permanentSplit;
        private System.Windows.Forms.SplitContainer splitContainer2;
        private System.Windows.Forms.Button nextButton;
        private System.Windows.Forms.Label dateLabel;
        private System.Windows.Forms.Button debugButton;
        private System.Windows.Forms.Button restartButton;
        private System.Windows.Forms.Button speedStep;
        private System.Windows.Forms.Button calendarButton;
        private System.Windows.Forms.Button journalButton;
        private System.Windows.Forms.Button logButton;
    }
}

