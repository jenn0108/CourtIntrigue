using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CourtIntrigue
{
    class InformationInstance
    {
        private List<Character> characters;
        private Information information;

        public InformationInstance(Information information, List<Character> characters)
        {
            this.information = information;
            this.characters = characters;
        }
    }
}
