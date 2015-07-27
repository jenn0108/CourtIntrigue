using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CourtIntrigue
{
    class AICharacter : Character
    {
        public AICharacter(string name, int money, Game game) : base(name, money, game)
        {

        }

        public override Action Tick(Room room, Logger debugLogger)
        {
            debugLogger.PrintText(Name + ": Doing the needful!");
            debugLogger.PrintText(Name + ": In room with: " + string.Join(", ", room.GetCharacters(this).Select(c => c.Name)));

            // Get number of solo actions
            string[] actions = room.Actions;

            // Determine if room is approachable & get them
            if (room.CharactersApproachable)
            {
                int num = Game.GetRandom(actions.Length + room.GetCharacters(this).Count());
                if (num < actions.Length)
                {
                    return new Action(actions[num], null);
                }
                else
                {
                    return new Action(Action.APPROACH_ACTION, room.GetCharacters(this).ElementAt(num - actions.Length));
                }
            }
            else
            {
                int num = Game.GetRandom(actions.Length);
                return new Action(actions[num], null);
            }

        }

        public override Room BeginDay(Logger debugLogger)
        {
            Room ret = Game.CommonRooms[Game.GetRandom(Game.CommonRooms.Length)];
            debugLogger.PrintText(Name + " is going to " + ret.Name);
            return ret;
        }

    }
}
