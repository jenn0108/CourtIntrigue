﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CourtIntrigue
{
    class Dynasty
    {
        public string Name { get; private set; }

        public Dynasty(string name)
        {
            Name = name;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
