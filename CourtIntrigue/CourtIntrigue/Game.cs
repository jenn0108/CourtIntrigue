﻿using System;
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
        public static int TICKS_PER_DAY = 5;
        private static int MAX_CHILDREN = 3; // Small for testing purposes.

        private List<Character> characters = new List<Character>();
        private Random random;
        private EventManager eventManager;
        private RoomManager roomManager;
        private InformationManager infoManager;
        private ModifierManager modifierManager;
        private JobManager jobManager;
        private Logger debugLogger;
        public Room[] CommonRooms { get; private set; }
        public int CurrentTime { get; private set; }
        public int CurrentDay { get { return CurrentTime - (CurrentTime % Game.TICKS_PER_DAY); } }
        public Character[] AllCharacters { get { return characters.ToArray(); } }

        private string[] maleNames;
        private string[] femaleNames;
        private string[] familyNames;
        private Dictionary<string, Dynasty> dynasties;

        public Game(Logger logger, int numCharacters, Character player)
        {
            debugLogger = logger;
            eventManager = new EventManager();
            roomManager = new RoomManager();
            infoManager = new InformationManager();
            modifierManager = new ModifierManager();
            jobManager = new JobManager();
            random = new Random();

            CurrentTime = 0;

            maleNames = File.ReadAllLines("Names/male_names.txt");
            femaleNames = File.ReadAllLines("Names/female_names.txt");
            dynasties = new Dictionary<string, Dynasty>();
            familyNames = File.ReadAllLines("Names/family_names.txt");
            foreach (var fam in familyNames)
            {
                dynasties.Add(fam, new Dynasty(fam));
            }

            Dictionary<string, int> badTags = new Dictionary<string, int>();

            //Go load all the xml files in our events directory.
            foreach (var file in Directory.EnumerateFiles("Events", "*.xml"))
            {
                eventManager.LoadEventsFromFile(file, badTags);
            }

            //Go load all the xml files in our rooms directory.
            foreach (var file in Directory.EnumerateFiles("Rooms", "*.xml"))
            {
                roomManager.LoadRoomsFromFile(file);
            }

            //Go load all the xml files in our information directory.
            foreach (var file in Directory.EnumerateFiles("Informations", "*.xml"))
            {
                infoManager.LoadInformationsFromFile(file, badTags);
            }

            //Go load all the xml files in our traits directory.
            foreach (var file in Directory.EnumerateFiles("Traits", "*.xml"))
            {
                modifierManager.LoadTraitsFromFile(file);
            }

            //Go load all the xml files in our modifiers directory.
            foreach (var file in Directory.EnumerateFiles("Modifiers", "*.xml"))
            {
                modifierManager.LoadModifiersFromFile(file, badTags);
            }

            //Go load all the xml files in our jobs directory.
            foreach (var file in Directory.EnumerateFiles("Jobs", "*.xml"))
            {
                jobManager.LoadJobsFromFile(file, badTags);
            }

            foreach (var pair in badTags)
            {
                Log(string.Format("Found unhandled xml tag <{0}> {1} times.", pair.Key, pair.Value));
            }

            CommonRooms = roomManager.GetCommonRooms();

            for (int iCharacter = 0; iCharacter < numCharacters; ++iCharacter)
            {
                characters.Add(GetRandomAICharacter());
            }
            /*
            foreach (var outer in characters)
            {
                foreach (var inner in characters)
                {
                    if (outer == inner)
                        continue;
                    Log(outer.Fullname + " opinion of " + inner.Fullname + " is " + outer.GetOpinionOf(inner));
                }
            }*/

        }

        public string[] FindAllowableActions(Room room, Character initiator, Character target)
        {
            return eventManager.FindAllowableActions(room, initiator, target);
        }

        public Event GetEventById(string id)
        {
            return eventManager.FindEventById(id);
        }

        public Information GetInformationById(string id)
        {
            return infoManager.GetInformationById(id);
        }

        public void BeginDay()
        {
            debugLogger.PrintText("Wake up");
            foreach(var outer in characters)
            {
                foreach(var inner in characters)
                {
                    if (inner == outer)
                        continue;

                    Log(outer.Fullname + " opinion of " + inner.Fullname + " is " + outer.GetOpinionOf(inner));
                }
            }
            foreach (var character in characters)
            {
                modifierManager.EvaluatePrestigeModifiers(character);
            }

            characters = characters.OrderBy(c => c.Dynasty.Prestige).ThenBy(c=>c.Age).ToList();

            foreach (var character in characters)
            {
                character.CurrentRoom = character.BeginDay();
            }


            debugLogger.PrintText("Start day");
        }

        public void Tick()
        {
            debugLogger.PrintText("Begin tick");

            foreach (var room in characters.Select(c => c.CurrentRoom).GroupBy(r => r).Select(r => r.Key))
            {
                room.ResetUnoccupied();
            }

            //If a character accepts a conversion (or other pair action), he may have his turn
            //consumed.  We need to keep track of which characters should have their turns
            //skipped because of this.
            ISet<Character> finishedCharacters = new HashSet<Character>();

            //We're going to need to delay solo actions until after the pair actions so those
            //characters can be interrupted.  This is an important gameplay idea so the last
            //character in the turn order has the opportunity to talk to other characters.
            Dictionary<Character, EventContext> soloActions = new Dictionary<Character, EventContext>();

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
                EventContext action = character.Tick();

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
                    character.MarkBusy();
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

            ++CurrentTime;
        }

        public OpinionModifierInstance CreateOpinionModifier(string identifier, Character character)
        {
            OpinionModifier mod = modifierManager.GetOpinionModifierById(identifier);
            return new OpinionModifierInstance(character, mod, CurrentDay);
        }

        private void ExecuteAction(Character character, EventContext context, ISet<Character> finishedCharacters)
        {
            //Find a matching event to execute.
            Event eventToPlay = eventManager.FindEventForAction(context, random);
            if (eventToPlay != null)
            {
                //We pass in an event results instead of accepting a return value because we want all
                //the event logic along the way to touch the same instance instead of having to worry
                //about merging a number of different instances.
                EventResults results = new EventResults();
                eventToPlay.Execute(results, this, context);

                //Did the target get their turn consumed?
                if (!results.TargetGetsTurn && context.Target != null)
                {
                    //Remove the target from the room (since they are busy) and make sure they don't
                    //get their normal action.
                    finishedCharacters.Add(context.Target);
                    context.Target.MarkBusy();
                }
            }

            //This character is now done.
            finishedCharacters.Add(character);

            // The initiator always gets their turn consumed so remove them from the room.
            character.MarkBusy();
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

        public int TicksToYear(int ticks)
        {
            return ticks / 200;
        }

        public int GetYearInTicks(int year)
        {
            return year * 40 * TICKS_PER_DAY;
        }

        public int GetAdultBirthdate()
        {
            //Age range [25, 50)
            int age = GetRandom(25) + 25;
            //Birthdates start before the start date.
            return GetYearInTicks(-age) - GetRandom( GetYearInTicks(1) );
        }

        public int GetWifeBirthdate(int otherBirthdate)
        {
            //We want a range near the birthdate, mostly should be younger.
            return otherBirthdate + GetRandom(GetYearInTicks(5) - GetYearInTicks(4));
        }

        public int GetChildBirthdate(int motherBirthdate)
        {
            int age = -(motherBirthdate + GetYearInTicks(16));
            return -GetRandom(age);
        }

        public AICharacter GetRandomAICharacter()
        {
            int birthdate = GetAdultBirthdate();
            int wifeBirthdate = GetWifeBirthdate(birthdate);

            Room home = roomManager.MakeUniqueRoom("ESTATE_ROOM");
            // All character have a wife for now.
            Dynasty dynasty = GetRandomDynasty();
            DependentCharacter spouse = new DependentCharacter(GetRandomFemaleName(), wifeBirthdate, dynasty, this, Character.GenderEnum.Female, home);

            // Now add a random number of children with random genders.
            List<DependentCharacter> children = new List<DependentCharacter>();
            int numChildren = GetRandom(MAX_CHILDREN);
            for (int i = 0; i < numChildren; i++)
            {
                Character.GenderEnum gender = (Character.GenderEnum) GetRandom(2);
                string name = gender == Character.GenderEnum.Female ? GetRandomFemaleName() : GetRandomMaleName();
                children.Add(new DependentCharacter(name, GetChildBirthdate(wifeBirthdate), dynasty, this, gender, home));
            }

            AICharacter character = new AICharacter(GetRandomMaleName(), birthdate, dynasty, 0, this, Character.GenderEnum.Male, spouse, children, home);
            modifierManager.AssignInitialTraits(this, character, 4);
            return character;
        }

        public void Log(string txt)
        {
            debugLogger.PrintText(txt);
        }
    }
}
