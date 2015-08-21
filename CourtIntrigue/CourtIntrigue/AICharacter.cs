using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CourtIntrigue
{
    class AICharacter : Character
    {
        public AICharacter(string name, int birthdate, Dynasty dynasty, int money, Game game, GenderEnum gender) : base(name, birthdate, dynasty, money, game, gender)
        {
        }

        public override EventContext OnTick(Action[] soloActions, Dictionary<Character, Action[]> characterActions)
        {
            CharacterLog("In room with: " + string.Join(", ", characterActions.Select(p => p.Key.Name)));

            int num = Game.GetRandom(soloActions.Length + characterActions.Count);
            if (num < soloActions.Length)
            {
                return new EventContext(soloActions[num].Identifier, this, null);
            }
            else
            {
                Character otherCharacter = characterActions.Keys.ElementAt(num - soloActions.Length);
                string[] pairActions = characterActions[otherCharacter].Select(a => a.Identifier).ToArray();
                return new EventContext(pairActions[Game.GetRandom(pairActions.Length)], this, otherCharacter);
            }

        }

        public override int OnBeginDay(Room[] rooms)
        {
            return Game.GetRandom(rooms.Length);
        }

        public override int OnChooseOption(EventOption[] options, int[] willpowerCost, EventContext context, Event e)
        {
            int allowedAcount = willpowerCost.Count(cost => cost <= WillPower);

            if(allowedAcount < 1)
            {
                //Events must have at least one willpower free option or else characters may be locked out
                //of doing anything at all.
                throw new EventIncorrectException("Events must have at least one willpower free option");
            }

            int chosenIndex = Game.GetRandom(options.Length);
            while(willpowerCost[chosenIndex] > WillPower)
            {
                chosenIndex = Game.GetRandom(options.Length);
            }
            EventOption chosen = options[chosenIndex];
            CharacterLog("Choosing " + EventHelper.ReplaceStrings(chosen.Label, context));
            return chosenIndex;
        }

        public override int OnChooseInformation(InformationInstance[] informations)
        {
            return Game.GetRandom(informations.Length);
        }

        public override int OnChooseCharacter(Character[] characters)
        {
            return Game.GetRandom(characters.Length);
        }

        public override void OnLearnInformation(InformationInstance info)
        {
            //Don't care
        }

    }
}
