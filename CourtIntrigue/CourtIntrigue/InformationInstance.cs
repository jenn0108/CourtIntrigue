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

        public InformationInstance(Information information, Dictionary<string, object> parameters)
        {
            this.information = information;
            this.parameters = parameters;
        }
    }
}
