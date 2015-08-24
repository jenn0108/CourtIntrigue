﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CourtIntrigue
{
    class PlayerCharacter : Character
    {

        private MainWindow main;
        private Notificator notificator;

        public PlayerCharacter(MainWindow main, string name, int birthdate, Dynasty dynasty, int money, Game game, Gender gender) : base(name, birthdate, dynasty, money, game, gender)
        {
            this.main = main;
            notificator = new Notificator();
            notificator.Show();
        }
        
        public override int OnBeginDay(Room[] rooms)
        {
            TextTopBottomButton view = new TextTopBottomButton(null, this, "A new day greets you!\nWhere would you like to go?", rooms.Select(r => r.Name).ToArray(), null, notificator);
            main.LaunchView(view);
            return view.SelectedIndex;
        }

        public override EventContext OnTick(Action[] soloActions, Dictionary<Character, Action[]> characterActions)
        {
            Character[] allCharacters = CurrentRoom.GetCharacters(this).ToArray();
            bool[] characterEnables = allCharacters.Select(c => characterActions.ContainsKey(c)).ToArray();
            Character[] characters = characterActions.Keys.ToArray();
            BothButton view = new BothButton(this, allCharacters, characterEnables, soloActions.Select(a => a.Label).ToArray(), null, notificator);
            main.LaunchView(view);

            if (!view.SelectedTop)
                return new EventContext(soloActions[view.SelectedIndex].Identifier, this, null);

            Character selectedCharacter = allCharacters[view.SelectedIndex];

            //Player selected a character.
            TextTopBottomButton secondView = new TextTopBottomButton(new Character[] { selectedCharacter }, this, "What would you like to do with " + selectedCharacter.Fullname, characterActions[selectedCharacter].Select(a => a.Label).ToArray(), null, notificator);
            main.LaunchView(secondView);
            return new EventContext(characterActions[selectedCharacter][secondView.SelectedIndex].Identifier, this, selectedCharacter);
        }

        public override int OnChooseOption(EventOption[] options, int[] willpowerCost, EventContext context, Event e)
        {
            string[] texts = options.Select(op => EventHelper.ReplaceStrings(op.Label, context)).ToArray();
            bool[] enabled = willpowerCost.Select(cost => cost <= WillPower).ToArray();
            TextTopBottomButton view = new TextTopBottomButton(context.GetCharacters(), this, e.CreateActionDescription(context), texts, enabled, notificator);
            main.LaunchView(view);
            return view.SelectedIndex;
        }

        public override int OnChooseInformation(InformationInstance[] informations)
        {
            string[] texts = informations.Select(info => info.Description).ToArray();
            TextTopBottomButton view = new TextTopBottomButton(null, this, "Choose an information...", texts, null, notificator);
            main.LaunchView(view);
            return view.SelectedIndex;
        }

        public override int OnChooseCharacter(Character[] characters)
        {
            SelectCharacterView view = new SelectCharacterView(this, characters, null, "Choose a character", notificator);
            main.LaunchView(view);
            return view.SelectedIndex;
        }

        public override void OnLearnInformation(InformationInstance info)
        {
            notificator.AddNotification(info);
        }

    }
}
