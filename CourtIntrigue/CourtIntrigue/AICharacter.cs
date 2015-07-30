using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CourtIntrigue
{
    class AICharacter : Character
    {
        public AICharacter(string name, Dynasty dynasty, int money, Game game) : base(name, dynasty, money, game)
        {

        }

        public override Action Tick(Room room)
        {
            CharacterLog("In room with: " + string.Join(", ", room.GetCharacters(this).Select(c => c.Name)));

            // Get number of solo actions
            string[] actions = room.Actions;

            // Determine if room is approachable & get them
            if (room.CharactersApproachable)
            {
                int num = Game.GetRandom(actions.Length + room.GetCharacters(this).Count());
                if (num < actions.Length)
                {
                    return new Action(actions[num], this, null);
                }
                else
                {
                    return new Action(Action.APPROACH_ACTION, this, room.GetCharacters(this).ElementAt(num - actions.Length));
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
