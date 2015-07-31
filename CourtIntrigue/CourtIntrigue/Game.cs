using System;
using System.Collections.Generic;
using System.IO;
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
        private static int MAX_CHILDREN = 3; // Small for testing purposes.

        private List<Character> characters = new List<Character>();
        private Random random;
        private Dictionary<Character, Room> chosenRooms = new Dictionary<Character, Room>();
        private EventManager eventManager;
        private RoomManager roomManager;
        private Logger debugLogger;
        public Room[] CommonRooms { get; private set; }
        public DateTime CurrentDate { get; private set; }

        private string[] maleNames;
        private string[] femaleNames;
        private string[] familyNames;
        private Dictionary<string, Dynasty> dynasties;

        public Game(Logger logger, int numCharacters, Character player)
        {
            debugLogger = logger;
            eventManager = new EventManager();
            roomManager = new RoomManager();
            random = new Random();

            CurrentDate = new DateTime(1066, 10, 13);//Day before the Battle of Hastings, ok for now.

            maleNames = File.ReadAllLines("Names/male_names.txt");
            femaleNames = File.ReadAllLines("Names/female_names.txt");
            dynasties = new Dictionary<string, Dynasty>();
            familyNames = File.ReadAllLines("Names/family_names.txt");
            foreach (var fam in familyNames)
            {
                dynasties.Add(fam, new Dynasty(fam));
            }

            for (int iCharacter = 0; iCharacter < numCharacters; ++iCharacter )
            {
                characters.Add(GetRandomAICharacter());
            }

            //Go load all the xml files in our events directory.
            foreach(var file in Directory.EnumerateFiles("Events", "*.xml"))
            {
                eventManager.LoadEventsFromFile(file);
            }

            //Go load all the xml files in our rooms directory.
            foreach (var file in Directory.EnumerateFiles("Rooms", "*.xml"))
            {
                roomManager.LoadRoomsFromFile(file);
            }

            CommonRooms = roomManager.GetCommonRooms();
        }

        public void BeginDay()
        {
            CurrentDate = CurrentDate.AddDays(1.0);

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

            //If a character accepts a conversion (or other pair action), he may have his turn
            //consumed.  We need to keep track of which characters should have their turns
            //skipped because of this.
            ISet<Character> finishedCharacters = new HashSet<Character>();

            //We're going to need to delay solo actions until after the pair actions so those
            //characters can be interrupted.  This is an important gameplay idea so the last
            //character in the turn order has the opportunity to talk to other characters.
            Dictionary<Character, Action> soloActions = new Dictionary<Character, Action>();

            //Give each player a turn according to turn order.
            foreach (var character in characters)
            {

                //Check for characters that accepted another action.
                if (finishedCharacters.Contains(character))
                {
                    debugLogger.PrintText("Skipping " + character.Name);
                    continue;
                }

                //Give the character their turn.
                Action action = character.Tick(chosenRooms[character]);

                if (action.Target == null)
                {
                    debugLogger.PrintText(character.Name + " chose " + action.Identifer);

                    //This character has chosen a solo action.  Queue it up and move on.
                    soloActions.Add(character, action);
                    continue;
                }
                else
                {
                    debugLogger.PrintText(character.Name + " chose " + action.Identifer + " with " + action.Target.Name);

                    //This character is unavailable for interaction because he is busy.
                    chosenRooms[character].RemoveCharacter(character);
                }
                
                ExecuteAction(character, action, finishedCharacters);

            }

            debugLogger.PrintText("Solo actions");

            foreach (var pair in soloActions)
            {
                //Like the main action loop, if the character accepted a pair action before this point,
                //it replaces their normal action.
                if (finishedCharacters.Contains(pair.Key))
                    continue;

                ExecuteAction(pair.Key, pair.Value, finishedCharacters);
            }

            debugLogger.PrintText("End tick");
        }

        private void ExecuteAction(Character character, Action action, ISet<Character> finishedCharacters)
        {
            //Find a matching event to execute.
            Event eventToPlay = eventManager.FindEventForAction(action, random);
            if (eventToPlay != null)
            {
                //We pass in an event results instead of accepting a return value because we want all
                //the event logic along the way to touch the same instance instead of having to worry
                //about merging a number of different instances.
                EventResults results = new EventResults();
                eventToPlay.Execute(results, eventManager, action);

                //Did the target get their turn consumed?
                if (!results.TargetGetsTurn && action.Target != null)
                {
                    //Remove the target from the room (since they are busy) and make sure they don't
                    //get their normal action.
                    finishedCharacters.Add(action.Target);
                    chosenRooms[character].RemoveCharacter(action.Target);
                }
            }

            //This character is now done.
            finishedCharacters.Add(character);

            // The initiator always gets their turn consumed so remove them from the room.
            chosenRooms[character].RemoveCharacter(character);
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

        public string GetRandomMaleName()
        {
            return maleNames[GetRandom(maleNames.Length)];
        }

        public string GetRandomFemaleName()
        {
            return femaleNames[GetRandom(femaleNames.Length)];
        }

        public Dynasty GetRandomDynasty()
        {
            string lastName = familyNames[GetRandom(familyNames.Length)];
            return dynasties[lastName];
        }

        public AICharacter GetRandomAICharacter()
        {
            // All character have a wife for now.
            Dynasty dynasty = GetRandomDynasty();
            List<DependentCharacter> dependants = new List<DependentCharacter>();
            dependants.Add(new DependentCharacter(GetRandomFemaleName(), dynasty, this, Character.GenderEnum.Female));

            // Now add a random number of children with random genders.
            int numChildren = GetRandom(MAX_CHILDREN);
            for (int i = 0; i < numChildren; i++)
            {
                Character.GenderEnum gender = (Character.GenderEnum) GetRandom(2);
                string name = gender == Character.GenderEnum.Female ? GetRandomFemaleName() : GetRandomMaleName();
                dependants.Add(new DependentCharacter(name, dynasty, this, gender));
            }

            return new AICharacter(GetRandomMaleName(), dynasty, 0, this, Character.GenderEnum.Male, dependants);
        }

        public void Log(string txt)
        {
            debugLogger.PrintText(txt);
        }
    }
}
