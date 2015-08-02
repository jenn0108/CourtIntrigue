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
        public Dictionary<string, Trait> traits = new Dictionary<string, Trait>();

        private Trait ReadTrait(XmlReader reader)
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
                    opposites = XmlHelper.ReadList(reader, "opposite");
                }
                else if (reader.NodeType == XmlNodeType.Element && reader.Name == "opposite_opinion")
                {
                    oppositeOpinion = reader.ReadElementContentAsInt();
                }
                else if (reader.NodeType == XmlNodeType.Element && reader.Name == "same_opinion")
                {
                    sameOpinion = reader.ReadElementContentAsInt();
                }
                else if (reader.NodeType == XmlNodeType.EndElement && reader.Name == "trait")
                {
                    break;
                }
            }
            return new Trait(identifier, label, description, sameOpinion, oppositeOpinion, opposites);
        }



        public void LoadTraitsFromFile(string filename)
        {
            using (XmlReader reader = XmlReader.Create(filename))
            {
                while (reader.Read())
                {
                    if (reader.NodeType == XmlNodeType.Element && reader.Name == "traits")
                    {
                        ReadTraits(reader);
                    }
                }
            }
        }

        private void ReadTraits(XmlReader reader)
        {
            while (reader.Read())
            {
                if (reader.NodeType == XmlNodeType.Element && reader.Name == "trait")
                {
                    Trait e = ReadTrait(reader);
                    traits.Add(e.Identifier, e);
                }
                else if (reader.NodeType == XmlNodeType.EndElement && reader.Name == "traits")
                {
                    break;
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
