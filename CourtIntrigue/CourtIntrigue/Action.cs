using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace CourtIntrigue
{

    class Action
    {
        public static string CUSTOM_ACTION = "CUSTOM_ACTION";
        public static string APPROACH_ACTION = "APPROACH_ACTION";
        public static string EAVESDROP_ACTION = "EAVESDROP_ACTION";
        public static string PUBLIC_URINATION_ACTION = "PUBLIC_URINATION_ACTION";

        public string Identifer { get; private set; }
        public Character Initiator { get; private set; }
        public Character Target { get; private set; }

        public Action(string ident, Character initiator, Character target)
        {
            Identifer = ident;
            Initiator = initiator;
            Target = target;
        }
    }


    static class ActionHelper
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
            PropertyInfo info = properties[parts[index]];

            //Evaluate it on the object we've been given.
            object val = info.GetValue(work);

            //Recurse in case we have more properties to evaluate
            return EvaluateProperty(parts, index + 1, val);
        }
    }
}
