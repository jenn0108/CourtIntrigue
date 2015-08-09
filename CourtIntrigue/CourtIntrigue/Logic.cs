using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CourtIntrigue
{
    class Logic
    {
        //Classes with no members only need a single instance.
        public static ILogic TRUE = new TrueLogic();
        public static ILogic FALSE = new FalseLogic();
        public static ILogic HAS_INFORMATION = new HasInformationTestLogic();
        public static ILogic HAS_SPOUSE = new HasSpouseTestLogic();
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

    class HasTraitLogic : ILogic
    {
        private string traitId;

        public HasTraitLogic(string trait)
        {
            traitId = trait;
        }

        public bool Evaluate(EventContext context)
        {
            return context.CurrentCharacter.HasTrait(traitId);
        }
    }

    class AnyChildLogic : ILogic
    {
        private ILogic requirements;

        public AnyChildLogic(ILogic requirements)
        {
            this.requirements = requirements;
        }

        public bool Evaluate(EventContext context)
        {
            //No children means its never true.
            if (context.CurrentCharacter.Children.Count == 0)
                return false;

            //TODO: start with a random child index.
            for(int i = 0; i < context.CurrentCharacter.Children.Count; ++i)
            {
                context.PushScope(context.CurrentCharacter.Children[i]);
                if(requirements.Evaluate(context))
                {
                    //We've found a child that meets the requirements.
                    context.PopScope();
                    return true;
                }
                context.PopScope();
            }
            return false;
        }
    }

    class AgeAtLeastLogic : ILogic
    {
        private int compareAge;

        public AgeAtLeastLogic(int compareAge)
        {
            this.compareAge = compareAge;
        }

        public bool Evaluate(EventContext context)
        {
            return context.CurrentCharacter.Age >= compareAge;
        }
    }

    class AgeAtMostLogic : ILogic
    {
        private int compareAge;

        public AgeAtMostLogic(int compareAge)
        {
            this.compareAge = compareAge;
        }

        public bool Evaluate(EventContext context)
        {
            return context.CurrentCharacter.Age <= compareAge;
        }
    }

    class PrestigeRankLogic : ILogic
    {
        private int prestigeRank;

        public PrestigeRankLogic(int prestigeRank)
        {
            this.prestigeRank = prestigeRank;
        }

        public bool Evaluate(EventContext context)
        {
            return context.CurrentCharacter.PrestigeRank <= prestigeRank;
        }
    }

    class HasTitleTypeLogic : ILogic
    {
        private string titleId;

        public HasTitleTypeLogic(string titleId)
        {
            this.titleId = titleId;
        }

        public bool Evaluate(EventContext context)
        {
            //TODO: Check title
            return false;
        }
    }

    class HasJobLogic : ILogic
    {
        private string jobId;

        public HasJobLogic(string jobId)
        {
            this.jobId = jobId;
        }

        public bool Evaluate(EventContext context)
        {
            return context.CurrentCharacter.HasJob(jobId);
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
