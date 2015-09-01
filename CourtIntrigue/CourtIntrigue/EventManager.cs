using System;
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

        public Event FindEventForAction(ActionDescriptor actionDescriptor, Game game)
        {
            //Which events can we perform?
            List<Event> okToRun = new List<Event>();
            // Smarter now, we look through only the events that are listed under the requested action.
            foreach(string eventId in actionDescriptor.Action.Events)
            {
                Event e = events[eventId];
                if(e.EvaluateActionRequirements(actionDescriptor, game))
                {
                    okToRun.Add(e);
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
            ActionDescriptor actionDescriptor = new ActionDescriptor(actions[BEGIN_DAY_ACTION_ID], character, null);

            foreach (string eventId in actions[BEGIN_DAY_ACTION_ID].Events)
            {
                Event e = events[eventId];

                if (e.EvaluateActionRequirements(actionDescriptor, game))
                {
                    EventContext context = new EventContext(actionDescriptor);
                    e.Execute(new EventResults(), game, context);
                    //Commit the day changes.
                    context.Commit();
                }
            }
        }

        public Action[] FindAllowableActions(Room room, Character initiator, Character target, Game game)
        {
            List<Action> allowableActions = new List<Action>();
            foreach(var actionId in room.Actions)
            {
                Action pairAction = actions[actionId];

                if (pairAction.Type != ActionType.Pair)
                    continue;

                ActionDescriptor actionDescriptor = new ActionDescriptor(pairAction, initiator, target);
                foreach (string eventId in actions[actionId].Events)
                {
                    Event e = events[eventId];
                    if (e.EvaluateActionRequirements(actionDescriptor, game))
                    {
                        allowableActions.Add(actions[actionId]);
                        break;
                    }
                }
            }

            return allowableActions.ToArray();
        }

        public Action[] FindAllowableActions(Room room, Character initiator, Game game)
        {
            List<Action> allowableActions = new List<Action>();
            foreach (var actionId in room.Actions)
            {
                Action soloAction = actions[actionId];

                if (soloAction.Type != ActionType.Delayed && soloAction.Type != ActionType.Immediate)
                    continue;

                ActionDescriptor actionDescriptor = new ActionDescriptor(soloAction, initiator, null);
                foreach (string eventId in actions[actionId].Events)
                {
                    Event e = events[eventId];
                    if (e.EvaluateActionRequirements(actionDescriptor, game))
                    {
                        allowableActions.Add(actions[actionId]);
                        break;
                    }
                }
            }

            return allowableActions.ToArray();
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

        public void LoadActionsFromFile(string filename, Counter<string> badTags)
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

        public void LoadEventsFromFile(string filename, Counter<string> badTags)
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

        public void Verify()
        {
            foreach(var action in actions.Values)
            {
                if (action.Type == ActionType.Pair && action.ParameterName == null)
                    throw new ActionIncorrectException("Pair actions must name a parameter");

                if (action.Type != ActionType.Pair && action.ParameterName != null)
                    throw new ActionIncorrectException("Only pair actions can name a parameter");

                foreach (var id in action.Events)
                {
                    //Make sure the event exists
                    Event e = events[id];
                    
                    if(action.Type == ActionType.Pair)
                    {
                        //Make sure parameters match the action parameter.  We're expecting a single
                        //parameter with the name in the action.
                        if (e.Parameters.Length != 1)
                            throw new EventIncorrectException("Events for pair actions must have exactly one parameter");

                        if (e.Parameters[0].Name != action.ParameterName)
                            throw new EventIncorrectException("Event must have parameter:" + action.ParameterName);

                        if (e.Parameters[0].Type != typeof(Character))
                            throw new EventIncorrectException("Event must have parameter type character");
                    }
                    else
                    {
                        if (e.Parameters.Length != 0)
                            throw new EventIncorrectException("Events for non-pair actions must have no parameters");
                    }
                }
            }
        }

        private void ReadEvents(XmlReader reader, Counter<string> badTags)
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

        private void ReadActions(XmlReader reader, Counter<string> badTags)
        {
            while (reader.Read())
            {
                if (reader.NodeType == XmlNodeType.Element && 
                    (reader.Name == "delayed_action" || reader.Name == "pair_action" || reader.Name == "internal_action" || 
                    reader.Name == "immediate_action"))
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

        private Event ReadEvent(XmlReader reader, Counter<string> badTags)
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

        private Action ReadAction(XmlReader reader, Counter<string> badTags)
        {
            ActionType type = ActionType.Internal;
            string tag = reader.Name;

            if (tag == "delayed_action")
                type = ActionType.Delayed;
            else if (tag == "internal_action")
                type = ActionType.Internal;
            else if (tag == "pair_action")
                type = ActionType.Pair;
            else if (tag == "immediate_action")
                type = ActionType.Immediate;
            else
                throw new KeyNotFoundException();

            string identifier = null;
            string label = null;
            List<string> eventIds = new List<string>();
            string parameter = null;
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
                else if (reader.NodeType == XmlNodeType.Element && reader.Name == "event_ids")
                {
                    eventIds = ReadEventIds(reader, badTags);
                }
                else if (reader.NodeType == XmlNodeType.Element && reader.Name == "parameter")
                {
                    parameter = reader.ReadElementContentAsString();
                }
                else if (reader.NodeType == XmlNodeType.EndElement && reader.Name == tag)
                {
                    break;
                }
            }
            return new Action(identifier, type, label, eventIds, parameter);
        }

        private List<string> ReadEventIds(XmlReader reader, Counter<string> badTags)
        {
            List<string> eventIds = new List<string>();
            while (reader.Read())
            {
                if (reader.NodeType == XmlNodeType.Element && reader.Name == "id")
                {
                    eventIds.Add(reader.ReadElementContentAsString());
                }
                else if (reader.NodeType == XmlNodeType.Element)
                {
                    badTags.Increment(reader.Name);
                }
                else if (reader.NodeType == XmlNodeType.EndElement && reader.Name == "event_ids")
                {
                    break;
                }
            }
            return eventIds;
        }


        private List<EventOption> ReadOptions(XmlReader reader, Counter<string> badTags)
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


        private EventOption ReadOption(XmlReader reader, Counter<string> badTags)
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
                else if (reader.NodeType == XmlNodeType.Element)
                {
                    badTags.Increment(reader.Name);
                }
                else if (reader.NodeType == XmlNodeType.EndElement && reader.Name == "option")
                {
                    break;
                }
            }
            return new EventOption(label, dirExec, requirements, adversions.ToArray(), desires.ToArray());
        }


        
    }

    enum ActionType
    {
        Internal,
        Delayed,
        Pair,
        Immediate
    }

    class Action
    {
        public string Identifier { get; private set; }
        public string Label { get; private set; }
        public ActionType Type { get; private set; }
        public string ParameterName { get; private set; }
        public List<string> Events { get; private set; }

        public Action(string id, ActionType type, string label, List<string> events, string parameterName )
        {
            Identifier = id;
            Type = type;
            Label = label;
            Events = events;
            ParameterName = parameterName;
        }
    }

    class EventIncorrectException : Exception
    {
        public EventIncorrectException(string reason) : base(reason)
        {

        }
    }

    class ActionIncorrectException : Exception
    {
        public ActionIncorrectException(string reason) : base(reason)
        {

        }
    }
}
