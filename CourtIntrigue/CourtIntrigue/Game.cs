using System;
using System.Collections.Generic;
using System.Drawing;
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
        public static int MAX_WILLPOWER = 50;
        private static int MAX_CHILDREN = 3; // Small for testing purposes.

        private List<Character> characters = new List<Character>();
        private Random random;
        private EventManager eventManager;
        private RoomManager roomManager;
        private InformationManager infoManager;
        private ModifierManager modifierManager;
        private JobManager jobManager;
        private CharacterVisualizationManager cvManager;
        private Logger debugLogger;
        public Room[] CommonRooms { get; private set; }
        public int CurrentTime { get; private set; }
        public int CurrentDay { get { return CurrentTime - (CurrentTime % Game.TICKS_PER_DAY); } }
        public Character[] AllCharacters { get { return characters.ToArray(); } }

        private string[] maleNames;
        private string[] femaleNames;
        private string[] familyNames;
        private Dictionary<string, Dynasty> dynasties;

        public Game(Logger logger, int numCharacters)
        {
            debugLogger = logger;
            eventManager = new EventManager();
            roomManager = new RoomManager();
            infoManager = new InformationManager();
            modifierManager = new ModifierManager();
            jobManager = new JobManager();
            cvManager = new CharacterVisualizationManager();
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

            Counter<string> badTags = new Counter<string>();

            //Go load all the xml files in our events directory.
            foreach (var file in Directory.EnumerateFiles("Events", "*.xml"))
            {
                eventManager.LoadEventsFromFile(file, badTags);
            }

            //Go load all the xml files in our actions directory.
            foreach (var file in Directory.EnumerateFiles("Actions", "*.xml"))
            {
                eventManager.LoadActionsFromFile(file, badTags);
            }

            //Go load all the xml files in our rooms directory.
            foreach (var file in Directory.EnumerateFiles("Rooms", "*.xml"))
            {
                roomManager.LoadRoomsFromFile(file, badTags);
            }

            //Go load all the xml files in our information directory.
            foreach (var file in Directory.EnumerateFiles("Informations", "*.xml"))
            {
                infoManager.LoadInformationsFromFile(file, badTags);
            }

            //Go load all the xml files in our traits directory.
            foreach (var file in Directory.EnumerateFiles("Traits", "*.xml"))
            {
                modifierManager.LoadTraitsFromFile(file, badTags);
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

            cvManager.LoadFromDirectory("Graphics/Portraits_2", badTags);

            foreach (var pair in badTags)
            {
                Log(string.Format("Found unhandled xml tag <{0}> {1} times.", pair.Key, pair.Value));
            }

            CommonRooms = roomManager.GetCommonRooms();

            for (int iCharacter = 0; iCharacter < numCharacters; ++iCharacter)
            {
                characters.Add(GetRandomAICharacter());
            }
            OrderCharacters();
            jobManager.InitializeJobs(characters, this);

        }

        public Bitmap GetPortrait(DNA dna)
        {
            return cvManager.ComposeFace(dna);
        }

        public void AddPlayer(Character player)
        {
            AssignFamily(player);
            modifierManager.AssignInitialTraits(this, player, 4);
            characters.Add(player);
            OrderCharacters();
        }

        public void ExecuteDayEvents(Character character)
        {
            eventManager.ExecuteDayEvents(character, this);
        }

        public Action[] FindAllowableActions(Room room, Character initiator, Character target)
        {
            return eventManager.FindAllowableActions(room, initiator, target, this);
        }

        public Action[] GetActionsById(string[] ids)
        {
            return eventManager.GetActionsById(ids);
        }

        public Event GetEventById(string id)
        {
            return eventManager.FindEventById(id);
        }

        public Information GetInformationById(string id)
        {
            return infoManager.GetInformationById(id);
        }

        public Job GetJobById(string id)
        {
            return jobManager.GetJobById(id);
        }

        public void GiveJobTo(Job job, Character newHolder)
        {
            jobManager.GiveJobTo(job, newHolder, this);
        }

        public void BeginDay()
        {
            debugLogger.PrintText("Wake up");
            foreach (var character in characters)
            {
                modifierManager.EvaluatePrestigeModifiers(character, this);
            }

            OrderCharacters();


            //Cache the character so that babies don't screw up the list when they are
            //added during the begin day events (which detect end of pregnancy.)
            var characterCache = new List<Character>(characters);
            foreach (var character in characterCache)
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

            //Keep track of the information that is observable in each room.  We'll give each character a go at it later.
            Dictionary<Room, List<ObservableInformation>> observableInformationByRoom = new Dictionary<Room, List<ObservableInformation>>();

            //If a character accepts a conversion (or other pair action), he may have his turn
            //consumed.  We need to keep track of which characters should have their turns
            //skipped because of this.
            ISet<Character> finishedCharacters = new HashSet<Character>();

            //We're going to need to delay solo actions until after the pair actions so those
            //characters can be interrupted.  This is an important gameplay idea so the last
            //character in the turn order has the opportunity to talk to other characters.
            Dictionary<Character, ActionDescriptor> soloActions = new Dictionary<Character, ActionDescriptor>();

            var charactersByRoom = characters.GroupBy(c => c.CurrentRoom).OrderByDescending(g=>g.Key.Priority);

            foreach (var pair in charactersByRoom)
            {
                Log("Handling Room: " + pair.Key);
                var roomCharacters = pair.OrderBy(c => c.PrestigeRank);
                //Give each player a turn according to turn order.
                foreach (var character in roomCharacters)
                {

                    //Check for characters that accepted another action.
                    if (finishedCharacters.Contains(character))
                    {
                        debugLogger.PrintText("Skipping " + character.Name);
                        continue;
                    }

                    bool characterDone = true;
                    do
                    {
                        //Give the character their turn.
                        ActionDescriptor actionDescriptor = character.Tick();

                        switch (actionDescriptor.Action.Type)
                        {
                            case ActionType.Delayed:
                                {
                                    debugLogger.PrintText(character.Name + " chose " + actionDescriptor.Action.Identifier);

                                    //This character has chosen a solo action.  Queue it up and move on.
                                    soloActions.Add(character, actionDescriptor);
                                    characterDone = true;
                                }
                                break;

                            case ActionType.Pair:
                                {
                                    debugLogger.PrintText(character.Name + " chose " + actionDescriptor.Action.Identifier + " with " + actionDescriptor.Target.Name);

                                    //Do the action.
                                    characterDone = ExecuteAction(character, actionDescriptor, finishedCharacters, observableInformationByRoom);
                                }
                                break;

                            case ActionType.Immediate:
                                {
                                    debugLogger.PrintText(character.Name + " chose " + actionDescriptor.Action.Identifier);

                                    //Do the action.
                                    characterDone = ExecuteAction(character, actionDescriptor, finishedCharacters, observableInformationByRoom);
                                }
                                break;

                            case ActionType.Internal:
                                {
                                    //Characters aren't allowed to deliberately call Internal actions.
                                    throw new Exception("Event type not allowed");
                                }
                        }

                    } while (!characterDone);
                }
            }

            debugLogger.PrintText("Solo actions");

            foreach (var pair in soloActions)
            {
                //Like the main action loop, if the character accepted a pair action before this point,
                //it replaces their normal action.
                if (finishedCharacters.Contains(pair.Key))
                    continue;

                ExecuteAction(pair.Key, pair.Value, finishedCharacters, observableInformationByRoom);
            }

            //Now that all the action has happened, we can give each character a chance to observe the information.
            foreach(var character in characters)
            {
                //First off, is there any information to observe in this character's room?
                List<ObservableInformation> infos = null;
                if (!observableInformationByRoom.TryGetValue(character.CurrentRoom, out infos))
                    continue;

                foreach(var info in infos)
                {
                    if(random.Next(100) < info.Chance * character.ObserveModifier)
                    {
                        if (character.AddInformation(info.Info))
                        {
                            Log(character.Name + " learned an information.");

                            if (info.Teller == null)
                                info.Info.ExecuteOnObserve(character, this, character.CurrentRoom);
                            else
                                info.Info.ExecuteOnTold(character, this, character.CurrentRoom);
                        }
                    }
                }
            }

            debugLogger.PrintText("End tick");

            ++CurrentTime;
        }

        public IEnumerable<Room> GetRooms(Character character)
        {
            return roomManager.GetRooms(character, this);
        }

        public Character[] FilterCharacters(ILogic requirements, EventContext context)
        {
            List<Character> matchingCharacters = new List<Character>();
            foreach (Character character in this.AllCharacters)
            {
                context.PushScope(character);
                if (requirements.Evaluate(context, this))
                {
                    matchingCharacters.Add(character);
                }
                context.PopScope();
            }
            return matchingCharacters.ToArray();
        }

        public OpinionModifier GetOpinionModifier(string identifier)
        {
            return modifierManager.GetOpinionModifierById(identifier);
        }

        public OpinionModifierInstance CreateOpinionModifier(string identifier, Character character)
        {
            OpinionModifier mod = modifierManager.GetOpinionModifierById(identifier);
            return new OpinionModifierInstance(character, mod, CurrentDay);
        }

        private void OrderCharacters()
        {
            //Order all characters by prestige.  Higher prestige characters go first in the turn order.
            characters = characters.OrderByDescending(c => c.Prestige).ThenBy(c => c.BirthDate).ToList();

            //Update the prestige rank on each of the characters so it can be quickly used instead of calculated.
            for (int i = 0; i < characters.Count; ++i)
            {
                characters[i].PrestigeRank = i;
            }
        }

        private bool ExecuteAction(Character character, ActionDescriptor actionDescriptor, ISet<Character> finishedCharacters, Dictionary<Room, List<ObservableInformation>> informations)
        {
            //Find a matching event to execute.
            Event eventToPlay = eventManager.FindEventForAction(actionDescriptor, this);

            if (eventToPlay != null)
            {
                //We pass in an event results instead of accepting a return value because we want all
                //the event logic along the way to touch the same instance instead of having to worry
                //about merging a number of different instances.
                EventResults results = new EventResults();
                eventToPlay.Execute(results, this, new EventContext(actionDescriptor));

                //Did the target get their turn consumed?
                if (!results.TargetGetsTurn && actionDescriptor.Target != null)
                {
                    //Remove the target from the room (since they are busy) and make sure they don't
                    //get their normal action.
                    finishedCharacters.Add(actionDescriptor.Target);
                    actionDescriptor.Target.MarkBusy();
                }

                if(results.HasInformation)
                {
                    //Add any information to the list of things that characters in the room might observe.
                    List<ObservableInformation> infos = null;
                    if (!informations.TryGetValue(character.CurrentRoom, out infos))
                    {
                        infos = new List<ObservableInformation>();
                        informations.Add(character.CurrentRoom, infos);
                    }
                    infos.AddRange(results.ObservableInformation);
                }

                if (results.ContinueTurn)
                {
                    //The turn is incomplete.  The character isn't busy or finished yet.
                    return false;
                }
            }

            //This character is now done.
            finishedCharacters.Add(character);

            // The initiator always gets their turn consumed so remove them from the room.
            character.MarkBusy();

            //The turn completed successfully.
            return true;
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

        public static int GetYearInTicks(int year)
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
            Dynasty dynasty = GetRandomDynasty();
            int birthdate = GetAdultBirthdate();
            AICharacter character = new AICharacter(GetRandomMaleName(), birthdate, dynasty, 100, this, Gender.Male);

            AssignFamily(character);
            modifierManager.AssignInitialTraits(this, character, 4);
            return character;
        }

        private void AssignFamily(Character character)
        {
            int wifeBirthdate = GetWifeBirthdate(character.BirthDate);

            Room home = roomManager.MakeUniqueRoom("ESTATE_ROOM");
            // All character have a wife for now.
            AICharacter spouse = new AICharacter(GetRandomFemaleName(), wifeBirthdate, character.Dynasty, 50, this, Gender.Female);
            characters.Add(spouse);

            // Now add a random number of children with random genders.
            List<Character> children = new List<Character>();
            int numChildren = GetRandom(MAX_CHILDREN);
            for (int i = 0; i < numChildren; i++)
            {
                Gender gender = (Gender)GetRandom(2);
                string name = gender == Gender.Female ? GetRandomFemaleName() : GetRandomMaleName();

                Character child = new AICharacter(name, GetChildBirthdate(wifeBirthdate), character.Dynasty, 5, this, gender);
                children.Add(child);
                characters.Add(child);
            }

            character.AssignFamily(spouse, children, home, cvManager.CreateRandomDNA(character, this), cvManager.CreateRandomDNA(spouse, this));
        }
        
        public DNA CreateChildDNA(Character character, DNA father, DNA mother)
        {
            return cvManager.CreateChildDNA(character, father, mother, this);
        }

        public void CreateChild(Character mother, Character father)
        {
            Gender gender = (Gender)GetRandom(2);
            string name = gender == Gender.Female ? GetRandomFemaleName() : GetRandomMaleName();

            Character child = new AICharacter(name, CurrentTime, mother.Spouse.Dynasty, 0, this, gender);
            child.AssignParents(mother, father);

            characters.Add(child);
        }

        public void Log(string txt)
        {
            debugLogger.PrintText(txt);
        }
    }
}
