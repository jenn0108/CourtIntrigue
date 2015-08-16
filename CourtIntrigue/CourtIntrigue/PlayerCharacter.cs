using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CourtIntrigue
{
    class PlayerCharacter : Character
    {
        private MainWindow main;

        public PlayerCharacter(MainWindow main, string name, int birthdate, Dynasty dynasty, int money, Game game, GenderEnum gender) : base(name, birthdate, dynasty, money, game, gender)
        {
            this.main = main;
        }
        
        public override int OnBeginDay(Room[] rooms)
        {
            TextTopBottomButton view = new TextTopBottomButton("A new day greets you!", rooms.Select(r => r.Name).ToArray(), null);
            main.LaunchView(view);
            return view.SelectedIndex;
        }

        public override EventContext OnTick(Action[] soloActions, Dictionary<Character, Action[]> characterActions)
        {
            Character[] allCharacters = CurrentRoom.GetCharacters(this).ToArray();
            bool[] characterEnables = allCharacters.Select(c => characterActions.ContainsKey(c)).ToArray();
            Character[] characters = characterActions.Keys.ToArray();
            BothButton view = new BothButton(allCharacters.Select(c => c.Fullname).ToArray(), characterEnables, soloActions.Select(a => a.Label).ToArray(), null);
            main.LaunchView(view);

            if (!view.SelectedTop)
                return new EventContext(soloActions[view.SelectedIndex].Identifier, this, null);

            Character selectedCharacter = allCharacters[view.SelectedIndex];

            //Player selected a character.
            TextTopBottomButton secondView = new TextTopBottomButton("What would you like to do with " + selectedCharacter.Fullname, characterActions[selectedCharacter].Select(a => a.Label).ToArray(), null);
            main.LaunchView(secondView);
            return new EventContext(characterActions[selectedCharacter][secondView.SelectedIndex].Identifier, this, selectedCharacter);
        }

        public override int OnChooseOption(EventOption[] options, int[] willpowerCost, EventContext context, Event e)
        {
            string[] texts = options.Select(op => EventHelper.ReplaceStrings(op.Label, context)).ToArray();
            bool[] enabled = willpowerCost.Select(cost => cost <= WillPower).ToArray();
            TextTopBottomButton view = new TextTopBottomButton(e.CreateActionDescription(context), texts, enabled);
            main.LaunchView(view);
            return view.SelectedIndex;
        }

        public override int OnChooseInformation(InformationInstance[] informations)
        {
            string[] texts = informations.Select(info => info.Description).ToArray();
            TextTopBottomButton view = new TextTopBottomButton("Choose an information...", texts, null);
            main.LaunchView(view);
            return view.SelectedIndex;
        }

    }
}
