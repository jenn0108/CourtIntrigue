﻿using System;
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

        public string Description { get { return EventHelper.ReplaceStrings(information.Description, this); } }

        public InformationInstance(Information information, Dictionary<string, object> parameters)
        {
            this.information = information;
            this.parameters = parameters;
        }

        public object GetParameter(string name)
        {
            return parameters[name];
        }

        public void ExecuteOnObserve(Character currentCharacter, Game game, Room room, Event e)
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
            EventContext observeContext = new EventContext("", currentCharacter, null, room, parameters);
            information.OnObserve.Execute(new EventResults(), game, observeContext, e);
        }

        public void ExecuteOnTold(Character currentCharacter, Character tellingCharacter, Game game, Room room, Event e)
        {
            //The current character is the root of the new context so that they will be the
            //default scope in the on_observe we are about to run.
            EventContext observeContext = new EventContext("", currentCharacter, tellingCharacter, room, parameters);
            information.OnTold.Execute(new EventResults(), game, observeContext, e);
        }

        public override bool Equals(object obj)
        {
            if(obj is InformationInstance)
            {
                //TODO:We need to add a timestamp to this class and check it here
                //to avoid the problem of two incidents of the same type being
                //equal even when they happen on different days/steps.
                InformationInstance other = obj as InformationInstance;

                if (other.information != information || other.parameters.Count != parameters.Count)
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

        public override int GetHashCode()
        {
            int code = information.GetHashCode() * 34578454;
            foreach(var pair in parameters)
            {
                code += pair.Key.GetHashCode() + pair.Value.GetHashCode() * 545345;
            }
            return code;
        }
    }
}
