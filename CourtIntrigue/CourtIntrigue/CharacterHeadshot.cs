using System;
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
    public partial class CharacterHeadshot : UserControl
    {
        private Character targetCharacter;
        private Character perspectiveCharacter;

        private bool active;

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

        public event EventHandler SelectCharacter;

        internal CharacterHeadshot(Character targetCharacter, Character perspectiveCharacter)
        {
            this.targetCharacter = targetCharacter;
            this.perspectiveCharacter = perspectiveCharacter;
            InitializeComponent();

            nameLabel.Text = targetCharacter.Fullname;
            prestigeBox.Text = targetCharacter.Prestige.ToString();
            SetLabelOpinion(perspectiveOfTarget, perspectiveCharacter.GetOpinionOf(targetCharacter), string.Format("{0} opinion of {1}", perspectiveCharacter.Fullname, targetCharacter.Fullname));
            SetLabelOpinion(targetOfPerspective, targetCharacter.GetOpinionOf(perspectiveCharacter), string.Format("{0} opinion of {1}", targetCharacter.Fullname, perspectiveCharacter.Fullname));

            actionButton.Click += ActionButton_Click;
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
                label.ForeColor = Color.Yellow;
            }
            mainTooltip.SetToolTip(label, meaning);
        }
    }
}
