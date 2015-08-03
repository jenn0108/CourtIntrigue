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
        bool Evaluate(EventContext context);
    }

    class AndLogic : ILogic
    {
        private ILogic[] subexpressions;
        public AndLogic(ILogic[] subexps)
        {
            subexpressions = subexps;
        }
        public bool Evaluate(EventContext context)
        {
            for(int i = 0; i < subexpressions.Length; ++i)
            {
                if (!subexpressions[i].Evaluate(context))
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
        public bool Evaluate(EventContext context)
        {
            return context.Identifer == identifier;
        }
    }

    class HasInformationTestLogic : ILogic
    {
        public bool Evaluate(EventContext context)
        {
            return context.CurrentCharacter.HasInformation();
        }
    }

    class HasSpouseTestLogic : ILogic
    {
        public bool Evaluate(EventContext context)
        {
            return context.CurrentCharacter.Spouse != null;
        }
    }

    class TrueLogic : ILogic
    {
        public bool Evaluate(EventContext context)
        {
            return true;
        }
    }

    class FalseLogic : ILogic
    {
        public bool Evaluate(EventContext context)
        {
            return false;
        }
    }
}
