﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace CourtIntrigue
{
    static class XmlHelper
    {
        public static IExecute ReadExecute(XmlReader reader)
        {
            //How did we start? Used for determining when we're done.
            string tag = reader.Name;
            List<IExecute> expressions = new List<IExecute>();
            while (reader.Read())
            {
                if (reader.NodeType == XmlNodeType.Element && reader.Name == "target_event")
                {
                    expressions.Add(new TargetEventExecute(reader.ReadElementContentAsString()));
                }
                else if (reader.NodeType == XmlNodeType.Element && reader.Name == "allow_event_selection")
                {
                    expressions.Add(new AllowEventSelectionExecute());
                }
                else if (reader.NodeType == XmlNodeType.Element && reader.Name == "everyone_in_room")
                {
                    expressions.Add(ReadScopingLoop(reader));
                }
                else if (reader.NodeType == XmlNodeType.Element && (reader.Name == "observe_information" ||
                         reader.Name == "tell_information"))
                {
                    expressions.Add(ReadGainInformation(reader));
                }
                else if (reader.NodeType == XmlNodeType.EndElement && reader.Name == tag)
                {
                    break;
                }
            }

            //Now that we've got all our expressions, there are a few special cases.
            //If we've only got a single expression, there's no point in anding them, so
            //we can just return that expression.
            //If there are no expressions, we should always be true (no restrictions.)
            //Otherwise, we'll just return an and of the expressions we have.
            if (expressions.Count == 1)
                return expressions[0];
            else if (expressions.Count == 0)
                return Execute.NOOP;
            else
                return new SequenceExecute(expressions.ToArray());
        }


        public static ILogic ReadLogic(XmlReader reader)
        {
            //How did we start? Used for determining when we're done.
            string tag = reader.Name;
            List<ILogic> expressions = new List<ILogic>();
            while (reader.Read())
            {
                if (reader.NodeType == XmlNodeType.Element && reader.Name == "action_id")
                {
                    expressions.Add(new ActionIdentifierTestLogic(reader.ReadElementContentAsString()));
                }
                else if (reader.NodeType == XmlNodeType.EndElement && reader.Name == tag)
                {
                    break;
                }
            }

            //Now that we've got all our expressions, there are a few special cases.
            //If we've only got a single expression, there's no point in anding them, so
            //we can just return that expression.
            //If there are no expressions, we should always be true (no restrictions.)
            //Otherwise, we'll just return an and of the expressions we have.
            if (expressions.Count == 1)
                return expressions[0];
            else if (expressions.Count == 0)
                return Logic.TRUE;
            else
                return new AndLogic(expressions.ToArray());
        }

        public static Type StringToType(string typeName)
        {
            if (typeName == "character")
            {
                return typeof(Character);
            }

            throw new ArgumentException("Found unexpected type name " + typeName);
        }

        private static IExecute ReadScopingLoop(XmlReader reader)
        {
            //How did we start? Used for determining when we're done.
            string tag = reader.Name;
            ILogic requirements = Logic.TRUE;
            IExecute operation = Execute.NOOP;
            while (reader.Read())
            {
                if (reader.NodeType == XmlNodeType.Element && reader.Name == "requirements")
                {
                    requirements = ReadLogic(reader);
                }
                else if (reader.NodeType == XmlNodeType.Element && reader.Name == "for_character")
                {
                    operation = ReadExecute(reader);
                }
                else if (reader.NodeType == XmlNodeType.EndElement && reader.Name == tag)
                {
                    break;
                }
            }

            // TODO: make this more general.
            return new EveryoneInRoomExecute(operation, requirements);
        }

        private static IExecute ReadGainInformation(XmlReader reader)
        {
            string id = null;
            int chance = 100;
            string tag = reader.Name;
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            while (reader.Read())
            {
                if (reader.NodeType == XmlNodeType.Element && reader.Name == "id")
                {
                    id = reader.ReadElementContentAsString();
                }
                else if (reader.NodeType == XmlNodeType.Element && reader.Name == "chance")
                {
                    chance = reader.ReadElementContentAsInt();
                }
                else if (reader.NodeType == XmlNodeType.Element && reader.Name == "parameters")
                {
                    while (reader.MoveToNextAttribute())
                    {
                        parameters.Add(reader.Name, reader.Value);
                    }
                }
                else if (reader.NodeType == XmlNodeType.EndElement && reader.Name == tag)
                {
                    break;
                }
            }

            if (tag == "observe_information")
                return new ObserveInformationExecute(id, parameters, chance);
            else if (tag == "add_information")
                return new TellInformationExecute(id, parameters);
            else
                throw new Exception("Found unknown information tag: " + tag);
        }
    }
}
