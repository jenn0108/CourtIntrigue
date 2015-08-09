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
        public IExecute DirectExecute { get; private set; }
        public Parameter[] Parameters { get; private set; }

        public Event(string id, string desc, ILogic requirements, IExecute dirExec, EventOption[] options, Parameter[] parameters)
        {
            Identifier = id;
            Description = desc;
            ActionRequirements = requirements;
            DirectExecute = dirExec;
            Options = options;
            Parameters = parameters;
        }

        public void Execute(EventResults result, Game game, EventContext context)
        {
            game.Log(context.CurrentCharacter.Fullname + ": Event:" + this.CreateActionDescription(context));

            //DirectExecute always happens if it is present.
            if (DirectExecute != null)
                DirectExecute.Execute(result, game, context);

            if (Options.Length > 0)
            {
                //If there are options, the character must choose one.
                EventOption chosen = context.CurrentCharacter.ChooseOption(context, this);

                if(chosen != null && chosen.DirectExecute != null)
                {
                    //Execute the option activity.
                    chosen.DirectExecute.Execute(result, game, context);
                }
            }
        }

        public EventOption[] GetAvailableOptions(EventContext context, Game game)
        {
            List<EventOption> options = new List<EventOption>();
            foreach (var option in Options)
            {
                if (option.Requirements.Evaluate(context, game))
                {
                    options.Add(option);
                }
            }
            return options.ToArray();
        }

        public string CreateActionDescription(EventContext a)
        {
            return EventHelper.ReplaceStrings(Description, a);
        }

        public override string ToString()
        {
            return Identifier;
        }
    }

    class EventOption
    {
        public string Label { get; private set; }
        public IExecute DirectExecute { get; private set; }
        public ILogic Requirements { get; private set; }

        public EventOption(string label, IExecute dirExec, ILogic requirements)
        {
            Label = label;
            DirectExecute = dirExec;
            Requirements = requirements;
        }
    }

    class EventResults
    {
        public bool TargetGetsTurn { get; private set; }
        public EventResults()
        {
            TargetGetsTurn = false;
        }

        public void GiveTargetTurn()
        {
            TargetGetsTurn = true;
        }
    }
}
