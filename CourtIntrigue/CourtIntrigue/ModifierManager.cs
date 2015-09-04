using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace CourtIntrigue
{
    class ModifierManager
    {
        private Dictionary<string, Trait> traits = new Dictionary<string, Trait>();
        private List<PrestigeModifier> prestigeModifiers = new List<PrestigeModifier>();
        private Dictionary<string, OpinionModifier> opinionModifiers = new Dictionary<string, OpinionModifier>();

        private Trait ReadTrait(XmlReader reader, Counter<string> badTags)
        {
            string identifier = null;
            string description = null;
            string label = null;
            int oppositeOpinion = 0;
            int sameOpinion = 0;
            string[] opposites = new string[0];
            
            while (reader.Read())
            {
                if (reader.NodeType == XmlNodeType.Element && reader.Name == "id")
                {
                    identifier = reader.ReadElementContentAsString();
                }
                else if (reader.NodeType == XmlNodeType.Element && reader.Name == "description")
                {
                    description = reader.ReadElementContentAsString();
                }
                else if (reader.NodeType == XmlNodeType.Element && reader.Name == "label")
                {
                    label = reader.ReadElementContentAsString();
                }
                else if (reader.NodeType == XmlNodeType.Element && reader.Name == "opposites")
                {
                    opposites = XmlHelper.ReadList(reader, "opposite", badTags);
                }
                else if (reader.NodeType == XmlNodeType.Element && reader.Name == "opposite_opinion")
                {
                    oppositeOpinion = reader.ReadElementContentAsInt();
                }
                else if (reader.NodeType == XmlNodeType.Element && reader.Name == "same_opinion")
                {
                    sameOpinion = reader.ReadElementContentAsInt();
                }
                else if (reader.NodeType == XmlNodeType.Element)
                {
                    badTags.Increment(reader.Name);
                }
                else if (reader.NodeType == XmlNodeType.EndElement && reader.Name == "trait")
                {
                    break;
                }
            }
            return new Trait(identifier, label, description, sameOpinion, oppositeOpinion, opposites);
        }

        private PrestigeModifier ReadPrestigeModifier(XmlReader reader, Counter<string> badTags)
        {
            string identifier = null;
            string description = null;
            string label = null;
            int dailyChange = 0;
            ILogic requirements = Logic.TRUE;

            while (reader.Read())
            {
                if (reader.NodeType == XmlNodeType.Element && reader.Name == "id")
                {
                    identifier = reader.ReadElementContentAsString();
                }
                else if (reader.NodeType == XmlNodeType.Element && reader.Name == "description")
                {
                    description = reader.ReadElementContentAsString();
                }
                else if (reader.NodeType == XmlNodeType.Element && reader.Name == "label")
                {
                    label = reader.ReadElementContentAsString();
                }
                else if (reader.NodeType == XmlNodeType.Element && reader.Name == "requirements")
                {
                    requirements = XmlHelper.ReadLogic(reader, badTags);
                }
                else if (reader.NodeType == XmlNodeType.Element && reader.Name == "daily_change")
                {
                    dailyChange = reader.ReadElementContentAsInt();
                }
                else if (reader.NodeType == XmlNodeType.Element)
                {
                    badTags.Increment(reader.Name);
                }
                else if (reader.NodeType == XmlNodeType.EndElement && reader.Name == "prestige_mod")
                {
                    break;
                }
            }
            return new PrestigeModifier(identifier, label, description, requirements, dailyChange);
        }

        private OpinionModifier ReadOpinionModifier(XmlReader reader, Counter<string> badTags)
        {
            string identifier = null;
            string description = null;
            string label = null;
            int change = 0;
            int duration = int.MaxValue;

            while (reader.Read())
            {
                if (reader.NodeType == XmlNodeType.Element && reader.Name == "id")
                {
                    identifier = reader.ReadElementContentAsString();
                }
                else if (reader.NodeType == XmlNodeType.Element && reader.Name == "description")
                {
                    description = reader.ReadElementContentAsString();
                }
                else if (reader.NodeType == XmlNodeType.Element && reader.Name == "label")
                {
                    label = reader.ReadElementContentAsString();
                }
                else if (reader.NodeType == XmlNodeType.Element && reader.Name == "duration")
                {
                    duration = reader.ReadElementContentAsInt();
                }
                else if (reader.NodeType == XmlNodeType.Element && reader.Name == "opinion_change")
                {
                    change = reader.ReadElementContentAsInt();
                }
                else if (reader.NodeType == XmlNodeType.Element)
                {
                    badTags.Increment(reader.Name);
                }
                else if (reader.NodeType == XmlNodeType.EndElement && reader.Name == "opinion_mod")
                {
                    break;
                }
            }
            return new OpinionModifier(identifier, label, description, duration, change);
        }

        public void LoadTraitsFromFile(string filename, Counter<string> badTags)
        {
            using (XmlReader reader = XmlReader.Create(filename))
            {
                while (reader.Read())
                {
                    if (reader.NodeType == XmlNodeType.Element && reader.Name == "traits")
                    {
                        ReadTraits(reader, badTags);
                    }
                }
            }
        }

        private void ReadTraits(XmlReader reader, Counter<string> badTags)
        {
            while (reader.Read())
            {
                if (reader.NodeType == XmlNodeType.Element && reader.Name == "trait")
                {
                    Trait e = ReadTrait(reader, badTags);
                    traits.Add(e.Identifier, e);
                }
                else if (reader.NodeType == XmlNodeType.Element)
                {
                    badTags.Increment(reader.Name);
                }
                else if (reader.NodeType == XmlNodeType.EndElement && reader.Name == "traits")
                {
                    break;
                }
            }
        }


        public void LoadModifiersFromFile(string filename, Counter<string> badTags)
        {
            using (XmlReader reader = XmlReader.Create(filename))
            {
                while (reader.Read())
                {
                    if (reader.NodeType == XmlNodeType.Element && reader.Name == "prestige_mods")
                    {
                        ReadPrestigeModifiers(reader, badTags);
                    }
                    else if (reader.NodeType == XmlNodeType.Element && reader.Name == "opinion_mods")
                    {
                        ReadOpinionModifiers(reader, badTags);
                    }
                    else if (reader.NodeType == XmlNodeType.Element)
                    {
                        badTags.Increment(reader.Name);
                    }
                }
            }
        }

        private void ReadOpinionModifiers(XmlReader reader, Counter<string> badTags)
        {
            while (reader.Read())
            {
                if (reader.NodeType == XmlNodeType.Element && reader.Name == "opinion_mod")
                {
                    OpinionModifier mod = ReadOpinionModifier(reader, badTags);
                    opinionModifiers.Add(mod.Identifier, mod);
                }
                else if (reader.NodeType == XmlNodeType.Element)
                {
                    badTags.Increment(reader.Name);
                }
                else if (reader.NodeType == XmlNodeType.EndElement && reader.Name == "opinion_mods")
                {
                    break;
                }
            }
        }

        public OpinionModifier GetOpinionModifierById(string identifier)
        {
            return opinionModifiers[identifier];
        }

        private void ReadPrestigeModifiers(XmlReader reader, Counter<string> badTags)
        {
            while (reader.Read())
            {
                if (reader.NodeType == XmlNodeType.Element && reader.Name == "prestige_mod")
                {
                    PrestigeModifier mod = ReadPrestigeModifier(reader, badTags);
                    prestigeModifiers.Add(mod);
                }
                else if (reader.NodeType == XmlNodeType.Element)
                {
                    badTags.Increment(reader.Name);
                }
                else if (reader.NodeType == XmlNodeType.EndElement && reader.Name == "prestige_mods")
                {
                    break;
                }
            }
        }

        public void EvaluatePrestigeModifiers(Character character, Game game)
        {
            foreach (var modifier in prestigeModifiers)
            {
                if (modifier.EvaluateRequirements(character, game))
                {
                    character.AddPrestigeModifier(modifier);
                    character.PrestigeChange(modifier.DailyChange);
                }
                else
                {
                    character.RemovePrestigeModifier(modifier);
                }
            }
        }

        public void GetChangedModifiers(EventContext context, Game game, List<PrestigeModifier> added, List<PrestigeModifier> removed)
        {

            foreach (var modifier in prestigeModifiers)
            {
                if (modifier.EvaluateRequirements(context, game) && !modifier.EvaluateRequirements(context.CurrentCharacter, game))
                {
                    added.Add(modifier);
                }
                else if (!modifier.EvaluateRequirements(context, game) && modifier.EvaluateRequirements(context.CurrentCharacter, game))
                {
                    removed.Add(modifier);
                }
            }
        }

        public void AssignInitialTraits(Game game, Character character, int maxInitialTraits)
        {
            ISet<string> banned = new HashSet<string>();
            for (int i = 0; i < maxInitialTraits; ++i)
            {
                Trait nextTrait = traits.Values.ElementAt(game.GetRandom(traits.Values.Count));
                if (banned.Contains(nextTrait.Identifier))
                {
                    continue;
                }

                foreach (var opposite in nextTrait.Opposites)
                {
                    banned.Add(opposite);
                }

                banned.Add(nextTrait.Identifier);
                character.AddTrait(nextTrait);
            }
        }
    }
}
