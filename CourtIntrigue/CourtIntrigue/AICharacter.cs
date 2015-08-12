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

        public override EventContext OnTick(string[] soloActions, Dictionary<Character, string[]> characterActions)
        {
            CharacterLog("In room with: " + string.Join(", ", characterActions.Select(p => p.Key.Name)));

            int num = Game.GetRandom(soloActions.Length + characterActions.Count);
            if (num < soloActions.Length)
            {
                return new EventContext(soloActions[num], this, null);
            }
            else
            {
                Character otherCharacter = characterActions.Keys.ElementAt(num - soloActions.Length);
                string[] pairActions = characterActions[otherCharacter];
                return new EventContext(pairActions[Game.GetRandom(pairActions.Length)], this, otherCharacter);
            }

        }

        public override int OnBeginDay(Room[] rooms)
        {
            return Game.GetRandom(rooms.Length);
        }

        public override int OnChooseOption(EventOption[] options, int[] willpowerCost, EventContext context, Event e)
        {
            int chosenIndex = Game.GetRandom(options.Length);
            EventOption chosen = options[chosenIndex];
            CharacterLog("Choosing " + EventHelper.ReplaceStrings(chosen.Label, context));
            return chosenIndex;
        }

        public override int OnChooseInformation(InformationInstance[] informations)
        {
            return Game.GetRandom(informations.Length);
        }

    }
}
