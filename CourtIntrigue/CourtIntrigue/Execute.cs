﻿using System;
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

    class TriggerEventExecute : IExecute
    {
        private string eventid;
        private Dictionary<string, string> parameters;
        public TriggerEventExecute(string id, Dictionary<string, string> parameters)
        {
            eventid = id;
            this.parameters = parameters;
        }

        public void Execute(EventResults result, Game game, EventContext context, Event e)
        {
            Dictionary<string, object> computedParameters = new Dictionary<string, object>();
            foreach (var pair in parameters)
            {
                computedParameters.Add(pair.Key, context.GetScopedObjectByName(pair.Value));
            }
            EventContext newContext = new EventContext(null, context.CurrentCharacter, context.GetScopedObjectByName("ROOT") as Character, context.Room, computedParameters);
            game.GetEventById(eventid).Execute(result, game, newContext);
        }
    }

    class ObserveInformationExecute : IExecute
    {
        private string informationId;
        private Dictionary<string, string> parameters;
        private int chance;
        public ObserveInformationExecute(string informationId, Dictionary<string,string> parameters, int chance)
        {
            this.informationId = informationId;
            this.parameters = parameters;
            this.chance = chance;
        }

        public void Execute(EventResults result, Game game, EventContext context, Event e)
        {
            if (game.GetRandom(100) < chance)
            {
                Information information = game.GetInformationById(informationId);
                Dictionary<string, object> computedParameters = new Dictionary<string, object>();
                foreach (var pair in parameters)
                {
                    computedParameters.Add(pair.Key, context.GetScopedObjectByName(pair.Value));
                }
                context.CurrentCharacter.KnownInformation.Add(new InformationInstance(information, computedParameters));
                game.Log(context.CurrentCharacter.Name + " learned an information.");
            }
        }
    }

    class TellInformationExecute : IExecute
    {
        public IExecute operation;
        public TellInformationExecute(IExecute operation)
        {
            this.operation = operation;
        }
        public void Execute(EventResults result, Game game, EventContext context, Event e)
        {
            InformationInstance informationInstance = (context.GetScopedObjectByName("ROOT") as Character).ChooseInformation();
            context.CurrentCharacter.KnownInformation.Add(informationInstance);
            game.Log(context.CurrentCharacter.Name + " learned an information.");
            context.PushScope(informationInstance);
            operation.Execute(result, game, context, e);
            context.PopScope();
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
            foreach (var character in context.Room.GetCharacters(context.CurrentCharacter))
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

    class TargetExecute : IExecute
    {
        private IExecute operation;
        public TargetExecute(IExecute operation)
        {
            this.operation = operation;
        }

        public void Execute(EventResults result, Game game, EventContext context, Event e)
        {
            context.PushScope(context.Target);
            operation.Execute(result, game, context, e);
            context.PopScope();
        }
    }
}
