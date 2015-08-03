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
        void Execute(EventResults result, Game game, EventContext context);
    }

    class NoOpExecute : IExecute
    {
        public void Execute(EventResults result, Game game, EventContext context)
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

        public void Execute(EventResults result, Game game, EventContext context)
        {
            for(int i = 0; i < instructions.Length; ++i)
            {
                instructions[i].Execute(result, game, context);
            }
        }
    }

    class AllowEventSelectionExecute : IExecute
    {
        public void Execute(EventResults result, Game game, EventContext context)
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

        public void Execute(EventResults result, Game game, EventContext context)
        {
            Dictionary<string, object> computedParameters = new Dictionary<string, object>();
            foreach (var pair in parameters)
            {
                computedParameters.Add(pair.Key, context.GetScopedObjectByName(pair.Value));
            }
            EventContext newContext = new EventContext(null, context.CurrentCharacter, context.GetScopedObjectByName("ROOT") as Character, computedParameters);
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

        public void Execute(EventResults result, Game game, EventContext context)
        {
            if (game.GetRandom(100) < chance)
            {
                Information information = game.GetInformationById(informationId);
                Dictionary<string, object> computedParameters = new Dictionary<string, object>();
                foreach (var pair in parameters)
                {
                    computedParameters.Add(pair.Key, context.GetScopedObjectByName(pair.Value));
                }
                InformationInstance newInfo = new InformationInstance(information, computedParameters, game.CurrentTime);
                if(context.CurrentCharacter.AddInformation(newInfo))
                {
                    game.Log(context.CurrentCharacter.Name + " learned an information.");
                    newInfo.ExecuteOnObserve(context.CurrentCharacter, game, context.Room);
                }
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
        public void Execute(EventResults result, Game game, EventContext context)
        {
            Character tellingCharacter = context.GetScopedObjectByName("ROOT") as Character;
            InformationInstance informationInstance = tellingCharacter.ChooseInformation();
            bool isNewInformation = context.CurrentCharacter.AddInformation(informationInstance);
            context.PushScope(informationInstance);
            operation.Execute(result, game, context);
            context.PopScope();

            if(isNewInformation)
            {
                game.Log(context.CurrentCharacter.Name + " learned an information.");
                informationInstance.ExecuteOnTold(context.CurrentCharacter, tellingCharacter, game, context.Room);
            }
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

        public void Execute(EventResults result, Game game, EventContext context)
        {
            foreach (var character in context.Room.GetCharacters(context.CurrentCharacter))
            {
                context.PushScope(character);
                if (requirements.Evaluate(context))
                {
                    operation.Execute(result, game, context);
                }
                context.PopScope();
            }
        }
    }

    class SetScopeExecute : IExecute
    {
        private string scopeName;
        private IExecute operation;
        public SetScopeExecute(string scopeName, IExecute operation)
        {
            this.scopeName = scopeName;
            this.operation = operation;
        }

        public void Execute(EventResults result, Game game, EventContext context)
        {
            context.PushScope(context.GetScopedObjectByName(scopeName));
            operation.Execute(result, game, context);
            context.PopScope();
        }
    }

    class ApplyOpinionModifierExecute : IExecute
    {
        private string identifier;
        private string character;
        public ApplyOpinionModifierExecute(string identifier, string character)
        {
            this.identifier = identifier;
            this.character = character;
        }

        public void Execute(EventResults result, Game game, EventContext context)
        {
            OpinionModifierInstance mod = game.CreateOpinionModifier(identifier, context.GetScopedObjectByName(character) as Character);
            context.CurrentCharacter.AddOpinionModifier(mod);
        }

    }

    class PrestigeChangeExecute : IExecute
    {
        private int change;
        public PrestigeChangeExecute(int change)
        {
            this.change = change;
        }

        public void Execute(EventResults result, Game game, EventContext context)
        {
            context.CurrentCharacter.Dynasty.PrestigeChange(change);
        }
    }
}
