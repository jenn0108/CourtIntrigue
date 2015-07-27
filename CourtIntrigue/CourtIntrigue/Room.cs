using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CourtIntrigue
{
    class Room
    {
        private List<Character> unoccuppiedCharacters = new List<Character>();
        public string Name { get; private set; }
        public bool CharactersApproachable { get; private set; }
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

        public Room(string name, bool approachable, string[] actions)
        {
            Name = name;
            CharactersApproachable = approachable;
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
}