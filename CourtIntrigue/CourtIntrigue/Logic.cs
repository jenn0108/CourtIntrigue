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
        public static ILogic FALSE = new TrueLogic();
    }

    interface ILogic
    {
        bool Evaluate(Action action);
    }

    class AndLogic : ILogic
    {
        private ILogic[] subexpressions;
        public AndLogic(ILogic[] subexps)
        {
            subexpressions = subexps;
        }
        public bool Evaluate(Action action)
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
        public bool Evaluate(Action action)
        {
            return action.Identifer == identifier;
        }
    }

    class TrueLogic : ILogic
    {
        public bool Evaluate(Action action)
        {
            return true;
        }
    }

    class FalseLogic : ILogic
    {
        public bool Evaluate(Action action)
        {
            return false;
        }
    }
}
