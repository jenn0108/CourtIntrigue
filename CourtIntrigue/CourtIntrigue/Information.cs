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

        public Information(string identifier, string description, Parameter[] parameters)
        {
            Identifier = identifier;
            Description = description;
            Parameters = parameters;
        }
    }
}
