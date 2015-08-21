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
        public static ILogic HAS_SPOUSE = new HasSpouseTestLogic();
    }

    interface ILogic
    {
        bool Evaluate(EventContext context, Game game);
    }

    class AndLogic : ILogic
    {
        private ILogic[] subexpressions;
        public AndLogic(ILogic[] subexps)
        {
            subexpressions = subexps;
        }
        public bool Evaluate(EventContext context, Game game)
        {
            for(int i = 0; i < subexpressions.Length; ++i)
            {
                if (!subexpressions[i].Evaluate(context, game))
                    return false;
            }
            return true;
        }
    }

    class OrLogic : ILogic
    {
        private ILogic[] subexpressions;
        public OrLogic(ILogic[] subexps)
        {
            subexpressions = subexps;
        }
        public bool Evaluate(EventContext context, Game game)
        {
            for (int i = 0; i < subexpressions.Length; ++i)
            {
                if (subexpressions[i].Evaluate(context, game))
                    return true;
            }
            return false;
        }
    }

    class NotLogic : ILogic
    {
        private ILogic logic;
        public NotLogic(ILogic logic)
        {
            this.logic = logic;
        }
        public bool Evaluate(EventContext context, Game game)
        {
            return !logic.Evaluate(context, game);
        }
    }

    class ActionIdentifierTestLogic : ILogic
    {
        private string identifier;
        public ActionIdentifierTestLogic(string lookingFor)
        {
            identifier = lookingFor;
        }
        public bool Evaluate(EventContext context, Game game)
        {
            return context.Identifer == identifier;
        }
    }

    class HasInformationTestLogic : ILogic
    {
        private string about;
        private InformationType type;

        public HasInformationTestLogic(string about, InformationType type)
        {
            this.about = about;
            this.type = type;
        }

        public bool Evaluate(EventContext context, Game game)
        {
            Character aboutCharacter = context.GetScopedObjectByName(about) as Character;
            return context.CurrentCharacter.HasInformationAbout(aboutCharacter, type);
        }
    }

    class HasSpouseTestLogic : ILogic
    {
        public bool Evaluate(EventContext context, Game game)
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

        public bool Evaluate(EventContext context, Game game)
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

        public bool Evaluate(EventContext context, Game game)
        {
            //No children means its never true.
            if (context.CurrentCharacter.Children.Count == 0)
                return false;

            //TODO: start with a random child index.
            for(int i = 0; i < context.CurrentCharacter.Children.Count; ++i)
            {
                context.PushScope(context.CurrentCharacter.Children[i]);
                if(requirements.Evaluate(context, game))
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

        public bool Evaluate(EventContext context, Game game)
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

        public bool Evaluate(EventContext context, Game game)
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

        public bool Evaluate(EventContext context, Game game)
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

        public bool Evaluate(EventContext context, Game game)
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

        public bool Evaluate(EventContext context, Game game)
        {
            return context.CurrentCharacter.HasJob(jobId);
        }
    }

    class TestEventOptionsLogic : ILogic
    {
        private string eventId;
        private Dictionary<string, string> parameters;

        public TestEventOptionsLogic(string eventId, Dictionary<string, string> parameters)
        {
            this.eventId = eventId;
            this.parameters = parameters;
        }


        public bool Evaluate(EventContext context, Game game)
        {
            Event e = game.GetEventById(eventId);
            if (e == null)
                return false;

            Dictionary<string, object> computedParameters = new Dictionary<string, object>();
            foreach (var pair in parameters)
            {
                computedParameters.Add(pair.Key, context.GetScopedObjectByName(pair.Value));
            }
            EventContext newContext = new EventContext(null, context.CurrentCharacter, context.GetScopedObjectByName("ROOT") as Character, computedParameters);
            return e.GetAvailableOptions(newContext, game).Length > 0;
        }
    }

    class SetScopeLogic : ILogic
    {
        private string scopeName;
        private ILogic logic;
        public SetScopeLogic(string scopeName, ILogic logic)
        {
            this.scopeName = scopeName;
            this.logic = logic;
        }

        public bool Evaluate(EventContext context, Game game)
        {
            context.PushScope(context.GetScopedObjectByName(scopeName));
            bool result = logic.Evaluate(context, game);
            context.PopScope();
            return result;
        }
    }

    class JobRequirementsLogic : ILogic
    {
        private string jobId;
        public JobRequirementsLogic(string jobId)
        {
            this.jobId = jobId;
        }

        public bool Evaluate(EventContext context, Game game)
        {
            return game.GetJobById(jobId).CanPerformJob(context.CurrentCharacter, game);
        }
    }

    class VariableGreaterThanLogic : ILogic
    {
        private string varName;
        private string testValue;
        public VariableGreaterThanLogic(string name, string testVal)
        {
            varName = name;
            testValue = testVal;
        }
        public bool Evaluate(EventContext context, Game game)
        {
            return context.CurrentCharacter.GetVariable(varName) > XmlHelper.GetTestValue(context, game, testValue);
        }
    }

    class VariableLessThanLogic : ILogic
    {
        private string varName;
        private string testValue;
        public VariableLessThanLogic(string name, string testVal)
        {
            varName = name;
            testValue = testVal;
        }
        public bool Evaluate(EventContext context, Game game)
        {
            return context.CurrentCharacter.GetVariable(varName) < XmlHelper.GetTestValue(context, game, testValue);
        }
    }

    class VariableEqualLogic : ILogic
    {
        private string varName;
        private string testValue;
        public VariableEqualLogic(string name, string testVal)
        {
            varName = name;
            testValue = testVal;
        }
        public bool Evaluate(EventContext context, Game game)
        {
            return context.CurrentCharacter.GetVariable(varName) == XmlHelper.GetTestValue(context, game, testValue);
        }
    }

    class VariableGreaterOrEqualLogic : ILogic
    {
        private string varName;
        private string testValue;
        public VariableGreaterOrEqualLogic(string name, string testVal)
        {
            varName = name;
            testValue = testVal;
        }
        public bool Evaluate(EventContext context, Game game)
        {
            return context.CurrentCharacter.GetVariable(varName) >= XmlHelper.GetTestValue(context, game, testValue);
        }
    }

    class VariableLessOrEqualLogic : ILogic
    {
        private string varName;
        private string testValue;
        public VariableLessOrEqualLogic(string name, string testVal)
        {
            varName = name;
            testValue = testVal;
        }
        public bool Evaluate(EventContext context, Game game)
        {
            return context.CurrentCharacter.GetVariable(varName) <= XmlHelper.GetTestValue(context, game, testValue);
        }
    }

    class VariableNotEqualLogic : ILogic
    {
        private string varName;
        private string testValue;
        public VariableNotEqualLogic(string name, string testVal)
        {
            varName = name;
            testValue = testVal;
        }
        public bool Evaluate(EventContext context, Game game)
        {
            return context.CurrentCharacter.GetVariable(varName) != XmlHelper.GetTestValue(context, game, testValue);
        }
    }

    class HasGoldLogic : ILogic
    {
        private int gold;

        public HasGoldLogic(int gold)
        {
            this.gold = gold;
        }

        public bool Evaluate(EventContext context, Game game)
        {
            return context.CurrentCharacter.Money >= gold;
        }
    }

    class TrueLogic : ILogic
    {
        public bool Evaluate(EventContext context, Game game)
        {
            return true;
        }
    }

    class FalseLogic : ILogic
    {
        public bool Evaluate(EventContext context, Game game)
        {
            return false;
        }
    }
}
