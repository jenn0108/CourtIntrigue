using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace CourtIntrigue
{
    class DependentCharacter : Character
    {
        public DependentCharacter(string name, Dynasty dynasty, Game game, GenderEnum gender) : base(name, dynasty, 0, game, gender, null, new List<DependentCharacter>())
        {
        }

    }
}