﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CourtIntrigue
{

    class Action
    {
        public static string APPROACH_ACTION = "APPROACH_ACTION";
        public static string EAVESDROP_ACTION = "EAVESDROP_ACTION";
        public static string PUBLIC_URINATION_ACTION = "PUBLIC_URINATION_ACTION";

        public string Identifer { get; private set; }
        public Character Target { get; private set; }

        public Action(string ident, Character target)
        {
            Identifer = ident;
            Target = target;
        }
    }
}