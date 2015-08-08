using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CourtIntrigue
{
    partial class JournalForm : Form
    {
        private Character[] characters;
        private Game game;

        public JournalForm(Character[] characters, Game game)
        {
            InitializeComponent();
            this.game = game;
            this.characters = characters;
            knownCharacters.Items.AddRange(characters);
            debugPerspectiveBox.Items.AddRange(characters);
            knownCharacters.SelectedIndex = 0;
            debugPerspectiveBox.SelectedIndex = 0;
        }

        private void UpdateCharacterInformation(Character character)
        {
            Character perspectiveChar = debugPerspectiveBox.SelectedItem as Character;
            if (perspectiveChar == null || character == null)
                return;

            nameLabel.Text = character.Fullname;
            goldLabel.Text = character.Money.ToString();
            ageLabel.Text = character.Age.ToString();
            int opinion = character.GetOpinionOf(perspectiveChar);
            if (opinion > 0)
                opinionLabel.Text = "+" + opinion;
            else
                opinionLabel.Text = opinion.ToString();

            prestigeLabel.Text = character.Dynasty.Prestige.ToString();

            StringBuilder infoBuilder = new StringBuilder();
            foreach (InformationInstance info in perspectiveChar.GetInformationAbout(character))
            {
                infoBuilder.AppendLine(info.Description);
            }
            informationLabel.Text = infoBuilder.ToString();

            traitLabel.Text = string.Join(", ", character.Traits.Select(t => t.Label));

            StringBuilder opModBuilder = new StringBuilder();
            opModBuilder.AppendLine("Prestige Modifiers:");
            foreach (PrestigeModifier mod in perspectiveChar.CurrentPrestigeModifiers)
            {
                opModBuilder.AppendLine(string.Format("{0} {1}", mod.DailyChange, mod.Description));
            }
            opModBuilder.AppendLine("Opinion Modifiers:");
            foreach (OpinionModifierInstance mod in perspectiveChar.GetOpinionModifiersAbout(character))
            {
                opModBuilder.AppendLine(string.Format("{0} {1}", mod.GetChange(game.CurrentTime), mod.Description));
            }
            debugStatsBox.Text = opModBuilder.ToString();
        }

        private void knownCharacters_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateCharacterInformation(knownCharacters.SelectedItem as Character);
        }

        private void debugPerspectiveBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateCharacterInformation(knownCharacters.SelectedItem as Character);
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
