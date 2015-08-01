using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CourtIntrigue
{
    class Execute
    {
        public static IExecute NOOP = new NoOpExecute();
    }

    interface IExecute
    {
        void Execute(EventResults r, EventManager m, EventContext a, Event e);
    }

    class NoOpExecute : IExecute
    {
        public void Execute(EventResults r, EventManager m, EventContext a, Event e)
        {

        }
    }

    class SequenceExecute : IExecute
    {
        private IExecute[] instructions;
        public SequenceExecute(IExecute[] instructions)
        {
            this.instructions = instructions;
        }

        public void Execute(EventResults r, EventManager m, EventContext a, Event e)
        {
            for(int i = 0; i < instructions.Length; ++i)
            {
                instructions[i].Execute(r, m, a, e);
            }
        }
    }

    class AllowEventSelectionExecute : IExecute
    {
        public void Execute(EventResults r, EventManager m, EventContext a, Event e)
        {
            r.GiveTargetTurn();
        }
    }

    class TargetEventExecute : IExecute
    {
        private string eventid;
        public TargetEventExecute(string id)
        {
            eventid = id;
        }

        public void Execute(EventResults r, EventManager m, EventContext a, Event e)
        {
            m.FindEventById(eventid).Execute(r, m, new EventContext(EventContext.CUSTOM_ACTION, a.Target, a.Initiator, a.Room));
        }
    }
}
