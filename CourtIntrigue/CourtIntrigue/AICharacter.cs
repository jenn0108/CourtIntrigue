using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CourtIntrigue
{
    class AICharacter : Character
    {
        public AICharacter(string name, int birthdate, Dynasty dynasty, int money, Game game, GenderEnum gender, DependentCharacter spouse, List<DependentCharacter> children, Room home) : base(name, birthdate, dynasty, money, game, gender, spouse, children, home)
        {
            CharacterLog("Created character with spouse: " + spouse.Name + " and children: " + string.Join(", ", children.Select(c => c.Name + "(" + c.Gender.ToString() + ")")));
        }

        public override EventContext OnTick()
        {
            CharacterLog("In room with: " + string.Join(", ", room.GetUnoccuppiedCharacters(this).Select(c => c.Name)));

            string[] actions = room.SoloActions;

            if (room.PairActions.Length > 0)
            {
                int num = Game.GetRandom(actions.Length + room.GetUnoccuppiedCharacters(this).Count());
                if (num < actions.Length)
                {
                    return new EventContext(actions[num], this, null);
                }
                else
                {
                    Character otherCharacter = room.GetUnoccuppiedCharacters(this).ElementAt(num - actions.Length);
                    string[] pairActions = Game.FindAllowableActions(room, this, otherCharacter);
                    return new EventContext(pairActions[Game.GetRandom(pairActions.Length)], this, otherCharacter);
                }
            }
            else
            {
                int num = Game.GetRandom(actions.Length);
                return new EventContext(actions[num], this, null);
            }

        }

        public override Room OnBeginDay()
        {
            int index = Game.GetRandom(Game.CommonRooms.Length + 1);
            if(index == Game.CommonRooms.Length)
            {
                CharacterLog("Staying home");
                return Home;
            }
            else
            {
                Room ret = Game.CommonRooms[index];
                CharacterLog("Going to " + ret.Name);
                return ret;
            }
        }

        public override int ChooseOption(EventOption[] options, int[] willpowerCost, EventContext context, Event e)
        {
            int chosenIndex = Game.GetRandom(options.Length);
            EventOption chosen = options[chosenIndex];
            CharacterLog("Choosing " + EventHelper.ReplaceStrings(chosen.Label, context));
            return chosenIndex;
        }

        public override InformationInstance ChooseInformation()
        {
            return KnownInformation[Game.GetRandom(KnownInformation.Count)];
        }

    }
}
