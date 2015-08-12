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
        //I'm not sure if we actually want to store events by id.
        private Dictionary<string, Event> events = new Dictionary<string, Event>();

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

        public string[] FindAllowableActions(Room room, Character initiator, Character target, Game game)
        {
            List<string> actions = new List<string>();
            foreach(var actionId in room.PairActions)
            {
                EventContext action = new EventContext(actionId, initiator, target);
                foreach (var pair in events)
                {
                    if (pair.Value.ActionRequirements.Evaluate(action, game))
                    {
                        actions.Add(actionId);
                        break;
                    }
                }
            }

            return actions.ToArray();
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
                    parameters = XmlHelper.ReadParameters(reader);

                }
                else if (reader.NodeType == XmlNodeType.EndElement && reader.Name == "event")
                {
                    break;
                }
            }
            return new Event(identifier, description, actionLogic, dirExec, options.ToArray(), parameters.ToArray());
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
}
