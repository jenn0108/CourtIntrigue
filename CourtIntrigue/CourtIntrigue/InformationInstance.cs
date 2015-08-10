using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CourtIntrigue
{
    class InformationInstance
    {
        private Dictionary<string, object> parameters;
        private Information information;
        private int time;

        public string Description { get { return EventHelper.ReplaceStrings(information.Description, this); } }

        public InformationInstance(Information information, Dictionary<string, object> parameters, int time)
        {
            this.information = information;
            this.parameters = parameters;
            this.time = time;
        }

        public object GetParameter(string name)
        {
            return parameters[name];
        }

        public void ExecuteOnObserve(Character currentCharacter, Game game, Room room)
        {
            foreach(var param in information.Parameters)
            {
                if(param.Type == typeof(Character) && param.Inform)
                {
                    Character curr = parameters[param.Name] as Character;
                    curr.AddHistory(this);
                }
            }

            //The current character is the root of the new context so that they will be the
            //default scope in the on_observe we are about to run.
            EventContext observeContext = new EventContext("", currentCharacter, null, parameters);
            information.OnObserve.Execute(new EventResults(), game, observeContext);
        }

        public void ExecuteOnTold(Character currentCharacter, Character tellingCharacter, Game game, Room room)
        {
            //The current character is the root of the new context so that they will be the
            //default scope in the on_observe we are about to run.
            EventContext observeContext = new EventContext("", currentCharacter, tellingCharacter, parameters);
            information.OnTold.Execute(new EventResults(), game, observeContext);
        }

        public bool IsExpired(int currentDayInTicks)
        {
            return time + information.Expires * Game.TICKS_PER_DAY <= currentDayInTicks;
        }

        public override bool Equals(object obj)
        {
            if(obj is InformationInstance)
            {
                InformationInstance other = obj as InformationInstance;

                if (other.information != information || other.parameters.Count != parameters.Count || other.time != time)
                    return false;

                foreach(var pair in parameters)
                {
                    if (!other.parameters.ContainsKey(pair.Key))
                        return false;

                    if (other.parameters[pair.Key] != pair.Value)
                        return false;
                }
                return true;
            }
            return false;
        }

        public bool IsAbout(Character character)
        {
            foreach(var pair in parameters)
            {
                if (pair.Value == character)
                    return true;
            }
            return false;
        }

        public override int GetHashCode()
        {
            int code = information.GetHashCode() * 34578454;
            foreach(var pair in parameters)
            {
                code += pair.Key.GetHashCode() + pair.Value.GetHashCode() * 545345;
            }
            code += time * 89633;

            return code;
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();

            builder.Append(information.Identifier);
            builder.Append(" (");
            foreach (var pair in parameters)
            {
                builder.Append(pair.Key);
                builder.Append(" = ");
                builder.Append(pair.Value);
                builder.Append(" ");
            }
            builder.Length -= 1;
            builder.Append(")");
            return builder.ToString();
        }
    }
}
