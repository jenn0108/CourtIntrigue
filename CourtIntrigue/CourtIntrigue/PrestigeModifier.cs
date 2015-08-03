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
        private string description;
        private string identifier;
        private string label;
        private ILogic requirements;

        public PrestigeModifier(string identifier, string label, string description, ILogic requirements, int dailyChange)
        {
            this.identifier = identifier;
            this.label = label;
            this.description = description;
            this.requirements = requirements;
            this.DailyChange = dailyChange;
        }

        public bool EvaluateRequirements(Character character)
        {
            return requirements.Evaluate(new EventContext(null, character, null, null, null));
        }

        public override string ToString()
        {
            return identifier;
        }
    }
}
