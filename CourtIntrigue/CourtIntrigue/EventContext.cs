﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace CourtIntrigue
{

    class EventContext
    {
        private Dictionary<string, object> parameters;
        private List<KeyValuePair<string,object>> scopes = new List<KeyValuePair<string, object>>();
        private Dictionary<Character, Dictionary<string, double>> changedVariables = new Dictionary<Character, Dictionary<string, double>>();


        public object CurrentScope
        {
            get { return scopes.Last().Value; }
        }

        public Character CurrentCharacter
        {
            get
            {
                for(int i = scopes.Count-1; i >= 0; --i)
                {
                    if (scopes[i].Value is Character)
                        return scopes[i].Value as Character;
                }
                throw new Exception("There is no ROOT character in context.");
            }
        }

        public EventContext(EventContext context)
        {
            //Copy the scopes.
            scopes.AddRange(context.scopes);

            //We can refer to the other parameters directly because we never change them.
            parameters = context.parameters;

            //Copy all the changed variables.
            foreach(var charVars in context.changedVariables)
            {
                foreach(var varPair in charVars.Value)
                {
                    SetVariable(charVars.Key, varPair.Key, varPair.Value);
                }
            }
        }

        public EventContext(Character initiator)
        {
            scopes.Add(new KeyValuePair<string, object>("ROOT", initiator));
            parameters = new Dictionary<string, object>();
        }

        public EventContext(Character initiator, Dictionary<string, object> parameters)
        {
            scopes.Add(new KeyValuePair<string, object>("ROOT", initiator));
            this.parameters = parameters;
        }

        public EventContext(ActionDescriptor actionDescriptor)
        {
            scopes.Add(new KeyValuePair<string, object>("ROOT", actionDescriptor.Initiator));
            parameters = new Dictionary<string, object>();
            if (actionDescriptor.Action.Type == ActionType.Pair && actionDescriptor.Target != null && actionDescriptor.Action.ParameterName != null)
            {
                parameters.Add(actionDescriptor.Action.ParameterName, actionDescriptor.Target);
            }

        }

        public void PushScope(object newObject, string name = null)
        {
            scopes.Add(new KeyValuePair<string, object>(name, newObject));
        }

        public void PopScope()
        {
            scopes.RemoveAt(scopes.Count-1);
        }

        public object GetScopedObjectByName(string name)
        {
            if (name == "TOP")
            {
                return CurrentScope;
            }
            else if (name == "SPOUSE")
            {
                return CurrentCharacter.Spouse;
            }

            foreach (var pair in scopes)
            {
                if (pair.Key == name)
                    return pair.Value;
            }
            return parameters[name];
        }

        public bool HasChanges()
        {
            return changedVariables.Count != 0;
        }

        public Character[] GetCharacters()
        {
            List<Character> characters = new List<Character>();
            for(int i = scopes.Count-1; i > 1; --i)
            {
                if (scopes[i].Value is Character)
                    characters.Add(scopes[i].Value as Character);
            }
            foreach(var pair in parameters)
            {
                if (pair.Value is Character)
                    characters.Add(pair.Value as Character);
            }
            return characters.ToArray();
        }

        public void Commit()
        {
            foreach(var charVarPair in changedVariables)
            {
                var character = charVarPair.Key;
                foreach(var varPair in charVarPair.Value)
                {
                    var name = varPair.Key;
                    var value = varPair.Value;
                    character.SetVariable(name, value);
                }
            }
        }

        public void CommitTo(EventContext parent)
        {
            foreach (var charVarPair in changedVariables)
            {
                var character = charVarPair.Key;
                foreach (var varPair in charVarPair.Value)
                {
                    var name = varPair.Key;
                    var value = varPair.Value;
                    parent.SetVariable(character, name, value);
                }
            }
        }

        public double GetVariable(Character character, string variable)
        {
            Dictionary<string, double> variables = null;
            if (!changedVariables.TryGetValue(character, out variables))
            {
                return character.GetVariable(variable);
            }

            double val = 0.0;
            if(variables.TryGetValue(variable, out val))
            {
                return val;
            }

            return character.GetVariable(variable);
        }

        public void OffsetVariable(Character character, string variable, double offset)
        {
            Dictionary<string, double> variables = null;
            if(!changedVariables.TryGetValue(character, out variables))
            {
                variables = new Dictionary<string, double>();
                changedVariables.Add(character, variables);
            }

            double prevValue = 0.0;
            if(variables.TryGetValue(variable, out prevValue))
            {
                variables[variable] = prevValue + offset;
            }
            else
            {
                variables.Add(variable, character.GetVariable(variable) + offset);
            }
        }

        public void SetVariable(Character character, string variable, double newValue)
        {
            Dictionary<string, double> variables = null;
            if (!changedVariables.TryGetValue(character, out variables))
            {
                variables = new Dictionary<string, double>();
                changedVariables.Add(character, variables);
            }

            double prevValue = 0.0;
            if (variables.TryGetValue(variable, out prevValue))
            {
                variables[variable] = newValue;
            }
            else
            {
                variables.Add(variable, newValue);
            }
        }
    }

    // What we originally called "Action". This is now the thing that Character.Tick()
    // returns to describe which action the character wants to take. It is then used
    // to create an EventContext.
    class ActionDescriptor
    {
        public Action Action { get; private set; }
        public Character Initiator { get; private set; }
        public Character Target { get; private set; }

        public ActionDescriptor(Action action, Character initiator, Character target)
        {
            Action = action;
            Initiator = initiator;
            Target = target;
        }
    }


    static class EventHelper
    { 
        /// <summary>
        /// Replaces $ denominated strings with their values.
        /// </summary>
        /// <param name="text">The input text which may contain $ denominated substrings.</param>
        /// <param name="working">The object to pull values from.</param>
        /// <returns>A string with all $ denominated substrings replaced</returns>
        /// <example>
        /// $ROOT.Name$ will be replaced with the root character's name if working is an EventContext
        /// $$ is replaced with only a single $ allowing escaping of the $
        /// </example>
        /// <remarks>
        /// All properties are evaluated using ToString() (only matters if the string isn't a property.)
        /// </remarks>
        public static string ReplaceStrings(string text, object working)
        {
            //Don't spend any time doing things if there aren't any substrings to replace.
            int pos = text.IndexOf('$');
            if (pos == -1)
                return text;

            StringBuilder builder = new StringBuilder();
            int lastPos = 0;

            while (pos > -1)
            {
                //Add in everything before the first $.
                builder.Append(text.Substring(lastPos, pos - lastPos));
                //Look for the ending $.
                int endPos = text.IndexOf('$', pos + 1);

                //If they are adjacent, we're dealing with $$=>$.
                if(endPos == pos+1)
                {
                    builder.Append('$');
                    lastPos = endPos + 1;
                }
                else
                {
                    //Find the substring
                    string substr = text.Substring(pos + 1, endPos - pos - 1);

                    //Break it into parts split by .
                    string[] parts = substr.Split(new char[] { '.' });

                    //Go do the work and figure out what the actual property is.
                    //We may wish to wrap this in a try/catch block in the future to prevent
                    //bad data from crashing the program.
                    string val = EvaluateProperty(parts, 0, working);

                    //Put the replacement in place of the substring.
                    builder.Append(val);
                    lastPos = endPos + 1;
                }
                pos = text.IndexOf('$', lastPos);
            }

            //Don't forget the stuff after the last $.
            if(lastPos < text.Length)
            {
                builder.Append(text.Substring(lastPos));
            }
            return builder.ToString();
        }

        //A cache of Type=>(Name=>Property) so we don't need to constantly fetch properties.
        private static Dictionary<Type, Dictionary<string, PropertyInfo>> s_typeCache = new Dictionary<Type, Dictionary<string, PropertyInfo>>();

        private static string EvaluateProperty(string[] parts, int index, object work)
        {
            //Base case: Did we run out of subparts?  If so, we want to stringify whatever
            //we have left and return that.
            if (index >= parts.Length)
                return work.ToString();

            Type type = work.GetType();

            //Have we already seen this type?  If not, add it to our cache.
            Dictionary<string, PropertyInfo> properties = null;
            if (!s_typeCache.TryGetValue(type, out properties))
            {
                properties = new Dictionary<string, PropertyInfo>();
                foreach(var p in type.GetProperties())
                {
                    properties.Add(p.Name, p);
                }
                s_typeCache.Add(type, properties);
            }

            //Get the property.  Will throw exception if it isn't a property.
            if(properties.ContainsKey(parts[index]))
            {
                PropertyInfo info = properties[parts[index]];

                //Evaluate it on the object we've been given.
                object val = info.GetValue(work);

                //Recurse in case we have more properties to evaluate
                return EvaluateProperty(parts, index + 1, val);
            }
            else if(work is EventContext)
            {
                EventContext context = work as EventContext;
                return EvaluateProperty(parts, index + 1, context.GetScopedObjectByName(parts[index]));
            }
            else if (work is InformationInstance)
            {
                InformationInstance info = work as InformationInstance;
                return EvaluateProperty(parts, index + 1, info.GetParameter(parts[index]));
            }
            else
            {
                
                throw new Exception("Unhandled type " + type);
            }
        }
    }

    public static class IEnumerableExt
    {
        /// <summary>
        /// Wraps this object instance into an IEnumerable&lt;T&gt;
        /// consisting of a single item.
        /// </summary>
        /// <typeparam name="T"> Type of the object. </typeparam>
        /// <param name="item"> The instance that will be wrapped. </param>
        /// <returns> An IEnumerable&lt;T&gt; consisting of a single item. </returns>
        public static IEnumerable<T> Yield<T>(this T item)
        {
            yield return item;
        }
    }
}
