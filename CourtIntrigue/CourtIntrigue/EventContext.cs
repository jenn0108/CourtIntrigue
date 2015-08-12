using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace CourtIntrigue
{

    class EventContext
    {
        public string Identifer { get; private set; }
        public Character Target { get; private set; }
        public Room Room { get { return (scopes.First().Value as Character).CurrentRoom; } }
        private Dictionary<string, object> parameters;
        private List<KeyValuePair<string,object>> scopes = new List<KeyValuePair<string, object>>();
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

        public EventContext(string ident, Character initiator, Character target)
        {
            Identifer = ident;
            Target = target;
            scopes.Add(new KeyValuePair<string, object>("ROOT", initiator));
            parameters = new Dictionary<string, object>();
        }

        public EventContext(string ident, Character initiator, Character target, Dictionary<string, object> parameters)
        {
            Identifer = ident;
            Target = target;
            scopes.Add(new KeyValuePair<string, object>("ROOT", initiator));
            this.parameters = parameters;
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
            if (name == "TARGET")
            {
                return Target;
            }
            else if (name == "TOP")
            {
                return CurrentScope;
            }

            foreach (var pair in scopes)
            {
                if (pair.Key == name)
                    return pair.Value;
            }
            return parameters[name];
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
        /// $Target.Name$ will be replaced with the target's name if working is an Action
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
