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
        private ISet<Character> unoccuppiedCharacters = new HashSet<Character>();
        private ISet<Character> characters = new HashSet<Character>();
        public string Identifier { get; private set; }
        public string Name { get; private set; }
        public bool Common { get; private set; }
        public string[] SoloActions { get; private set; }
        public string[] PairActions { get; private set; }
        public ILogic Requirements { get; private set; }

        public IEnumerable<Character> GetCharacters(Character skip = null)
        {
            foreach (var character in characters)
            {
                if (character != skip)
                {
                    yield return character;
                }
            }
        }

        public IEnumerable<Character> GetUnoccuppiedCharacters(Character skip = null)
        {
            foreach (var character in unoccuppiedCharacters)
            {
                if (character != skip)
                {
                    yield return character;
                }
            }
        }

        public Room(string id, string name, bool common, string[] soloActions, string[] pairActions, ILogic requirements)
        {
            Identifier = id;
            Name = name;
            PairActions = pairActions;
            Common = common;
            SoloActions = soloActions;
            Requirements = requirements;
            if (soloActions == null || soloActions.Length == 0)
            {
                throw new ArgumentNullException("Rooms must have at least one action");
            }
        }

        public void RemoveCharacter(Character character)
        {
            characters.Remove(character);
            unoccuppiedCharacters.Remove(character);
        }

        public void AddCharacter(Character character)
        {
            characters.Add(character);
            unoccuppiedCharacters.Add(character);
        }

        public void ClearRoom()
        {
            characters.Clear();
            unoccuppiedCharacters.Clear();
        }

        public void MarkBusy(Character character)
        {
            unoccuppiedCharacters.Remove(character);
        }

        public void ResetUnoccupied()
        {
            unoccuppiedCharacters.Clear();
            unoccuppiedCharacters.UnionWith(characters);
        }

        public Room Clone()
        {
            return new Room(Identifier, Name, Common, SoloActions, PairActions, Requirements);
        }

        public override string ToString()
        {
            return Name;
        }
    }


    class RoomManager
    {
        private List<Room> rooms = new List<Room>();

        public Room[] GetCommonRooms()
        {
            return rooms.Where(r => r.Common).ToArray();
        }

        public Room GetRoomById(string id)
        {
            return rooms.Where(r => r.Identifier == id).First();
        }

        public void LoadRoomsFromFile(string filename, Counter<string> badTags)
        {
            using (XmlReader reader = XmlReader.Create(filename))
            {
                while (reader.Read())
                {
                    if (reader.NodeType == XmlNodeType.Element && reader.Name == "rooms")
                    {
                        ReadRooms(reader, badTags);
                    }
                    else if (reader.NodeType == XmlNodeType.Element)
                    {
                        badTags.Increment(reader.Name);
                    }
                }
            }
        }

        private void ReadRooms(XmlReader reader, Counter<string> badTags)
        {
            while (reader.Read())
            {
                if (reader.NodeType == XmlNodeType.Element && reader.Name == "room")
                {
                    Room r = ReadRoom(reader, badTags);
                    rooms.Add(r);
                }
                else if (reader.NodeType == XmlNodeType.Element)
                {
                    badTags.Increment(reader.Name);
                }
                else if (reader.NodeType == XmlNodeType.EndElement && reader.Name == "rooms")
                {
                    break;
                }
            }
        }

        private Room ReadRoom(XmlReader reader, Counter<string> badTags)
        {
            string identifier = null;
            string name = null;
            bool common = false;
            List<string> soloActions = new List<string>();
            List<string> pairActions = new List<string>();
            ILogic requirements = Logic.TRUE;
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
                else if (reader.NodeType == XmlNodeType.Element && reader.Name == "solo_actions")
                {
                    soloActions = ReadActions(reader);
                }
                else if (reader.NodeType == XmlNodeType.Element && reader.Name == "pair_actions")
                {
                    pairActions = ReadActions(reader);
                }
                else if (reader.NodeType == XmlNodeType.Element && reader.Name == "requirements")
                {
                    requirements = XmlHelper.ReadLogic(reader, badTags);
                }
                else if (reader.NodeType == XmlNodeType.Element)
                {
                    badTags.Increment(reader.Name);
                }
                else if (reader.NodeType == XmlNodeType.EndElement && reader.Name == "room")
                {
                    break;
                }
            }
            return new Room(identifier, name, common, soloActions.ToArray(), pairActions.ToArray(), requirements);
        }


        private List<string> ReadActions(XmlReader reader)
        {
            string tag = reader.Name;
            List<string> actions = new List<string>();
            while (reader.Read())
            {
                if (reader.NodeType == XmlNodeType.Element && reader.Name == "action")
                {
                    actions.Add(reader.ReadElementContentAsString());
                }
                else if (reader.NodeType == XmlNodeType.EndElement && reader.Name == tag)
                {
                    break;
                }
            }
            return actions;
        }

        public Room MakeUniqueRoom(string id)
        {
            return GetRoomById(id).Clone();
        }

        public IEnumerable<Room> GetRooms(Character character, Game game)
        {
            EventContext context = new EventContext(character);
            foreach(var room in rooms)
            {
                if (room.Common && room.Requirements.Evaluate(context, game))
                    yield return room;
            }
        }
    }
}