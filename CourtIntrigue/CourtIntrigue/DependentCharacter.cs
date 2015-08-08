using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace CourtIntrigue
{
    class DependentCharacter : Character
    {
        public DependentCharacter(string name, int birthdate, Dynasty dynasty, Game game, GenderEnum gender, Room home) : base(name, birthdate, dynasty, 0, game, gender, null, new List<DependentCharacter>(), home)
        {
        }

    }
}