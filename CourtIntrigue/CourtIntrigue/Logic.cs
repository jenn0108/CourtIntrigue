using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CourtIntrigue
{
    class Logic
    {
        public static ILogic TRUE = new TrueLogic();
        public static ILogic FALSE = new FalseLogic();
    }

    interface ILogic
    {
        bool Evaluate(EventContext action);
    }

    class AndLogic : ILogic
    {
        private ILogic[] subexpressions;
        public AndLogic(ILogic[] subexps)
        {
            subexpressions = subexps;
        }
        public bool Evaluate(EventContext action)
        {
            for(int i = 0; i < subexpressions.Length; ++i)
            {
                if (!subexpressions[i].Evaluate(action))
                    return false;
            }
            return true;
        }
    }

    class ActionIdentifierTestLogic : ILogic
    {
        private string identifier;
        public ActionIdentifierTestLogic(string lookingFor)
        {
            identifier = lookingFor;
        }
        public bool Evaluate(EventContext action)
        {
            return action.Identifer == identifier;
        }
    }

    class HasInformationTestLogic : ILogic
    {
        public bool Evaluate(EventContext context)
        {
            return context.CurrentScope.KnownInformation.Count() > 0;
        }
    }

    class TrueLogic : ILogic
    {
        public bool Evaluate(EventContext action)
        {
            return true;
        }
    }

    class FalseLogic : ILogic
    {
        public bool Evaluate(EventContext action)
        {
            return false;
        }
    }
}
