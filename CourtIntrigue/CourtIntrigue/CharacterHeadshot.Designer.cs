namespace CourtIntrigue
{
    partial class CharacterHeadshot
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.portrait = new System.Windows.Forms.PictureBox();
            this.nameLabel = new System.Windows.Forms.Label();
            this.prestigeBox = new System.Windows.Forms.Label();
            this.perspectiveOfTarget = new System.Windows.Forms.Label();
            this.targetOfPerspective = new System.Windows.Forms.Label();
            this.actionButton = new System.Windows.Forms.Button();
            this.mainTooltip = new System.Windows.Forms.ToolTip(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.portrait)).BeginInit();
            this.SuspendLayout();
            // 
            // portrait
            // 
            this.portrait.Location = new System.Drawing.Point(0, 19);
            this.portrait.Name = "portrait";
            this.portrait.Size = new System.Drawing.Size(96, 96);
            this.portrait.TabIndex = 0;
            this.portrait.TabStop = false;
            // 
            // nameLabel
            // 
            this.nameLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.nameLabel.AutoEllipsis = true;
            this.nameLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.nameLabel.Location = new System.Drawing.Point(3, 0);
            this.nameLabel.Name = "nameLabel";
            this.nameLabel.Size = new System.Drawing.Size(150, 16);
            this.nameLabel.TabIndex = 1;
            this.nameLabel.Text = "nameLabel";
            this.nameLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // prestigeBox
            // 
            this.prestigeBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.prestigeBox.Location = new System.Drawing.Point(98, 19);
            this.prestigeBox.Name = "prestigeBox";
            this.prestigeBox.Size = new System.Drawing.Size(50, 16);
            this.prestigeBox.TabIndex = 2;
            this.prestigeBox.Text = "prestige";
            this.prestigeBox.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // perspectiveOfTarget
            // 
            this.perspectiveOfTarget.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.perspectiveOfTarget.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(192)))), ((int)(((byte)(0)))));
            this.perspectiveOfTarget.Location = new System.Drawing.Point(98, 37);
            this.perspectiveOfTarget.Name = "perspectiveOfTarget";
            this.perspectiveOfTarget.Size = new System.Drawing.Size(50, 16);
            this.perspectiveOfTarget.TabIndex = 4;
            this.perspectiveOfTarget.Text = "+10";
            this.perspectiveOfTarget.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // targetOfPerspective
            // 
            this.targetOfPerspective.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.targetOfPerspective.ForeColor = System.Drawing.Color.Red;
            this.targetOfPerspective.Location = new System.Drawing.Point(98, 55);
            this.targetOfPerspective.Name = "targetOfPerspective";
            this.targetOfPerspective.Size = new System.Drawing.Size(50, 16);
            this.targetOfPerspective.TabIndex = 5;
            this.targetOfPerspective.Text = "-10";
            this.targetOfPerspective.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // actionButton
            // 
            this.actionButton.Location = new System.Drawing.Point(6, 124);
            this.actionButton.Name = "actionButton";
            this.actionButton.Size = new System.Drawing.Size(84, 23);
            this.actionButton.TabIndex = 6;
            this.actionButton.Text = "Select";
            this.actionButton.UseVisualStyleBackColor = true;
            // 
            // CharacterHeadshot
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.Controls.Add(this.actionButton);
            this.Controls.Add(this.targetOfPerspective);
            this.Controls.Add(this.perspectiveOfTarget);
            this.Controls.Add(this.prestigeBox);
            this.Controls.Add(this.nameLabel);
            this.Controls.Add(this.portrait);
            this.Name = "CharacterHeadshot";
            this.Size = new System.Drawing.Size(148, 148);
            ((System.ComponentModel.ISupportInitialize)(this.portrait)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.PictureBox portrait;
        private System.Windows.Forms.Label nameLabel;
        private System.Windows.Forms.Label prestigeBox;
        private System.Windows.Forms.Label perspectiveOfTarget;
        private System.Windows.Forms.Label targetOfPerspective;
        private System.Windows.Forms.Button actionButton;
        private System.Windows.Forms.ToolTip mainTooltip;
    }
}
