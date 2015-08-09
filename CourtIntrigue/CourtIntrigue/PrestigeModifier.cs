using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CourtIntrigue
{
    class PrestigeModifier
    {
        public int DailyChange { get; private set; }
        public string Description { get; private set; }
        public string Identifier { get; private set; }
        public string Label { get; private set; }
        private ILogic requirements;

        public PrestigeModifier(string identifier, string label, string description, ILogic requirements, int dailyChange)
        {
            Identifier = identifier;
            Label = label;
            Description = description;
            this.requirements = requirements;
            DailyChange = dailyChange;
        }

        public bool EvaluateRequirements(Character character, Game game)
        {
            return requirements.Evaluate(new EventContext(null, character, null), game);
        }

        public override string ToString()
        {
            return Identifier;
        }
    }
}
