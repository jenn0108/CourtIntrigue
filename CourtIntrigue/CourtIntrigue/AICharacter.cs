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

        public override EventContext Tick(Room room)
        {
            CharacterLog("In room with: " + string.Join(", ", room.GetUnoccuppiedCharacters(this).Select(c => c.Name)));

            string[] actions = room.SoloActions;

            if (room.PairActions.Length > 0)
            {
                int num = Game.GetRandom(actions.Length + room.GetUnoccuppiedCharacters(this).Count());
                if (num < actions.Length)
                {
                    return new EventContext(actions[num], this, null, room);
                }
                else
                {
                    Character otherCharacter = room.GetUnoccuppiedCharacters(this).ElementAt(num - actions.Length);
                    string[] pairActions = Game.FindAllowableActions(room, this, otherCharacter);
                    return new EventContext(pairActions[Game.GetRandom(pairActions.Length)], this, otherCharacter, room);
                }
            }
            else
            {
                int num = Game.GetRandom(actions.Length);
                return new EventContext(actions[num], this, null, room);
            }

        }

        public override Room BeginDay()
        {
            Room ret = Game.CommonRooms[Game.GetRandom(Game.CommonRooms.Length)];
            CharacterLog("Going to " + ret.Name);
            return ret;
        }

        public override EventOption ChooseOption(EventContext context, Event e)
        {
            EventOption[] options = e.GetAvailableOptions(context);
            EventOption chosen = options[Game.GetRandom(options.Length)];
            CharacterLog("Event:" + e.CreateActionDescription(context));
            CharacterLog("Choosing " + EventHelper.ReplaceStrings(chosen.Label, context));
            return chosen;
        }

        public override InformationInstance ChooseInformation()
        {
            return KnownInformation[Game.GetRandom(KnownInformation.Count())];
        }

    }
}
