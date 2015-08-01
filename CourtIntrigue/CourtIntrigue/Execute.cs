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
        void Execute(EventResults result, Game game, EventContext context, Event e);
    }

    class NoOpExecute : IExecute
    {
        public void Execute(EventResults result, Game game, EventContext context, Event e)
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

        public void Execute(EventResults result, Game game, EventContext context, Event e)
        {
            for(int i = 0; i < instructions.Length; ++i)
            {
                instructions[i].Execute(result, game, context, e);
            }
        }
    }

    class AllowEventSelectionExecute : IExecute
    {
        public void Execute(EventResults result, Game game, EventContext context, Event e)
        {
            result.GiveTargetTurn();
        }
    }

    class TargetEventExecute : IExecute
    {
        private string eventid;
        public TargetEventExecute(string id)
        {
            eventid = id;
        }

        public void Execute(EventResults result, Game game, EventContext context, Event e)
        {
            game.GetEventById(eventid).Execute(result, game, new EventContext(EventContext.CUSTOM_ACTION, context.Target, context.CurrentScope, context.Room));
        }
    }

    class AddInformationExecute : IExecute
    {
        private string informationId;
        private Dictionary<string, string> parameters;
        public AddInformationExecute(string id, Dictionary<string,string> parameters)
        {
            this.informationId = id;
            this.parameters = parameters;
        }

        public void Execute(EventResults result, Game game, EventContext context, Event e)
        {
            Information information = game.GetInformationById(informationId);
            Dictionary<string, object> computedParameters = new Dictionary<string, object>();
            foreach(var pair in parameters)
            {
                computedParameters.Add(pair.Key, context.GetScopedCharacterByName(pair.Value));
            }
            context.CurrentScope.KnownInformation.Add(new InformationInstance(information, computedParameters));
        }
    }

    class EveryoneInRoomExecute : IExecute
    {
        private IExecute operation;
        private ILogic requirements;
        public EveryoneInRoomExecute(IExecute operation, ILogic requirements)
        {
            this.operation = operation;
            this.requirements = requirements;
        }

        public void Execute(EventResults result, Game game, EventContext context, Event e)
        {
            foreach (var character in context.Room.GetCharacters(context.CurrentScope))
            {
                context.PushScope(character);
                if (requirements.Evaluate(context))
                {
                    operation.Execute(result, game, context, e);
                }
                context.PopScope();
            }
        }
    }
}
