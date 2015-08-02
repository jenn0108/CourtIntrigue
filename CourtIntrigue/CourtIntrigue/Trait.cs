using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CourtIntrigue
{
    class Trait
    {
        public string Identifier { get; private set; }
        public string Label { get; private set; }
        public string Description { get; private set; }
        public string[] Opposites { get; private set; }
        public int OppositeOpinion { get; private set; }
        public int SameOpinion { get; private set; }

        public Trait(string identifier, string label, string description, int sameOpinion, int oppositeOpinion, string[] opposites)
        {
            Identifier = identifier;
            Label = label;
            Description = description;
            SameOpinion = sameOpinion;
            OppositeOpinion = oppositeOpinion;
            Opposites = opposites;
        }

    }
}
