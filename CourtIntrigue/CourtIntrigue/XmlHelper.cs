using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace CourtIntrigue
{
    struct Parameter
    {
        public Type Type;
        public string Name;
        public bool Inform;

        public Parameter(Type type, string name, bool inform)
        {
            Type = type;
            Name = name;
            Inform = inform;
        }
    }

    static class XmlHelper
    {
        public static IExecute ReadExecute(XmlReader reader, Dictionary<string, int> badTags)
        {
            //How did we start? Used for determining when we're done.
            string tag = reader.Name;
            List<IExecute> expressions = new List<IExecute>();
            while (reader.Read())
            {
                if (reader.NodeType == XmlNodeType.Element && reader.Name == "trigger_event")
                {
                    expressions.Add(ReadTriggerEvent(reader, badTags));
                }
                else if (reader.NodeType == XmlNodeType.Element && reader.Name == "allow_event_selection")
                {
                    expressions.Add(new AllowEventSelectionExecute());
                }
                else if (reader.NodeType == XmlNodeType.Element && (reader.Name == "everyone_in_room" || reader.Name == "any_child"))
                {
                    expressions.Add(ReadScopingLoop(reader ,badTags));
                }
                else if (reader.NodeType == XmlNodeType.Element && reader.Name == "set_scope")
                {
                    string scopeName = reader.GetAttribute("name");
                    expressions.Add(new SetScopeExecute(scopeName, ReadExecute(reader, badTags)));
                }
                else if (reader.NodeType == XmlNodeType.Element && reader.Name == "choose_character")
                {
                    string scopeName = reader.GetAttribute("name");
                    expressions.Add(ReadChooseCharacter(reader, badTags));
                }
                else if (reader.NodeType == XmlNodeType.Element && (reader.Name == "observe_information"))
                {
                    expressions.Add(ReadGainInformation(reader, badTags));
                }
                else if (reader.NodeType == XmlNodeType.Element && (reader.Name == "tell_information"))
                {
                    int overhearChance = int.Parse(reader.GetAttribute("overhear"));
                    string about = reader.GetAttribute("about");
                    string typeName = reader.GetAttribute("type");
                    InformationType type = StringToInformationType(typeName);
                    expressions.Add(new TellInformationExecute(ReadExecute(reader, badTags), overhearChance, about, type));
                }
                else if (reader.NodeType == XmlNodeType.Element && reader.Name == "prestige_change")
                {
                    expressions.Add(new PrestigeChangeExecute(reader.ReadElementContentAsInt()));
                }
                else if (reader.NodeType == XmlNodeType.Element && reader.Name == "apply_opinion_mod")
                {
                    expressions.Add(ReadApplyOpinionMod(reader, badTags));
                }
                else if (reader.NodeType == XmlNodeType.Element && reader.Name == "give_job")
                {
                    expressions.Add(new GiveJobExecute(reader.ReadElementContentAsString()));
                }
                else if (reader.NodeType == XmlNodeType.Element && reader.Name == "spend_gold")
                {
                    expressions.Add(new SpendGoldExecute(reader.ReadElementContentAsInt()));
                }
                else if (reader.NodeType == XmlNodeType.Element && reader.Name == "get_gold")
                {
                    expressions.Add(new GetGoldExecute(reader.ReadElementContentAsInt()));
                }
                else if (reader.NodeType == XmlNodeType.Element && reader.Name == "multiply_observe_chance")
                {
                    expressions.Add(new MultiplyOvserveChanceExecute(reader.ReadElementContentAsDouble()));
                }
                else if (reader.NodeType == XmlNodeType.Element && reader.Name == "debug")
                {
                    expressions.Add(Execute.DEBUG);
                }
                else if (reader.NodeType == XmlNodeType.Element && reader.Name == "if")
                {
                    expressions.Add(ReadIfExecute(reader, badTags));
                }
                else if (reader.NodeType == XmlNodeType.Element && reader.Name == "set_variable")
                {
                    string varName = reader.GetAttribute("name");
                    expressions.Add(new SetVariableExecute(varName, reader.ReadElementContentAsString()));
                }
                else if (reader.NodeType == XmlNodeType.Element && reader.Name == "offset_variable")
                {
                    string varName = reader.GetAttribute("name");
                    expressions.Add(new OffsetVariableExecute(varName, reader.ReadElementContentAsString()));
                }
                else if (reader.NodeType == XmlNodeType.Element)
                {
                    if (badTags.ContainsKey(reader.Name))
                        ++badTags[reader.Name];
                    else
                        badTags.Add(reader.Name, 1);
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


        public static ILogic ReadLogic(XmlReader reader, Dictionary<string, int> badTags)
        {
            //How did we start? Used for determining when we're done.
            string tag = reader.Name;
            List<ILogic> expressions = new List<ILogic>();
            while (reader.Read())
            {
                if (reader.NodeType == XmlNodeType.Element)
                {
                    expressions.Add(ReadSingleLogic(reader, badTags));
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
            {
                //An empty and is evaluated to false.
                if (tag == "and")
                    return Logic.FALSE;

                //These are expected to be requirements
                return Logic.TRUE;
            }
            else
                return new AndLogic(expressions.ToArray());
        }

        private static ILogic ReadSingleLogic(XmlReader reader, Dictionary<string, int> badTags)
        {
            if (reader.NodeType == XmlNodeType.Element && reader.Name == "action_id")
            {
                return new ActionIdentifierTestLogic(reader.ReadElementContentAsString());
            }
            else if (reader.NodeType == XmlNodeType.Element && reader.Name == "has_information")
            {
                string about = reader.GetAttribute("about");
                string typeName = reader.GetAttribute("type");
                InformationType type = StringToInformationType(typeName);

                return new HasInformationTestLogic(about, type);
            }
            else if (reader.NodeType == XmlNodeType.Element && reader.Name == "true")
            {
                return Logic.TRUE;
            }
            else if (reader.NodeType == XmlNodeType.Element && reader.Name == "false")
            {
                return Logic.FALSE;
            }
            else if (reader.NodeType == XmlNodeType.Element && reader.Name == "not")
            {
                return new NotLogic(ReadLogic(reader, badTags));
            }
            else if (reader.NodeType == XmlNodeType.Element && reader.Name == "and")
            {
                //ReadLogic will just and whatever it finds.
                return ReadLogic(reader, badTags);
            }
            else if (reader.NodeType == XmlNodeType.Element && reader.Name == "or")
            {
                //ReadLogic will just or whatever it finds.
                return ReadOrLogic(reader, badTags);
            }
            else if (reader.NodeType == XmlNodeType.Element && reader.Name == "has_spouse")
            {
                return Logic.HAS_SPOUSE;
            }
            else if (reader.NodeType == XmlNodeType.Element && reader.Name == "is_male")
            {
                return Logic.IS_MALE;
            }
            else if (reader.NodeType == XmlNodeType.Element && reader.Name == "is_female")
            {
                return Logic.IS_FEMALE;
            }
            else if (reader.NodeType == XmlNodeType.Element && reader.Name == "is_adult")
            {
                return Logic.IS_ADULT;
            }
            else if (reader.NodeType == XmlNodeType.Element && reader.Name == "is_spouse_of")
            {
                return new IsSpouseOfLogic(reader.ReadElementContentAsString());
            }
            else if (reader.NodeType == XmlNodeType.Element && reader.Name == "is_child_of")
            {
                return new IsChildOfLogic(reader.ReadElementContentAsString());
            }
            else if (reader.NodeType == XmlNodeType.Element && reader.Name == "is_character")
            {
                return new IsCharacterLogic(reader.ReadElementContentAsString());
            }

            else if (reader.NodeType == XmlNodeType.Element && reader.Name == "has_trait")
            {
                return new HasTraitLogic(reader.ReadElementContentAsString());
            }
            else if (reader.NodeType == XmlNodeType.Element && reader.Name == "any_child")
            {
                return new AnyChildLogic(ReadLogic(reader, badTags));
            }
            else if (reader.NodeType == XmlNodeType.Element && reader.Name == "all_child")
            {
                return new AllChildLogic(ReadLogic(reader, badTags));
            }
            else if (reader.NodeType == XmlNodeType.Element && reader.Name == "prestige_rank")
            {
                return new PrestigeRankLogic(reader.ReadElementContentAsInt());
            }
            else if (reader.NodeType == XmlNodeType.Element && reader.Name == "job_requirements")
            {
                return new JobRequirementsLogic(reader.ReadElementContentAsString());
            }
            else if (reader.NodeType == XmlNodeType.Element && reader.Name == "has_title_type")
            {
                return new HasTitleTypeLogic(reader.ReadElementContentAsString());
            }
            else if (reader.NodeType == XmlNodeType.Element && reader.Name == "has_job")
            {
                return new HasJobLogic(reader.ReadElementContentAsString());
            }
            else if (reader.NodeType == XmlNodeType.Element && reader.Name == "test_event_options")
            {
                return ReadTestEventOptionsLogic(reader, badTags);
            }
            else if (reader.NodeType == XmlNodeType.Element && reader.Name == "set_scope")
            {
                string scopeName = reader.GetAttribute("name");
                return new SetScopeLogic(scopeName, ReadLogic(reader, badTags));
            }
            else if (reader.NodeType == XmlNodeType.Element && reader.Name == "var_gt")
            {
                string varName = reader.GetAttribute("name");
                return new VariableGreaterThanLogic(varName, reader.ReadElementContentAsString());
            }
            else if (reader.NodeType == XmlNodeType.Element && reader.Name == "var_lt")
            {
                string varName = reader.GetAttribute("name");
                return new VariableLessThanLogic(varName, reader.ReadElementContentAsString());
            }
            else if (reader.NodeType == XmlNodeType.Element && reader.Name == "var_eq")
            {
                string varName = reader.GetAttribute("name");
                return new VariableEqualLogic(varName, reader.ReadElementContentAsString());
            }
            else if (reader.NodeType == XmlNodeType.Element && reader.Name == "var_ge")
            {
                string varName = reader.GetAttribute("name");
                return new VariableGreaterOrEqualLogic(varName, reader.ReadElementContentAsString());
            }
            else if (reader.NodeType == XmlNodeType.Element && reader.Name == "var_le")
            {
                string varName = reader.GetAttribute("name");
                return new VariableLessOrEqualLogic(varName, reader.ReadElementContentAsString());
            }
            else if (reader.NodeType == XmlNodeType.Element && reader.Name == "var_ne")
            {
                string varName = reader.GetAttribute("name");
                return new VariableNotEqualLogic(varName, reader.ReadElementContentAsString());
            }
            else if (reader.NodeType == XmlNodeType.Element)
            {
                if (badTags.ContainsKey(reader.Name))
                    ++badTags[reader.Name];
                else
                    badTags.Add(reader.Name, 1);
            }
            return Logic.FALSE;
        }

        public static Type StringToType(string typeName)
        {
            if (typeName == "character")
            {
                return typeof(Character);
            }
            else if (typeName == "information")
            {
                return typeof(Information);
            }

            throw new ArgumentException("Found unexpected type name " + typeName);
        }

        private static InformationType StringToInformationType(string typeName)
        {
            if (typeName == "POSITIVE")
                return InformationType.Positive;
            else if (typeName == "NEGATIVE")
                return InformationType.Negative;
            else if (typeName == "NEURTRAL")
                return InformationType.Neutral;
            else
                throw new ArgumentException("Type not understood for has_information: " + typeName);
        }

        private static ILogic ReadOrLogic(XmlReader reader, Dictionary<string, int> badTags)
        {
            List<ILogic> expressions = new List<ILogic>();
            while (reader.Read())
            {
                if (reader.NodeType == XmlNodeType.Element)
                {
                    expressions.Add(ReadSingleLogic(reader, badTags));
                }
                else if (reader.NodeType == XmlNodeType.EndElement && reader.Name == "or")
                {
                    break;
                }
            }

            //Now that we've got all our expressions, there are a few special cases.
            //If we've only got a single expression, there's no point in or-ing them, so
            //we can just return that expression.
            //If there are no expressions, we should always be false
            //Otherwise, we'll just return an or of the expressions we have.
            if (expressions.Count == 1)
                return expressions[0];
            else if (expressions.Count == 0)
                return Logic.FALSE;
            else
                return new OrLogic(expressions.ToArray());
        }

        private static IExecute ReadScopingLoop(XmlReader reader, Dictionary<string, int> badTags)
        {
            //How did we start? Used for determining when we're done.
            string tag = reader.Name;
            ILogic requirements = Logic.TRUE;
            IExecute operation = Execute.NOOP;
            while (reader.Read())
            {
                if (reader.NodeType == XmlNodeType.Element && reader.Name == "requirements")
                {
                    requirements = ReadLogic(reader, badTags);
                }
                else if (reader.NodeType == XmlNodeType.Element && reader.Name == "for_character")
                {
                    operation = ReadExecute(reader, badTags);
                }
                else if (reader.NodeType == XmlNodeType.Element)
                {
                    if (badTags.ContainsKey(reader.Name))
                        ++badTags[reader.Name];
                    else
                        badTags.Add(reader.Name, 1);
                }
                else if (reader.NodeType == XmlNodeType.EndElement && reader.Name == tag)
                {
                    break;
                }
            }

            // TODO: make this more general.
            if (tag == "everyone_in_room")
                return new EveryoneInRoomExecute(operation, requirements);
            else if (tag == "any_child")
                return new AnyChildExecute(operation, requirements);
            else
                throw new Exception("Unexpected tag " + tag);
        }

        private static IExecute ReadGainInformation(XmlReader reader, Dictionary<string, int> badTags)
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
                else if (reader.NodeType == XmlNodeType.Element)
                {
                    if (badTags.ContainsKey(reader.Name))
                        ++badTags[reader.Name];
                    else
                        badTags.Add(reader.Name, 1);
                }
                else if (reader.NodeType == XmlNodeType.EndElement && reader.Name == tag)
                {
                    break;
                }
            }

            if (tag == "observe_information")
                return new ObserveInformationExecute(id, parameters, chance);
            else
                throw new Exception("Found unknown information tag: " + tag);
        }

        private static IExecute ReadApplyOpinionMod(XmlReader reader, Dictionary<string, int> badTags)
        {
            string id = null;
            string character = null;
            while (reader.Read())
            {
                if (reader.NodeType == XmlNodeType.Element && reader.Name == "id")
                {
                    id = reader.ReadElementContentAsString();
                }
                else if (reader.NodeType == XmlNodeType.Element && reader.Name == "character")
                {
                    character = reader.ReadElementContentAsString();
                }
                else if (reader.NodeType == XmlNodeType.Element)
                {
                    if (badTags.ContainsKey(reader.Name))
                        ++badTags[reader.Name];
                    else
                        badTags.Add(reader.Name, 1);
                }
                else if (reader.NodeType == XmlNodeType.EndElement && reader.Name == "apply_opinion_mod")
                {
                    break;
                }
            }

            return new ApplyOpinionModifierExecute(id, character);
        }

        private static IExecute ReadTriggerEvent(XmlReader reader, Dictionary<string, int> badTags)
        {
            string id = null;
            string tag = reader.Name;
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            while (reader.Read())
            {
                if (reader.NodeType == XmlNodeType.Element && reader.Name == "id")
                {
                    id = reader.ReadElementContentAsString();
                }
                else if (reader.NodeType == XmlNodeType.Element && reader.Name == "parameters")
                {
                    while (reader.MoveToNextAttribute())
                    {
                        parameters.Add(reader.Name, reader.Value);
                    }
                }
                else if (reader.NodeType == XmlNodeType.Element)
                {
                    if (badTags.ContainsKey(reader.Name))
                        ++badTags[reader.Name];
                    else
                        badTags.Add(reader.Name, 1);
                }
                else if (reader.NodeType == XmlNodeType.EndElement && reader.Name == tag)
                {
                    break;
                }
            }

            if (tag == "trigger_event")
                return new TriggerEventExecute(id, parameters);
            else
                throw new Exception("Found unknown information tag: " + tag);
        }

        public static List<Parameter> ReadParameters(XmlReader reader, Dictionary<string, int> badTags)
        {
            List<Parameter> parameters = new List<Parameter>();
            while (reader.Read())
            {
                if (reader.NodeType == XmlNodeType.Element && reader.Name == "parameter")
                {
                    string type = reader.GetAttribute("type");
                    bool inform = reader.GetAttribute("inform") != "No";
                    string name = reader.ReadElementContentAsString();
                    parameters.Add(new Parameter(XmlHelper.StringToType(type), name, inform));
                }
                else if (reader.NodeType == XmlNodeType.Element)
                {
                    if (badTags.ContainsKey(reader.Name))
                        ++badTags[reader.Name];
                    else
                        badTags.Add(reader.Name, 1);
                }
                else if (reader.NodeType == XmlNodeType.EndElement && reader.Name == "parameters")
                {
                    break;
                }
            }
            return parameters;
        }

        public static string[] ReadList(XmlReader reader, string elementTag, Dictionary<string, int> badTags)
        {
            string tag = reader.Name;
            List<string> elements = new List<string>();
            while (reader.Read())
            {
                if (reader.NodeType == XmlNodeType.Element && reader.Name == elementTag)
                {
                    elements.Add(reader.ReadElementContentAsString());
                }
                else if (reader.NodeType == XmlNodeType.Element)
                {
                    if (badTags.ContainsKey(reader.Name))
                        ++badTags[reader.Name];
                    else
                        badTags.Add(reader.Name, 1);
                }
                else if (reader.NodeType == XmlNodeType.EndElement && reader.Name == tag)
                {
                    break;
                }
            }
            return elements.ToArray();
        }

        private static ILogic ReadTestEventOptionsLogic(XmlReader reader, Dictionary<string, int> badTags)
        {
            string id = null;
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            while (reader.Read())
            {
                if (reader.NodeType == XmlNodeType.Element && reader.Name == "id")
                {
                    id = reader.ReadElementContentAsString();
                }
                else if (reader.NodeType == XmlNodeType.Element && reader.Name == "parameters")
                {
                    while (reader.MoveToNextAttribute())
                    {
                        parameters.Add(reader.Name, reader.Value);
                    }
                }
                else if (reader.NodeType == XmlNodeType.Element)
                {
                    if (badTags.ContainsKey(reader.Name))
                        ++badTags[reader.Name];
                    else
                        badTags.Add(reader.Name, 1);
                }
                else if (reader.NodeType == XmlNodeType.EndElement && reader.Name == "test_event_options")
                {
                    break;
                }
            }

            return new TestEventOptionsLogic(id, parameters);
        }

        private static IExecute ReadIfExecute(XmlReader reader, Dictionary<string, int> badTags)
        {
            ILogic requirements = Logic.TRUE;
            IExecute thenExecute = Execute.NOOP;
            IExecute elseExecute = Execute.NOOP;
            while (reader.Read())
            {
                if (reader.NodeType == XmlNodeType.Element && reader.Name == "requirements")
                {
                    requirements = ReadLogic(reader, badTags);
                }
                else if (reader.NodeType == XmlNodeType.Element && reader.Name == "then")
                {
                    thenExecute = ReadExecute(reader, badTags);
                }
                else if (reader.NodeType == XmlNodeType.Element && reader.Name == "else")
                {
                    elseExecute = ReadExecute(reader, badTags);
                }
                else if (reader.NodeType == XmlNodeType.Element)
                {
                    if (badTags.ContainsKey(reader.Name))
                        ++badTags[reader.Name];
                    else
                        badTags.Add(reader.Name, 1);
                }
                else if (reader.NodeType == XmlNodeType.EndElement && reader.Name == "if")
                {
                    break;
                }
            }

            return new IfExecute(requirements, thenExecute, elseExecute);
        }

        private static IExecute ReadChooseCharacter(XmlReader reader, Dictionary<string, int> badTags)
        {
            ILogic requirements = Logic.TRUE;
            IExecute exec = Execute.NOOP;
            string name = reader.GetAttribute("name");
            while (reader.Read())
            {
                if (reader.NodeType == XmlNodeType.Element && reader.Name == "requirements")
                {
                    requirements = ReadLogic(reader, badTags);
                }
                else if (reader.NodeType == XmlNodeType.Element && reader.Name == "exec")
                {
                    exec = ReadExecute(reader, badTags);
                }
                else if (reader.NodeType == XmlNodeType.EndElement && reader.Name == "choose_character")
                {
                    break;
                }
            }

            return new ChooseCharacterExecute(name, exec, requirements);
        }


        public static int GetTestValue(EventContext context, Game game, string value)
        {
            //These values must match the special cases on IsSpecialName
            if (value == "TIME")
                return game.CurrentTime;
            else if (value == "AGE")
                return context.CurrentCharacter.Age;
            else if (value == "GOLD")
                return context.CurrentCharacter.Money;

            int intValue;
            if (int.TryParse(value, out intValue))
                return intValue;

            return context.CurrentCharacter.GetVariable(value);
        }

        public static bool IsSpecialName(string value)
        {
            //These values must match the special cases on GetTestValue
            if (value == "TIME" || value == "AGE" || value == "GOLD")
                return true;

            return false;
        }
    }


}
