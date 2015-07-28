using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CourtIntrigue
{
    class Event
    {
        public string Identifier { get; private set; }
        public string Description { get; private set; }
        public ILogic ActionRequirements { get; private set; }
        public EventOption[] Options { get; private set; }

        public Event(string id, string desc, ILogic requirements, EventOption[] options)
        {
            Identifier = id;
            Description = desc;
            ActionRequirements = requirements;
            Options = options;
        }
    }

    class EventOption
    {
        public string Label { get; private set; }

        public EventOption(string label)
        {
            Label = label;
        }
    }
}
