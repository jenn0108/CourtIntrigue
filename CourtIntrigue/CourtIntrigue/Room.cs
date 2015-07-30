using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace CourtIntrigue
{
    class Room
    {
        private List<Character> unoccuppiedCharacters = new List<Character>();
        public string Identifier { get; private set; }
        public string Name { get; private set; }
        public bool CharactersApproachable { get; private set; }
        public bool Common { get; private set; }
        public string[] Actions { get; private set; }

        public IEnumerable<Character> GetCharacters(Character skip = null)
        {
            foreach (var character in unoccuppiedCharacters)
            {
                if (character != skip)
                {
                    yield return character;
                }
            }
        }

        public Room(string id, string name, bool approachable, bool common, string[] actions)
        {
            Identifier = id;
            Name = name;
            CharactersApproachable = approachable;
            Common = common;
            Actions = actions;
            if (actions == null || actions.Length == 0)
            {
                throw new ArgumentNullException("Rooms must have at least one action");
            }
        }

        public void AddCharacter(Character character)
        {
            unoccuppiedCharacters.Add(character);
        }

        public void RemoveCharacter(Character character)
        {
            unoccuppiedCharacters.Remove(character);
        }
    }


    class RoomManager
    {
        private List<Room> rooms = new List<Room>();

        public Room[] GetCommonRooms()
        {
            return rooms.Where(r => r.Common).ToArray();
        }

        public void LoadRoomsFromFile(string filename)
        {
            using (XmlReader reader = XmlReader.Create(filename))
            {
                while (reader.Read())
                {
                    if (reader.NodeType == XmlNodeType.Element && reader.Name == "rooms")
                    {
                        ReadRooms(reader);
                    }
                }
            }
        }

        private void ReadRooms(XmlReader reader)
        {
            while (reader.Read())
            {
                if (reader.NodeType == XmlNodeType.Element && reader.Name == "room")
                {
                    Room r = ReadRoom(reader);
                    rooms.Add(r);
                }
                else if (reader.NodeType == XmlNodeType.EndElement && reader.Name == "rooms")
                {
                    break;
                }
            }
        }

        private Room ReadRoom(XmlReader reader)
        {
            string identifier = null;
            string name = null;
            bool common = false;
            bool charactersApproachable = false;
            List<string> actions = new List<string>();
            while (reader.Read())
            {
                if (reader.NodeType == XmlNodeType.Element && reader.Name == "id")
                {
                    identifier = reader.ReadElementContentAsString();
                }
                else if (reader.NodeType == XmlNodeType.Element && reader.Name == "name")
                {
                    name = reader.ReadElementContentAsString();
                }
                else if (reader.NodeType == XmlNodeType.Element && reader.Name == "common")
                {
                    common = reader.ReadElementContentAsString() == "Yes";
                }
                else if (reader.NodeType == XmlNodeType.Element && reader.Name == "characters_approachable")
                {
                    charactersApproachable = reader.ReadElementContentAsString() == "Yes";
                }
                else if (reader.NodeType == XmlNodeType.Element && reader.Name == "actions")
                {
                    actions = ReadActions(reader);
                }
                else if (reader.NodeType == XmlNodeType.EndElement && reader.Name == "room")
                {
                    break;
                }
            }
            return new Room(identifier, name, charactersApproachable, common, actions.ToArray());
        }


        private List<string> ReadActions(XmlReader reader)
        {
            List<string> actions = new List<string>();
            while (reader.Read())
            {
                if (reader.NodeType == XmlNodeType.Element && reader.Name == "action")
                {
                    actions.Add(reader.ReadElementContentAsString());
                }
                else if (reader.NodeType == XmlNodeType.EndElement && reader.Name == "actions")
                {
                    break;
                }
            }
            return actions;
        }
    }
}