﻿using System;
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
        private Character fixedPerspective;

        public JournalForm(Character[] characters, Game game, Character fixedPerspective)
        {
            InitializeComponent();
            this.game = game;
            this.characters = characters;
            this.fixedPerspective = fixedPerspective;
            knownCharacters.Items.AddRange(characters);
            knownCharacters.SelectedIndex = 0;

            if(fixedPerspective != null)
            {
                debugPerspectiveBox.Visible = false;
            }
            else
            {
                debugPerspectiveBox.Items.AddRange(characters);
                debugPerspectiveBox.SelectedIndex = 0;
            }
        }

        private void UpdateCharacterInformation(Character character)
        {
            Character perspectiveChar = fixedPerspective == null ? debugPerspectiveBox.SelectedItem as Character : fixedPerspective;
            if (perspectiveChar == null || character == null)
                return;

            headshot.TargetCharacter = character;
            headshot.PerspectiveCharacter = perspectiveChar;
            
            goldLabel.Text = character.Money.ToString();
            ageLabel.Text = character.Age.ToString();
            willpowerLabel.Text = character.WillPower.ToString();

            StringBuilder infoBuilder = new StringBuilder();
            foreach (InformationInstance info in perspectiveChar.GetInformationAbout(character))
            {
                infoBuilder.AppendLine(info.Description);
            }
            informationLabel.Text = infoBuilder.ToString();

            traitLabel.Text = string.Join(", ", character.Traits.Select(t => t.Label));

            StringBuilder opModBuilder = new StringBuilder();
            opModBuilder.AppendLine("Prestige Modifiers:");
            foreach (PrestigeModifier mod in character.CurrentPrestigeModifiers)
            {
                opModBuilder.AppendLine(string.Format("{0} {1}", mod.DailyChange, mod.Description));
            }
            opModBuilder.AppendLine("Opinion Modifiers:");
            foreach (OpinionModifierInstance mod in perspectiveChar.GetOpinionModifiersAbout(character))
            {
                opModBuilder.AppendLine(string.Format("{0} {1}", mod.GetChange(game.CurrentDay), mod.Description));
            }
            foreach (String variable in character.GetVariableNames())
            {
                opModBuilder.AppendLine(string.Format("{0} = {1}", variable, character.GetVariable(variable)));
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
