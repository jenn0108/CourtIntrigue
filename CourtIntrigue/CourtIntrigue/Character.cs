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
        public List<DependentCharacter> Dependents { get; private set; }
        public List<InformationInstance> KnownInformation { get; private set; }
        protected Dictionary<string,Trait> Traits { get; private set; }
        protected Game Game { get; private set; }

        public string Fullname
        {
            get { return Name + " " + Dynasty.Name; }
        }

        public Character(string name, Dynasty dynasty, int money, Game game, GenderEnum gender, List<DependentCharacter> dependents)
        {
            Name = name;
            Dynasty = dynasty;
            Money = money;
            Game = game;
            Gender = gender;
            Dependents = dependents;
            KnownInformation = new List<InformationInstance>();
            Traits = new Dictionary<string, Trait>();
        }

        public int GetOpinionOf(Character character)
        {
            int opinion = 0;
            foreach (var trait in Traits.Values)
            {
                if (character.Traits.ContainsKey(trait.Identifier))
                {
                    opinion += trait.SameOpinion;
                }
                foreach (var oppositeId in trait.Opposites)
                {
                    if (character.Traits.ContainsKey(oppositeId))
                    {
                        opinion += trait.OppositeOpinion;
                    }
                }
            }
            return opinion;
        }

        public virtual EventContext Tick(Room room)
        {
            throw new NotImplementedException();
        }

        public virtual Room BeginDay()
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

        public void AddInformation(InformationInstance info)
        {
            KnownInformation.Add(info);
        }

        internal void AddTrait(Trait trait)
        {
            CharacterLog(" Gained the trait: " + trait.Label);
            Traits.Add(trait.Identifier, trait);
        }

        protected void CharacterLog(string text)
        {
            Game.Log(Fullname + ": " + text);
        }

    }
}
