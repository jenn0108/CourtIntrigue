﻿using System;
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
        public static ILogic IS_MALE = new IsMaleTestLogic();
        public static ILogic IS_FEMALE = new IsFemaleTestLogic();
        public static ILogic IS_ADULT = new IsAdultTestLogic();
        public static ILogic IS_EARLYMORNING = new IsEarlyMorningTestLogic();

        public static double EPSILON = 1.0e-5;
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

    class IsEarlyMorningTestLogic : ILogic
    {
        public bool Evaluate(EventContext context, Game game)
        {
            return game.CurrentTime == game.CurrentDay;
        }
    }

    class HasSpouseTestLogic : ILogic
    {
        public bool Evaluate(EventContext context, Game game)
        {
            return context.CurrentCharacter.Spouse != null;
        }
    }

    class IsMaleTestLogic : ILogic
    {
        public bool Evaluate(EventContext context, Game game)
        {
            return context.CurrentCharacter.Gender == Gender.Male;
        }
    }

    class IsFemaleTestLogic : ILogic
    {
        public bool Evaluate(EventContext context, Game game)
        {
            return context.CurrentCharacter.Gender == Gender.Female;
        }
    }

    class IsAdultTestLogic : ILogic
    {
        public bool Evaluate(EventContext context, Game game)
        {
            return context.CurrentCharacter.Age >= 21;
        }
    }

    class IsChildOfLogic : ILogic
    {
        private string otherCharacter;

        public IsChildOfLogic(string otherCharacter)
        {
            this.otherCharacter = otherCharacter;
        }

        public bool Evaluate(EventContext context, Game game)
        {
            return (context.GetScopedObjectByName(otherCharacter) as Character).Children.Contains(context.CurrentCharacter);
        }
    }

    class IsCharacterLogic : ILogic
    {
        private string character;

        public IsCharacterLogic(string character)
        {
            this.character = character;
        }

        public bool Evaluate(EventContext context, Game game)
        {
            return (context.GetScopedObjectByName(character) as Character) == context.CurrentCharacter;
        }
    }

    class IsSpouseOfLogic : ILogic
    {
        private string otherCharacter;

        public IsSpouseOfLogic(string otherCharacter)
        {
            this.otherCharacter = otherCharacter;
        }


        public bool Evaluate(EventContext context, Game game)
        {
            return (context.GetScopedObjectByName(otherCharacter) as Character).Spouse == context.CurrentCharacter;
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

    class AllChildLogic : ILogic
    {
        private ILogic requirements;

        public AllChildLogic(ILogic requirements)
        {
            this.requirements = requirements;
        }

        public bool Evaluate(EventContext context, Game game)
        {
            //No children means its never true.  Used in CHILDREN_WELLDRESS_PRESTIGE_MOD.
            if (context.CurrentCharacter.Children.Count == 0)
                return false;
            
            for (int i = 0; i < context.CurrentCharacter.Children.Count; ++i)
            {
                context.PushScope(context.CurrentCharacter.Children[i]);
                if (!requirements.Evaluate(context, game))
                {
                    //We've found a child that doesn't the requirements.  This is a failure
                    context.PopScope();
                    return false;
                }
                context.PopScope();
            }
            return true;
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
            EventContext newContext = new EventContext(context.CurrentCharacter, computedParameters);
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
        private ICalculate testValue;
        public VariableGreaterThanLogic(string name, ICalculate testVal)
        {
            varName = name;
            testValue = testVal;
        }
        public bool Evaluate(EventContext context, Game game)
        {
            double myValue = XmlHelper.GetTestValue(context, game, varName);
            return myValue > testValue.Calculate(context, game);
        }
    }

    class VariableLessThanLogic : ILogic
    {
        private string varName;
        private ICalculate testValue;
        public VariableLessThanLogic(string name, ICalculate testVal)
        {
            varName = name;
            testValue = testVal;
        }
        public bool Evaluate(EventContext context, Game game)
        {
            double myValue = XmlHelper.GetTestValue(context, game, varName);
            return myValue < testValue.Calculate(context, game);
        }
    }

    class VariableEqualLogic : ILogic
    {
        private string varName;
        private ICalculate testValue;
        public VariableEqualLogic(string name, ICalculate testVal)
        {
            varName = name;
            testValue = testVal;
        }
        public bool Evaluate(EventContext context, Game game)
        {
            double myValue = XmlHelper.GetTestValue(context, game, varName);
            return Math.Abs(myValue - testValue.Calculate(context, game)) < Logic.EPSILON;
        }
    }

    class VariableGreaterOrEqualLogic : ILogic
    {
        private string varName;
        private ICalculate testValue;
        public VariableGreaterOrEqualLogic(string name, ICalculate testVal)
        {
            varName = name;
            testValue = testVal;
        }
        public bool Evaluate(EventContext context, Game game)
        {
            double myValue = XmlHelper.GetTestValue(context, game, varName);
            return myValue > testValue.Calculate(context, game) ||
                Math.Abs(myValue - testValue.Calculate(context, game)) < Logic.EPSILON;
        }
    }

    class VariableLessOrEqualLogic : ILogic
    {
        private string varName;
        private ICalculate testValue;
        public VariableLessOrEqualLogic(string name, ICalculate testVal)
        {
            varName = name;
            testValue = testVal;
        }
        public bool Evaluate(EventContext context, Game game)
        {
            double myValue = XmlHelper.GetTestValue(context, game, varName);
            return myValue < testValue.Calculate(context, game) ||
                Math.Abs(myValue - testValue.Calculate(context, game)) < Logic.EPSILON;
        }
    }

    class VariableNotEqualLogic : ILogic
    {
        private string varName;
        private ICalculate testValue;
        public VariableNotEqualLogic(string name, ICalculate testVal)
        {
            varName = name;
            testValue = testVal;
        }
        public bool Evaluate(EventContext context, Game game)
        {
            double myValue = XmlHelper.GetTestValue(context, game, varName);
            return Math.Abs(myValue - testValue.Calculate(context, game)) >= Logic.EPSILON;
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
