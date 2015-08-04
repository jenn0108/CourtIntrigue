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
            this.nameLabel = new System.Windows.Forms.Label();
            this.opinionTitleLabel = new System.Windows.Forms.Label();
            this.opinionLabel = new System.Windows.Forms.Label();
            this.debugPerspectiveBox = new System.Windows.Forms.ComboBox();
            this.traitLabel = new System.Windows.Forms.Label();
            this.prestigeTitleLabel = new System.Windows.Forms.Label();
            this.prestigeLabel = new System.Windows.Forms.Label();
            this.debugStatsBox = new System.Windows.Forms.TextBox();
            this.informationLabel = new System.Windows.Forms.TextBox();
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
            // nameLabel
            // 
            this.nameLabel.AutoSize = true;
            this.nameLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 20F);
            this.nameLabel.Location = new System.Drawing.Point(245, 9);
            this.nameLabel.Name = "nameLabel";
            this.nameLabel.Size = new System.Drawing.Size(228, 31);
            this.nameLabel.TabIndex = 1;
            this.nameLabel.Text = "Robert Baratheon";
            // 
            // opinionTitleLabel
            // 
            this.opinionTitleLabel.AutoSize = true;
            this.opinionTitleLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.opinionTitleLabel.Location = new System.Drawing.Point(249, 54);
            this.opinionTitleLabel.Name = "opinionTitleLabel";
            this.opinionTitleLabel.Size = new System.Drawing.Size(65, 17);
            this.opinionTitleLabel.TabIndex = 3;
            this.opinionTitleLabel.Text = "Opinion: ";
            // 
            // opinionLabel
            // 
            this.opinionLabel.AutoSize = true;
            this.opinionLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.opinionLabel.Location = new System.Drawing.Point(320, 54);
            this.opinionLabel.Name = "opinionLabel";
            this.opinionLabel.Size = new System.Drawing.Size(32, 17);
            this.opinionLabel.TabIndex = 4;
            this.opinionLabel.Text = "+20";
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
            this.traitLabel.Location = new System.Drawing.Point(249, 80);
            this.traitLabel.Name = "traitLabel";
            this.traitLabel.Size = new System.Drawing.Size(232, 17);
            this.traitLabel.TabIndex = 6;
            this.traitLabel.Text = "Brave, Trusting, Gluttonous, Lustful";
            // 
            // prestigeTitleLabel
            // 
            this.prestigeTitleLabel.AutoSize = true;
            this.prestigeTitleLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.prestigeTitleLabel.Location = new System.Drawing.Point(528, 54);
            this.prestigeTitleLabel.Name = "prestigeTitleLabel";
            this.prestigeTitleLabel.Size = new System.Drawing.Size(64, 17);
            this.prestigeTitleLabel.TabIndex = 7;
            this.prestigeTitleLabel.Text = "Prestige:";
            // 
            // prestigeLabel
            // 
            this.prestigeLabel.AutoSize = true;
            this.prestigeLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.prestigeLabel.Location = new System.Drawing.Point(598, 54);
            this.prestigeLabel.Name = "prestigeLabel";
            this.prestigeLabel.Size = new System.Drawing.Size(32, 17);
            this.prestigeLabel.TabIndex = 8;
            this.prestigeLabel.Text = "130";
            // 
            // debugStatsBox
            // 
            this.debugStatsBox.Location = new System.Drawing.Point(682, 111);
            this.debugStatsBox.Multiline = true;
            this.debugStatsBox.Name = "debugStatsBox";
            this.debugStatsBox.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.debugStatsBox.Size = new System.Drawing.Size(200, 334);
            this.debugStatsBox.TabIndex = 9;
            // 
            // informationLabel
            // 
            this.informationLabel.BackColor = System.Drawing.SystemColors.Control;
            this.informationLabel.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.informationLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.informationLabel.Location = new System.Drawing.Point(251, 111);
            this.informationLabel.Multiline = true;
            this.informationLabel.Name = "informationLabel";
            this.informationLabel.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.informationLabel.Size = new System.Drawing.Size(416, 334);
            this.informationLabel.TabIndex = 10;
            this.informationLabel.TextChanged += new System.EventHandler(this.textBox2_TextChanged);
            // 
            // JournalForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(894, 462);
            this.Controls.Add(this.informationLabel);
            this.Controls.Add(this.debugStatsBox);
            this.Controls.Add(this.prestigeLabel);
            this.Controls.Add(this.prestigeTitleLabel);
            this.Controls.Add(this.traitLabel);
            this.Controls.Add(this.debugPerspectiveBox);
            this.Controls.Add(this.opinionLabel);
            this.Controls.Add(this.opinionTitleLabel);
            this.Controls.Add(this.nameLabel);
            this.Controls.Add(this.knownCharacters);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "JournalForm";
            this.Text = "JournalForm";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListBox knownCharacters;
        private System.Windows.Forms.Label nameLabel;
        private System.Windows.Forms.Label opinionTitleLabel;
        private System.Windows.Forms.Label opinionLabel;
        private System.Windows.Forms.ComboBox debugPerspectiveBox;
        private System.Windows.Forms.Label traitLabel;
        private System.Windows.Forms.Label prestigeTitleLabel;
        private System.Windows.Forms.Label prestigeLabel;
        private System.Windows.Forms.TextBox debugStatsBox;
        private System.Windows.Forms.TextBox informationLabel;
    }
}