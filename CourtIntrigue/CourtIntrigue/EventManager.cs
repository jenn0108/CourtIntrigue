﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Xml;

namespace CourtIntrigue
{
    class EventManager
    {
        private static string BEGIN_DAY_ACTION_ID = "BEGIN_DAY_ACTION_ID";

        //I'm not sure if we actually want to store events by id.
        private Dictionary<string, Event> events = new Dictionary<string, Event>();
        private Dictionary<string, Action> actions = new Dictionary<string, Action>();

        public Event FindEventForAction(EventContext context, Game game)
        {
            //Which events can we perform?
            List<Event> okToRun = new List<Event>();
            //We really should be smarter about this.
            foreach(var pair in events)
            {
                if(pair.Value.ActionRequirements.Evaluate(context, game))
                {
                    okToRun.Add(pair.Value);
                }
            }
            if (okToRun.Count == 0)
                return null;
            else if (okToRun.Count == 1)
                return okToRun[0];

            return okToRun[game.GetRandom(okToRun.Count)];
        }

        public void ExecuteDayEvents(Character character, Game game)
        {
            EventContext context = new EventContext(BEGIN_DAY_ACTION_ID, character, null);

            foreach (var pair in events)
            {
                if (pair.Value.ActionRequirements.Evaluate(context, game))
                {
                    pair.Value.Execute(new EventResults(), game, context);
                }
            }
        }

        public Action[] FindAllowableActions(Room room, Character initiator, Character target, Game game)
        {
            List<Action> actions = new List<Action>();
            foreach(var actionId in room.PairActions)
            {
                EventContext context = new EventContext(actionId, initiator, target);
                foreach (var pair in events)
                {
                    if (pair.Value.ActionRequirements.Evaluate(context, game))
                    {
                        actions.Add(this.actions[actionId]);
                        break;
                    }
                }
            }

            return actions.ToArray();
        }

        public Action[] GetActionsById(string[] ids)
        {
            return ids.Select(id => actions[id]).ToArray();
        }

        public Event FindEventById(string id)
        {
            if (id == null)
                return null;

            //We can just let this throw an exeception if the event isn't present
            //for the time being.  In the future, we probably want to log the fact
            //and continue.
            return events[id];
        }

        public void LoadActionsFromFile(string filename, Dictionary<string, int> badTags)
        {
            using (XmlReader reader = XmlReader.Create(filename))
            {
                while (reader.Read())
                {
                    if (reader.NodeType == XmlNodeType.Element && reader.Name == "actions")
                    {
                        ReadActions(reader, badTags);
                    }
                }
            }
        }

        public void LoadEventsFromFile(string filename, Dictionary<string, int> badTags)
        {
            using (XmlReader reader = XmlReader.Create(filename))
            {
                while (reader.Read())
                {
                    if (reader.NodeType == XmlNodeType.Element && reader.Name == "events")
                    {
                        ReadEvents(reader, badTags);
                    }
                }
            }
        }

        private void ReadEvents(XmlReader reader, Dictionary<string, int> badTags)
        {
            while (reader.Read())
            {
                if(reader.NodeType == XmlNodeType.Element && reader.Name == "event")
                {
                    Event e = ReadEvent(reader, badTags);
                    events.Add(e.Identifier, e);
                }
                else if (reader.NodeType == XmlNodeType.EndElement && reader.Name == "events")
                {
                    break;
                }
            }
        }

        private void ReadActions(XmlReader reader, Dictionary<string, int> badTags)
        {
            while (reader.Read())
            {
                if (reader.NodeType == XmlNodeType.Element && reader.Name == "action")
                {
                    Action a = ReadAction(reader, badTags);
                    actions.Add(a.Identifier, a);
                }
                else if (reader.NodeType == XmlNodeType.EndElement && reader.Name == "actions")
                {
                    break;
                }
            }
        }

        private Event ReadEvent(XmlReader reader, Dictionary<string, int> badTags)
        {
            string identifier = null;
            string description = null;

            //Actions without any action logic can't be triggered in FindEventForAction
            //so we can just use FALSE so they'll always be unavailable.
            ILogic actionLogic = Logic.FALSE;

            //Actions without a top level exec shouldn't do anything in their exec.
            IExecute dirExec = Execute.NOOP;
            List<EventOption> options = new List<EventOption>();
            List<Parameter> parameters = new List<Parameter>();
            while (reader.Read())
            {
                if (reader.NodeType == XmlNodeType.Element && reader.Name == "id")
                {
                    identifier = reader.ReadElementContentAsString();
                }
                else if (reader.NodeType == XmlNodeType.Element && reader.Name == "description")
                {
                    description = reader.ReadElementContentAsString();
                }
                else if (reader.NodeType == XmlNodeType.Element && reader.Name == "requirements")
                {
                    actionLogic = XmlHelper.ReadLogic(reader, badTags);
                }
                else if (reader.NodeType == XmlNodeType.Element && reader.Name == "exec")
                {
                    dirExec = XmlHelper.ReadExecute(reader, badTags);
                }
                else if (reader.NodeType == XmlNodeType.Element && reader.Name == "options")
                {
                    options = ReadOptions(reader, badTags);
                }
                else if (reader.NodeType == XmlNodeType.Element && reader.Name == "parameters")
                {
                    parameters = XmlHelper.ReadParameters(reader, badTags);

                }
                else if (reader.NodeType == XmlNodeType.EndElement && reader.Name == "event")
                {
                    break;
                }
            }

            if (options.Count == 0)
                throw new EventIncorrectException("Events must have at least one option or else they look weird");

            return new Event(identifier, description, actionLogic, dirExec, options.ToArray(), parameters.ToArray());
        }

        private Action ReadAction(XmlReader reader, Dictionary<string, int> badTags)
        {
            string identifier = null;
            string label = null;
            while (reader.Read())
            {
                if (reader.NodeType == XmlNodeType.Element && reader.Name == "id")
                {
                    identifier = reader.ReadElementContentAsString();
                }
                else if (reader.NodeType == XmlNodeType.Element && reader.Name == "label")
                {
                    label = reader.ReadElementContentAsString();
                }
                else if (reader.NodeType == XmlNodeType.EndElement && reader.Name == "action")
                {
                    break;
                }
            }
            return new Action(identifier, label);
        }

        private List<EventOption> ReadOptions(XmlReader reader, Dictionary<string, int> badTags)
        {
            List<EventOption> options = new List<EventOption>();
            while (reader.Read())
            {
                if (reader.NodeType == XmlNodeType.Element && reader.Name == "option")
                {
                    options.Add(ReadOption(reader, badTags));
                }
                else if (reader.NodeType == XmlNodeType.EndElement && reader.Name == "options")
                {
                    break;
                }
            }
            return options;
        }


        private EventOption ReadOption(XmlReader reader, Dictionary<string, int> badTags)
        {
            string label = null;

            //Options without an exec shouldn't do anything.
            IExecute dirExec = Execute.NOOP;
            ILogic requirements = Logic.TRUE;

            List<Adversion> adversions = new List<Adversion>();
            List<Desire> desires = new List<Desire>();

            while (reader.Read())
            {
                if (reader.NodeType == XmlNodeType.Element && reader.Name == "label")
                {
                    label = reader.ReadElementContentAsString();
                }
                else if (reader.NodeType == XmlNodeType.Element && reader.Name == "exec")
                {
                    dirExec = XmlHelper.ReadExecute(reader, badTags);
                }
                else if (reader.NodeType == XmlNodeType.Element && reader.Name == "requirements")
                {
                    requirements = XmlHelper.ReadLogic(reader, badTags);
                }
                else if (reader.Name == "willpower")
                {
                    //Just skip it.
                }
                else if (reader.NodeType == XmlNodeType.Element && reader.Name == "trait_adversion")
                {
                    adversions.Add(new Adversion(reader.GetAttribute("type"), int.Parse(reader.GetAttribute("cost"))));
                }
                else if (reader.NodeType == XmlNodeType.Element && reader.Name == "trait_desire")
                {
                    desires.Add(new Desire(reader.GetAttribute("type"), int.Parse(reader.GetAttribute("cost"))));
                }
                else if (reader.NodeType == XmlNodeType.EndElement && reader.Name == "option")
                {
                    break;
                }
            }
            return new EventOption(label, dirExec, requirements, adversions.ToArray(), desires.ToArray());
        }


        
    }

    class Action
    {
        public string Identifier { get; private set; }
        public string Label { get; private set; }

        public Action(string id, string label)
        {
            Identifier = id;
            Label = label;
        }
    }

    class EventIncorrectException : Exception
    {
        public EventIncorrectException(string reason) : base(reason)
        {

        }
    }
}
