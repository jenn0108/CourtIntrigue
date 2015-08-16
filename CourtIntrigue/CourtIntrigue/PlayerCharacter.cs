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
            TextTopBottomButton view = new TextTopBottomButton("A new day greets you!", rooms.Select(r => r.Name).ToArray());
            main.LaunchView(view);
            return view.SelectedIndex;
        }

        public override EventContext OnTick(Action[] soloActions, Dictionary<Character, Action[]> characterActions)
        {
            Character[] characters = characterActions.Keys.ToArray();
            BothButton view = new BothButton(characters.Select(c => c.Fullname).ToArray(), soloActions.Select(a => a.Label).ToArray());
            main.LaunchView(view);

            if (!view.SelectedTop)
                return new EventContext(soloActions[view.SelectedIndex].Identifier, this, null);

            //Player selected a character.
            TextTopBottomButton secondView = new TextTopBottomButton("What would you like to do with " + characters[view.SelectedIndex].Fullname, characterActions[characters[view.SelectedIndex]].Select(a => a.Label).ToArray());
            main.LaunchView(secondView);
            return new EventContext(characterActions[characters[view.SelectedIndex]][secondView.SelectedIndex].Identifier, this, characters[view.SelectedIndex]);
        }

        public override int OnChooseOption(EventOption[] options, int[] willpowerCost, EventContext context, Event e)
        {
            string[] texts = options.Select(op => op.Label).ToArray();
            TextTopBottomButton view = new TextTopBottomButton(e.CreateActionDescription(context), texts);
            main.LaunchView(view);
            return view.SelectedIndex;
        }

        public override int OnChooseInformation(InformationInstance[] informations)
        {
            string[] texts = informations.Select(info => info.Description).ToArray();
            TextTopBottomButton view = new TextTopBottomButton("Choose an information...", texts);
            main.LaunchView(view);
            return view.SelectedIndex;
        }

    }
}
