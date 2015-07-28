using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CourtIntrigue
{
    interface Logger
    {
        void PrintText(string text);
    }
    class Game
    {
        private List<Character> characters = new List<Character>();
        private Random random;
        private Dictionary<Character, Room> chosenRooms = new Dictionary<Character, Room>();
        private EventManager eventManager;
        private Logger debugLogger;
        public Room[] CommonRooms { get; private set; }
        public Game(Logger logger, int numCharacters, Character player)
        {
            debugLogger = logger;
            eventManager = new EventManager();
            random = new Random();

            for (int iCharacter = 0; iCharacter < numCharacters; ++iCharacter )
            {
                string name = new string((char)('A' + iCharacter), 1);
                characters.Add(new AICharacter(name, 0, this));
            }
            CommonRooms = new Room[2] { new Room("Town", false, new string[] { Action.PUBLIC_URINATION_ACTION }), new Room("Court", true, new string[] { Action.EAVESDROP_ACTION }) };

            eventManager.LoadEventsFromFile("Events/testevents.xml");
        }

        public void BeginDay()
        {
            debugLogger.PrintText("Wake up");
            chosenRooms.Clear();
            foreach (var character in characters)
            {
                Room room = character.BeginDay();
                chosenRooms.Add(character, room);
                room.AddCharacter(character);
            }


            debugLogger.PrintText("Start day");
        }

        public void Tick()
        {
            debugLogger.PrintText("Begin tick");
            foreach (var character in characters)
            {
                Action action = character.Tick(chosenRooms[character]);
                if (action.Target == null )
                {
                    debugLogger.PrintText(character.Name + " chose " + action.Identifer);
                }
                else
                {
                    chosenRooms[character].RemoveCharacter(character);
                    debugLogger.PrintText(character.Name + " chose " + action.Identifer + " with " + action.Target.Name);
                }

                // event.dologic();
            }


            debugLogger.PrintText("End tick");
        }

        /// <summary>
        /// Returns a random number between 0 and max-1.
        /// </summary>
        /// <param name="max"></param>
        /// <returns></returns>
        public int GetRandom(int max)
        {
            return random.Next(max);
        }

        public void Log(string txt)
        {
            debugLogger.PrintText(txt);
        }
    }
}
