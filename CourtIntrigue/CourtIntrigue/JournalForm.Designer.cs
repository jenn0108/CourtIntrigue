namespace CourtIntrigue
{
    partial class JournalForm
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
            this.knownCharacters = new System.Windows.Forms.ListBox();
            this.debugPerspectiveBox = new System.Windows.Forms.ComboBox();
            this.traitLabel = new System.Windows.Forms.Label();
            this.debugStatsBox = new System.Windows.Forms.TextBox();
            this.informationLabel = new System.Windows.Forms.TextBox();
            this.goldLabel = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.ageLabel = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.willpowerLabel = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.headshot = new CourtIntrigue.CharacterHeadshot();
            this.SuspendLayout();
            // 
            // knownCharacters
            // 
            this.knownCharacters.FormattingEnabled = true;
            this.knownCharacters.Location = new System.Drawing.Point(12, 12);
            this.knownCharacters.Name = "knownCharacters";
            this.knownCharacters.Size = new System.Drawing.Size(202, 433);
            this.knownCharacters.TabIndex = 0;
            this.knownCharacters.SelectedIndexChanged += new System.EventHandler(this.knownCharacters_SelectedIndexChanged);
            // 
            // debugPerspectiveBox
            // 
            this.debugPerspectiveBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.debugPerspectiveBox.FormattingEnabled = true;
            this.debugPerspectiveBox.Location = new System.Drawing.Point(761, 12);
            this.debugPerspectiveBox.Name = "debugPerspectiveBox";
            this.debugPerspectiveBox.Size = new System.Drawing.Size(121, 21);
            this.debugPerspectiveBox.TabIndex = 5;
            this.debugPerspectiveBox.SelectedIndexChanged += new System.EventHandler(this.debugPerspectiveBox_SelectedIndexChanged);
            // 
            // traitLabel
            // 
            this.traitLabel.AutoSize = true;
            this.traitLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.traitLabel.Location = new System.Drawing.Point(375, 112);
            this.traitLabel.Name = "traitLabel";
            this.traitLabel.Size = new System.Drawing.Size(232, 17);
            this.traitLabel.TabIndex = 6;
            this.traitLabel.Text = "Brave, Trusting, Gluttonous, Lustful";
            // 
            // debugStatsBox
            // 
            this.debugStatsBox.Location = new System.Drawing.Point(682, 135);
            this.debugStatsBox.Multiline = true;
            this.debugStatsBox.Name = "debugStatsBox";
            this.debugStatsBox.ReadOnly = true;
            this.debugStatsBox.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.debugStatsBox.Size = new System.Drawing.Size(200, 310);
            this.debugStatsBox.TabIndex = 9;
            // 
            // informationLabel
            // 
            this.informationLabel.BackColor = System.Drawing.SystemColors.Control;
            this.informationLabel.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.informationLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.informationLabel.Location = new System.Drawing.Point(251, 135);
            this.informationLabel.Multiline = true;
            this.informationLabel.Name = "informationLabel";
            this.informationLabel.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.informationLabel.Size = new System.Drawing.Size(416, 310);
            this.informationLabel.TabIndex = 10;
            this.informationLabel.TextChanged += new System.EventHandler(this.textBox2_TextChanged);
            // 
            // goldLabel
            // 
            this.goldLabel.AutoSize = true;
            this.goldLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.goldLabel.Location = new System.Drawing.Point(449, 33);
            this.goldLabel.Name = "goldLabel";
            this.goldLabel.Size = new System.Drawing.Size(32, 17);
            this.goldLabel.TabIndex = 12;
            this.goldLabel.Text = "130";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.label2.Location = new System.Drawing.Point(405, 33);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(42, 17);
            this.label2.TabIndex = 11;
            this.label2.Text = "Gold:";
            // 
            // ageLabel
            // 
            this.ageLabel.AutoSize = true;
            this.ageLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.ageLabel.Location = new System.Drawing.Point(449, 16);
            this.ageLabel.Name = "ageLabel";
            this.ageLabel.Size = new System.Drawing.Size(32, 17);
            this.ageLabel.TabIndex = 14;
            this.ageLabel.Text = "130";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.label3.Location = new System.Drawing.Point(410, 16);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(37, 17);
            this.label3.TabIndex = 13;
            this.label3.Text = "Age:";
            // 
            // willpowerLabel
            // 
            this.willpowerLabel.AutoSize = true;
            this.willpowerLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.willpowerLabel.Location = new System.Drawing.Point(449, 50);
            this.willpowerLabel.Name = "willpowerLabel";
            this.willpowerLabel.Size = new System.Drawing.Size(32, 17);
            this.willpowerLabel.TabIndex = 16;
            this.willpowerLabel.Text = "130";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.label4.Location = new System.Drawing.Point(375, 50);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(72, 17);
            this.label4.TabIndex = 15;
            this.label4.Text = "Willpower:";
            // 
            // headshot
            // 
            this.headshot.Active = false;
            this.headshot.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.headshot.Interaction = false;
            this.headshot.Location = new System.Drawing.Point(221, 13);
            this.headshot.Name = "headshot";
            this.headshot.PerspectiveCharacter = null;
            this.headshot.Size = new System.Drawing.Size(148, 116);
            this.headshot.TabIndex = 17;
            this.headshot.TargetCharacter = null;
            // 
            // JournalForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(894, 462);
            this.Controls.Add(this.headshot);
            this.Controls.Add(this.willpowerLabel);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.ageLabel);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.goldLabel);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.informationLabel);
            this.Controls.Add(this.debugStatsBox);
            this.Controls.Add(this.traitLabel);
            this.Controls.Add(this.debugPerspectiveBox);
            this.Controls.Add(this.knownCharacters);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "JournalForm";
            this.Text = "JournalForm";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListBox knownCharacters;
        private System.Windows.Forms.ComboBox debugPerspectiveBox;
        private System.Windows.Forms.Label traitLabel;
        private System.Windows.Forms.TextBox debugStatsBox;
        private System.Windows.Forms.TextBox informationLabel;
        private System.Windows.Forms.Label goldLabel;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label ageLabel;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label willpowerLabel;
        private System.Windows.Forms.Label label4;
        private CharacterHeadshot headshot;
    }
}