using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CourtIntrigue
{
    class AICharacter : Character
    {
        public AICharacter(string name, Dynasty dynasty, int money, Game game, GenderEnum gender, List<DependentCharacter> dependents) : base(name, dynasty, money, game, gender, dependents)
        {
            CharacterLog("Created character with dependents: " + string.Join(", ", dependents.Select(c => c.Name + "(" + c.Gender.ToString() + ")")));
        }

        public override Action Tick(Room room)
        {
            CharacterLog("In room with: " + string.Join(", ", room.GetCharacters(this).Select(c => c.Name)));

            string[] actions = room.SoloActions;

            if (room.PairActions.Length > 0)
            {
                int num = Game.GetRandom(actions.Length + room.GetCharacters(this).Count());
                if (num < actions.Length)
                {
                    return new Action(actions[num], this, null);
                }
                else
                {
                    Character otherCharacter = room.GetCharacters(this).ElementAt(num - actions.Length);
                    string[] pairActions = Game.FindAllowableActions(room, this, otherCharacter);
                    return new Action(pairActions[Game.GetRandom(pairActions.Length)], this, otherCharacter);
                }
            }
            else
            {
                int num = Game.GetRandom(actions.Length);
                return new Action(actions[num], this, null);
            }

        }

        public override Room BeginDay()
        {
            Room ret = Game.CommonRooms[Game.GetRandom(Game.CommonRooms.Length)];
            CharacterLog("Going to " + ret.Name);
            return ret;
        }

        public override EventOption ChooseOption(Action action, Event e)
        {
            EventOption[] options = e.GetAvailableOptions(action);
            EventOption chosen = options[Game.GetRandom(options.Length)];
            CharacterLog("Event:" + e.CreateActionDescription(action));
            CharacterLog("Choosing " + chosen.Label);
            return chosen;
        }

    }
}
