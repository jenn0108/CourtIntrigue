﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CourtIntrigue
{
    internal partial class CharacterHeadshot : UserControl
    {
        private Character targetCharacter;
        private Character perspectiveCharacter;

        private bool active;
        private bool interaction;

        public bool Active
        {
            get
            {
                return active;
            }
            set
            {
                active = value;
                actionButton.Enabled = active;
            }
        }

        public bool Interaction
        {
            get
            {
                return interaction;
            }
            set
            {
                interaction = value;
                if(interaction)
                {
                    actionButton.Visible = true;
                    Size = new Size(148, 148);
                }
                else
                {
                    actionButton.Visible = false;
                    Size = new Size(148, 116);
                }
            }
        }

        public Character TargetCharacter
        {
            get
            {
                return targetCharacter;
            }
            set
            {
                targetCharacter = value;
                FillInfo();
            }
        }

        public Character PerspectiveCharacter
        {
            get
            {
                return perspectiveCharacter;
            }
            set
            {
                perspectiveCharacter = value;
                FillInfo();
            }
        }

        public event EventHandler SelectCharacter;

        public CharacterHeadshot()
        {
            InitializeComponent();
            FillInfo();
            actionButton.Click += ActionButton_Click;
        }

        private void FillInfo()
        {
            if(targetCharacter != null)
            {
                portrait.Image = targetCharacter.GetPortrait();

                nameLabel.Text = targetCharacter.Fullname;
                prestigeBox.Text = targetCharacter.Prestige.ToString();

                if(perspectiveCharacter != null && targetCharacter != perspectiveCharacter)
                {
                    SetLabelOpinion(perspectiveOfTarget, perspectiveCharacter.GetOpinionOf(targetCharacter), string.Format("{0} opinion of {1}", perspectiveCharacter.Fullname, targetCharacter.Fullname));
                    SetLabelOpinion(targetOfPerspective, targetCharacter.GetOpinionOf(perspectiveCharacter), string.Format("{0} opinion of {1}", targetCharacter.Fullname, perspectiveCharacter.Fullname));

                    perspectiveOfTarget.Visible = true;
                    targetOfPerspective.Visible = true;
                }
                else
                {
                    perspectiveOfTarget.Visible = false;
                    targetOfPerspective.Visible = false;
                }
            }
        }

        private void ActionButton_Click(object sender, EventArgs e)
        {
            //A button press selects the character.
            OnSelectCharactrer(e);
        }

        protected virtual void OnSelectCharactrer(EventArgs e)
        {
            EventHandler handler = SelectCharacter;
            if(handler != null)
            {
                handler(this, e);
            }
        }

        private void SetLabelOpinion(Label label, int opinion, string meaning)
        {
            //TODO: This should only be ranges instead of numbers
            if (opinion > 0)
            {
                label.Text = string.Format("+{0}", opinion);
                label.ForeColor = Color.Green;
            }
            else if (opinion < 0)
            {
                label.Text = opinion.ToString();
                label.ForeColor = Color.Red;
            }
            else
            {
                label.Text = "0";
                label.ForeColor = Color.DarkGray;
            }
            mainTooltip.SetToolTip(label, meaning);
        }
    }
}
