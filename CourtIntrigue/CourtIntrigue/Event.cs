﻿using System;
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

        public Event(string id, string desc, ILogic requirements, IExecute dirExec, EventOption[] options)
        {
            Identifier = id;
            Description = desc;
            ActionRequirements = requirements;
            DirectExecute = dirExec;
            Options = options;
        }

        public void Execute(EventResults r, EventManager m, Action a)
        {
            //DirectExecute always happens if it is present.
            if (DirectExecute != null)
                DirectExecute.Execute(r, m, a, this);

            if (Options.Length > 0)
            {
                //If there are options, the character must choose one.
                EventOption chosen = a.Initiator.ChooseOption(a, this);

                if(chosen != null && chosen.DirectExecute != null)
                {
                    //Execute the option activity.
                    chosen.DirectExecute.Execute(r, m, a, this);
                }
            }
        }

        public EventOption[] GetAvailableOptions(Action action)
        {
            //In the future we will need to evaluate whether or not each
            //option should be visible to the character.
            return Options;
        }

        public string CreateActionDescription(Action a)
        {
            return ActionHelper.ReplaceStrings(Description, a);
        }
    }

    class EventOption
    {
        public string Label { get; private set; }
        public IExecute DirectExecute { get; private set; }

        public EventOption(string label, IExecute dirExec)
        {
            Label = label;
            DirectExecute = dirExec;
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
