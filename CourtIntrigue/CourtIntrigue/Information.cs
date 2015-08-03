using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CourtIntrigue
{
    class Information
    {
        public string Identifier { get; private set; }
        public Parameter[] Parameters { get; private set; }
        public string Description { get; private set; }
        public IExecute OnObserve { get; private set; }
        public IExecute OnTold { get; private set; }

        public Information(string identifier, string description, Parameter[] parameters, IExecute onObserve, IExecute onTold)
        {
            Identifier = identifier;
            Description = description;
            Parameters = parameters;
            OnObserve = onObserve;
            OnTold = onTold;
        }
    }
}
