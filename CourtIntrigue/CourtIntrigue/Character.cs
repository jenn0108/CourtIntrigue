using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CourtIntrigue
{
    class Character
    {
        public string Name { get; private set; }
        public int Money { get; private set; }
        protected Game Game { get; private set; }

        public Character(string name, int money, Game game)
        {
            Name = name;
            Money = money;
            Game = game;
        }

        public virtual Action Tick(Room room)
        {
            throw new NotImplementedException();
        }

        public virtual Room BeginDay()
        {
            throw new NotImplementedException();
        }

        public virtual EventOption ChooseOption(Action action, Event e)
        {
            throw new NotImplementedException();
        }

        protected void CharacterLog(string text)
        {
            Game.Log(Name + ": " + text);
        }
    }
}
