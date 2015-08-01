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

        public Event FindEventForAction(Action action, Random R)
        {
            //Which events can we perform?
            List<Event> okToRun = new List<Event>();
            //We really should be smarter about this.
            foreach(var pair in events)
            {
                if(pair.Value.ActionRequirements.Evaluate(action))
                {
                    okToRun.Add(pair.Value);
                }
            }
            if (okToRun.Count == 0)
                return null;
            else if (okToRun.Count == 1)
                return okToRun[0];

            return okToRun[R.Next(okToRun.Count)];
        }

        public Event FindEventById(string id)
        {
            //We can just let this throw an exeception if the event isn't present
            //for the time being.  In the future, we probably want to log the fact
            //and continue.
            return events[id];
        }

        public void LoadEventsFromFile(string filename)
        {
            using (XmlReader reader = XmlReader.Create(filename))
            {
                while (reader.Read())
                {
                    if (reader.NodeType == XmlNodeType.Element && reader.Name == "events")
                    {
                        ReadEvents(reader);
                    }
                }
            }
        }

        private void ReadEvents(XmlReader reader)
        {
            while (reader.Read())
            {
                if(reader.NodeType == XmlNodeType.Element && reader.Name == "event")
                {
                    Event e = ReadEvent(reader);
                    events.Add(e.Identifier, e);
                }
                else if (reader.NodeType == XmlNodeType.EndElement && reader.Name == "events")
                {
                    break;
                }
            }
        }

        private Event ReadEvent(XmlReader reader)
        {
            string identifier = null;
            string description = null;

            //Actions without any action logic can't be triggered in FindEventForAction
            //so we can just use FALSE so they'll always be unavailable.
            ILogic actionLogic = Logic.FALSE;

            //Actions without a top level exec shouldn't do anything in their exec.
            IExecute dirExec = Execute.NOOP;
            List<EventOption> options = new List<EventOption>();
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
                    actionLogic = XmlHelper.ReadLogic(reader);
                }
                else if (reader.NodeType == XmlNodeType.Element && reader.Name == "exec")
                {
                    dirExec = XmlHelper.ReadExecute(reader);
                }
                else if (reader.NodeType == XmlNodeType.Element && reader.Name == "options")
                {
                    options = ReadOptions(reader);
                }
                else if (reader.NodeType == XmlNodeType.EndElement && reader.Name == "event")
                {
                    break;
                }
            }
            return new Event(identifier, description, actionLogic, dirExec, options.ToArray());
        }

        private List<EventOption> ReadOptions(XmlReader reader)
        {
            List<EventOption> options = new List<EventOption>();
            while (reader.Read())
            {
                if (reader.NodeType == XmlNodeType.Element && reader.Name == "option")
                {
                    options.Add(ReadOption(reader));
                }
                else if (reader.NodeType == XmlNodeType.EndElement && reader.Name == "options")
                {
                    break;
                }
            }
            return options;
        }


        private EventOption ReadOption(XmlReader reader)
        {
            string label = null;

            //Options without an exec shouldn't do anything.
            IExecute dirExec = Execute.NOOP;

            while (reader.Read())
            {
                if (reader.NodeType == XmlNodeType.Element && reader.Name == "label")
                {
                    label = reader.ReadElementContentAsString();
                }
                else if (reader.NodeType == XmlNodeType.Element && reader.Name == "exec")
                {
                    dirExec = XmlHelper.ReadExecute(reader);
                }
                else if (reader.NodeType == XmlNodeType.EndElement && reader.Name == "option")
                {
                    break;
                }
            }
            return new EventOption(label, dirExec);
        }


        
    }
}
