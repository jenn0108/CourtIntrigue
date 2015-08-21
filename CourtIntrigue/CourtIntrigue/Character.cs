using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace CourtIntrigue
{
    class Character
    {
        public enum GenderEnum { Female, Male };
        public DNA DNA { get; private set; }
        public string Name { get; private set; }
        public int BirthDate { get; private set; }
        public int Money { get; private set; }
        public int Prestige { get; private set; }
        public int PrestigeRank { get; set; }
        public double ObserveModifier { get; private set; }
        public int WillPower { get; private set; }
        public Dynasty Dynasty { get; private set; }
        public GenderEnum Gender { get; private set; }
        public DependentCharacter Spouse { get; private set; }
        public List<DependentCharacter> Children { get; private set; }
        public Room Home { get; private set; }
        protected Room room;
        protected List<InformationInstance> KnownInformation { get; private set; }
        protected ISet<InformationInstance> history = new HashSet<InformationInstance>();
        protected Dictionary<string, Trait> traits { get; private set; }
        protected Dictionary<string, Job> jobs { get; private set; }
        protected Dictionary<string, int> variables = new Dictionary<string, int>();
        protected ISet<PrestigeModifier> prestigeModifiers { get; private set; }
        protected Dictionary<Character, ISet<OpinionModifierInstance>> opinionModifiers = new Dictionary<Character, ISet<OpinionModifierInstance>>();
        protected Game Game { get; private set; }

        public string Fullname
        {
            get { return Name + " " + Dynasty.Name; }
        }

        public IEnumerable<Trait> Traits
        {
            get { return traits.Values; }
        }

        public IEnumerable<PrestigeModifier> CurrentPrestigeModifiers
        {
            get { return prestigeModifiers; }
        }

        public Room CurrentRoom
        {
            get { return room; }
            set
            {
                if (room != null)
                {
                    room.RemoveCharacter(this);
                }
                room = value;
                if (room != null)
                {
                    room.AddCharacter(this);
                }
            }
        }

        public int Age
        {
            get { return Game.TicksToYear(Game.CurrentDay - BirthDate); }
        }

        public Character(string name, int birthdate, Dynasty dynasty, int money, Game game, GenderEnum gender)
        {
            Name = name;
            BirthDate = birthdate;
            Dynasty = dynasty;
            Money = money;
            WillPower = Game.MAX_WILLPOWER;
            Game = game;
            Gender = gender;
            KnownInformation = new List<InformationInstance>();
            traits = new Dictionary<string, Trait>();
            jobs = new Dictionary<string, Job>();
            prestigeModifiers = new HashSet<PrestigeModifier>();
        }

        public void AssignFamily(DependentCharacter spouse, List<DependentCharacter> children, Room home, DNA dna)
        {
            DNA = dna;
            Home = home;
            Spouse = spouse;
            Children = children;
            CharacterLog("Created family for " + Name + " with spouse: " + spouse.Name + " and children: " + string.Join(", ", children.Select(c => c.Name + "(" + c.Gender.ToString() + ")")));

            //Assign the home to the dependents as well.
            spouse.Home = home;
            foreach(var c in children)
            {
                c.Home = home;
            }
        }

        public IEnumerable<string> GetVariableNames()
        {
            return variables.Keys;
        }

        public Bitmap GetPortrait()
        {
            return Game.GetPortrait(DNA);
        }

        public Weights GetWeights()
        {
            return new Weights(this);
        }

        public IEnumerable<OpinionModifierInstance> GetOpinionModifiersAbout(Character character)
        {
            ISet<OpinionModifierInstance> list;
            if (opinionModifiers.TryGetValue(character, out list))
            {
                return list;
            }
            return Enumerable.Empty<OpinionModifierInstance>();
        }

        public IEnumerable<InformationInstance> GetInformationAbout(Character character)
        {
            if(character == this)
            {
                foreach(var info in history)
                {
                    yield return info;
                }
            }
            else
            {
                foreach (var info in KnownInformation)
                {
                    if (info.IsAbout(character))
                        yield return info;
                }
            }
        }

        public int GetOpinionOf(Character character)
        {
            int opinion = 0;
            foreach (var trait in traits.Values)
            {
                if (character.traits.ContainsKey(trait.Identifier))
                {
                    opinion += trait.SameOpinion;
                }
                foreach (var oppositeId in trait.Opposites)
                {
                    if (character.traits.ContainsKey(oppositeId))
                    {
                        opinion += trait.OppositeOpinion;
                    }
                }
            }
            ISet<OpinionModifierInstance> mods = null;
            if(opinionModifiers.TryGetValue(character, out mods))
            {
                foreach(var mod in mods)
                {
                    opinion += mod.GetChange(Game.CurrentDay);
                }
            }
            return opinion;
        }

        public void PrestigeChange(int change)
        {
            Prestige += change;
        }

        public Room BeginDay()
        {
            //Remove expired opinion modifiers
            foreach(var pair in opinionModifiers)
            {
                ISet<OpinionModifierInstance> expiredMods = new HashSet<OpinionModifierInstance>();
                foreach(var mod in pair.Value)
                {
                    if(mod.IsExpired(Game.CurrentDay))
                    {
                        expiredMods.Add(mod);
                    }
                }

                pair.Value.ExceptWith(expiredMods);
            }

            KnownInformation = KnownInformation.Where(info => !info.IsExpired(Game.CurrentDay)).ToList();

            Room[] rooms = Game.CommonRooms.Union(Home.Yield()).ToArray();

            int index = OnBeginDay(rooms);
            if (index == Game.CommonRooms.Length)
            {
                CharacterLog("Staying home");
            }
            else
            {
                Room ret = Game.CommonRooms[index];
                CharacterLog("Going to " + ret.Name);
            }
            return rooms[index];
        }

        public EventContext Tick()
        {
            //A Character's observe modifier is always reset to 1 at the start of a tick.
            //Many events will change this.  Remember that information is doled out at the
            //end of the tick
            ObserveModifier = 1.0;

            if (++WillPower > Game.MAX_WILLPOWER)
                WillPower = Game.MAX_WILLPOWER;

            Dictionary<Character, Action[]> characterActions = new Dictionary<Character, Action[]>();
            foreach(var otherCharacter in room.GetUnoccuppiedCharacters(this))
            {
                Action[] pairActions = Game.FindAllowableActions(room, this, otherCharacter);
                if(pairActions != null && pairActions.Length > 0)
                    characterActions.Add(otherCharacter, pairActions);
            }

            Action[] soloActions = Game.GetActionsById(room.SoloActions);

            return OnTick(soloActions, characterActions);
        }

        public int ChooseOption(EventOption[] options, int[] willpowerCost, EventContext context, Event e)
        {
            return OnChooseOption(options, willpowerCost, context, e);
        }

        public InformationInstance ChooseInformationAbout(InformationType type, Character about)
        {
            InformationInstance[] information;
            if (type == InformationType.Positive)
                information = KnownInformation.Where(info => info.EvaluateOnTold(about, this, Game) > 0.0).ToArray();
            else if (type == InformationType.Negative)
                information = KnownInformation.Where(info => info.EvaluateOnTold(about, this, Game) < 0.0).ToArray();
            else
                information = KnownInformation.ToArray();
            int index = OnChooseInformation(information);
            return information[index];
        }

        public Character ChooseCharacter()
        {
            int index = OnChooseCharacter(Game.AllCharacters);
            return Game.AllCharacters[index];
        }

        public virtual EventContext OnTick(Action[] soloActions, Dictionary<Character, Action[]> characterActions)
        {
            throw new NotImplementedException();
        }

        public virtual int OnBeginDay(Room[] rooms)
        {
            throw new NotImplementedException();
        }

        public virtual int OnChooseOption(EventOption[] options, int[] willpowerCost, EventContext context, Event e)
        {
            throw new NotImplementedException();
        }

        public virtual int OnChooseInformation(InformationInstance[] knownInformation)
        {
            throw new NotImplementedException();
        }

        public virtual int OnChooseCharacter(Character[] characters)
        {
            throw new NotImplementedException();
        }

        public virtual void OnLearnInformation(InformationInstance info)
        {
            throw new NotImplementedException();
        }

        public void AddHistory(InformationInstance info)
        {
            history.Add(info);

            if (history.Count > 1)
                return;
        }

        public bool AddInformation(InformationInstance info)
        {
            //You can't learn about something you actually did.
            if (history.Contains(info))
                return false;

            if (KnownInformation.Contains(info))
                return false;

            KnownInformation.Add(info);

            //We've just learned an info.
            OnLearnInformation(info);
            return true;
        }

        public bool HasInformationAbout(Character about, InformationType type)
        {
            foreach(var info in KnownInformation)
            {
                if (info.IsAbout(about))
                {
                    double result = info.EvaluateOnTold(about, this, Game);
                    if (type == InformationType.Positive && result > 0.0)
                        return true;
                    else if (type == InformationType.Negative && result < 0.0)
                        return true;
                    else if (type == InformationType.Neutral && result == 0.0)
                        return true;
                }
            }
            return false;
        }

        public bool HasJob(string jobId)
        {
            return jobs.ContainsKey(jobId);
        }

        public void GiveJob(Job job)
        {
            jobs.Add(job.Identifier, job);
        }

        public void FireFromJob(Job job)
        {
            jobs.Remove(job.Identifier);
        }

        public void AddTrait(Trait trait)
        {
            traits.Add(trait.Identifier, trait);
        }

        public bool HasTrait(string traitId)
        {
            return traits.ContainsKey(traitId);
        }

        protected void CharacterLog(string text)
        {
            Game.Log(Fullname + ": " + text);
        }

        public void AddPrestigeModifier(PrestigeModifier modifier)
        {
            prestigeModifiers.Add(modifier);
        }

        public void RemovePrestigeModifier(PrestigeModifier modifier)
        {
            prestigeModifiers.Remove(modifier);
        }

        public void AddOpinionModifier(OpinionModifierInstance mod)
        {
            ISet<OpinionModifierInstance> list;
            if (!opinionModifiers.TryGetValue(mod.Target, out list))
            {
                list = new HashSet<OpinionModifierInstance>();
                opinionModifiers.Add(mod.Target, list);
            }
            list.Add(mod);
        }

        public void MarkBusy()
        {
            if (CurrentRoom != null)
            {
                CurrentRoom.MarkBusy(this);
            }
        }

        public int GetVariable(string name)
        {
            int val = 0;
            variables.TryGetValue(name, out val);
            return val;
        }

        public void SetVariable(string name, int val)
        {
            if (variables.ContainsKey(name))
                variables[name] = val;
            else
                variables.Add(name, val);
        }

        public void SpendGold(int gold)
        {
            if (Money < gold)
                throw new InvalidOperationException("Character has insufficient gold.");

            Money -= gold;
        }

        public void GetGold(int gold)
        {
            Money += gold;
        }

        public void SpendWillpower(int cost)
        {
            if (cost > 0)
            {
                if (WillPower < cost)
                    throw new InvalidOperationException("Character has insufficient willpower.");

                WillPower -= cost;
            }
        }

        public void MultiplyObserveModifier(double multiplier)
        {
            ObserveModifier *= multiplier;
        }

        public override string ToString()
        {
            return Fullname;
        }
    }
}
