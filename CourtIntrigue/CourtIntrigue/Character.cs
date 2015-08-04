using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CourtIntrigue
{
    class Character
    {
        public enum GenderEnum { Female, Male };

        public string Name { get; private set; }
        public int Money { get; private set; }
        public Dynasty Dynasty { get; private set; }
        public GenderEnum Gender { get; private set; }
        public DependentCharacter Spouse { get; private set; }
        public List<DependentCharacter> Children { get; private set; }
        protected Room room;
        protected List<InformationInstance> KnownInformation { get; private set; }
        protected ISet<InformationInstance> history = new HashSet<InformationInstance>();
        protected Dictionary<string, Trait> traits { get; private set; }
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

        public Character(string name, Dynasty dynasty, int money, Game game, GenderEnum gender, DependentCharacter spouse, List<DependentCharacter> children)
        {
            Name = name;
            Dynasty = dynasty;
            Money = money;
            Game = game;
            Gender = gender;
            Spouse = spouse;
            Children = children;
            KnownInformation = new List<InformationInstance>();
            traits = new Dictionary<string, Trait>();
            prestigeModifiers = new HashSet<PrestigeModifier>();
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
                    opinion += mod.GetChange(Game.CurrentTime);
                }
            }
            return opinion;
        }

        public Room BeginDay()
        {
            //Remove expired opinion modifiers
            foreach(var pair in opinionModifiers)
            {
                ISet<OpinionModifierInstance> expiredMods = new HashSet<OpinionModifierInstance>();
                foreach(var mod in pair.Value)
                {
                    if(mod.IsExpired(Game.CurrentTime))
                    {
                        expiredMods.Add(mod);
                    }
                }

                pair.Value.ExceptWith(expiredMods);
            }

            return OnBeginDay();
        }

        public EventContext Tick()
        {
            return OnTick();
        }

        public virtual EventContext OnTick()
        {
            throw new NotImplementedException();
        }

        public virtual Room OnBeginDay()
        {
            throw new NotImplementedException();
        }

        public virtual EventOption ChooseOption(EventContext action, Event e)
        {
            throw new NotImplementedException();
        }

        public virtual InformationInstance ChooseInformation()
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
            return true;
        }

        public bool HasInformation()
        {
            return KnownInformation.Count > 0;
        }

        public void AddTrait(Trait trait)
        {
            CharacterLog(" Gained the trait: " + trait.Label);
            traits.Add(trait.Identifier, trait);
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

        public override string ToString()
        {
            return Fullname;
        }
    }
}
