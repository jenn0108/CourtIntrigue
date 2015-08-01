using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CourtIntrigue
{
    struct InformationParameter
    {
        Type Type;
        string Name;

        public InformationParameter(Type type, string name)
        {
            Type = type;
            Name = name;
        }
    }

    class Information
    {
        public string Identifier { get; private set; }
        public InformationParameter[] Parameters { get; private set; }
        public string Description { get; private set; }

        public Information(string identifier, string description, InformationParameter[] parameters)
        {
            Identifier = identifier;
            Description = description;
            Parameters = parameters;
        }
    }
}
