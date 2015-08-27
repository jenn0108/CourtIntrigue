using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace CourtIntrigue
{
    class Execute
    {
        public static IExecute NOOP = new NoOpExecute();
        public static IExecute DEBUG = new DebugExecute();
    }

    class Weights
    {
        private Character perspective;

        public Weights(Character perspective)
        {
            this.perspective = perspective;
        }

        public double MeasureGold(Character currentCharacter, int gold)
        {
            if (perspective == currentCharacter)
                return gold * 1.0;
            else
                return 0.0;
        }

        public double MeasurePrestige(Character currentCharacter, int change)
        {
            if (perspective == currentCharacter)
                return change * 100.0;
            else
                return 0.0;
        }

        public double MeasureOpinion(Character charWithOpinion, Character opinionOf, OpinionModifier mod)
        {
            if (perspective == opinionOf)
                return mod.Change * 1.0;
            else
                return 0.0;
        }

        public double MeasureJob(Character currentCharacter, Job job)
        {
            if (perspective == currentCharacter)
                return 1000.0; //Jobs are great.  Really we should measure prestige modifiers since jobs can give you one.
            else
                return 0.0;
        }

        public double MeasureAllowEventSelection(Character currentCharacter)
        {
            if (perspective == currentCharacter)
                return 0.0; // Nobody cares about getting their turn back.
            else
                return 0.0;
        }

        public double MeasureObserveInformation(InformationInstance info, Game game, Character observingCharacter)
        {
            // 1. Is info about the perspective character
            // then we like it as much as we like whatever the OnObserve of that information does.
            if (info.IsAbout(perspective))
            {
                return info.EvaluateOnObserve(perspective, game);
            }
            else if (observingCharacter == perspective)
            {
                return 100.0; // It's good to know things about people.
            }
            else
            {
                return 0.0; // Don't care about other people learning random information
            }
        }

        public double MeasureObserveChance(Character currentCharacter, double multiplier)
        {
            if (perspective == currentCharacter)
                return multiplier * 10.0; // It's better when the observe chance is getting higher
            else
                return 0.0;
        }
    }

    interface IExecute
    {
        void Execute(EventResults result, Game game, EventContext context);
        double Evaluate(Game game, EventContext context, Weights weights);
    }

    class NoOpExecute : IExecute
    {
        public void Execute(EventResults result, Game game, EventContext context)
        {

        }

        public double Evaluate(Game game, EventContext context, Weights weights)
        {
            return 0.0;
        }
    }


    class DebugExecute : IExecute
    {
        public void Execute(EventResults result, Game game, EventContext context)
        {
            Debugger.Break();
        }

        public double Evaluate(Game game, EventContext context, Weights weights)
        {
            return 0.0;
        }
    }

    class SequenceExecute : IExecute
    {
        private IExecute[] instructions;
        public SequenceExecute(IExecute[] instructions)
        {
            this.instructions = instructions;
        }

        public void Execute(EventResults result, Game game, EventContext context)
        {
            for(int i = 0; i < instructions.Length; ++i)
            {
                instructions[i].Execute(result, game, context);
            }
        }

        public double Evaluate(Game game, EventContext context, Weights weights)
        {
            double result = 0.0;
            for (int i = 0; i < instructions.Length; ++i)
            {
                result += instructions[i].Evaluate(game, context, weights);
            }
            return result;
        }
    }

    class AllowEventSelectionExecute : IExecute
    {
        public void Execute(EventResults result, Game game, EventContext context)
        {
            result.GiveTargetTurn();
        }

        public double Evaluate(Game game, EventContext context, Weights weights)
        {
            return weights.MeasureAllowEventSelection(context.CurrentCharacter);
        }
    }

    class TriggerEventExecute : IExecute
    {
        private string eventid;
        private Dictionary<string, string> parameters;
        public TriggerEventExecute(string id, Dictionary<string, string> parameters)
        {
            eventid = id;
            this.parameters = parameters;
        }

        public void Execute(EventResults result, Game game, EventContext context)
        {
            Dictionary<string, object> computedParameters = new Dictionary<string, object>();
            foreach (var pair in parameters)
            {
                computedParameters.Add(pair.Key, context.GetScopedObjectByName(pair.Value));
            }
            EventContext newContext = new EventContext(context.CurrentCharacter, context.GetScopedObjectByName("ROOT") as Character, computedParameters);
            game.GetEventById(eventid).Execute(result, game, newContext);
        }

        public double Evaluate(Game game, EventContext context, Weights weights)
        {
            Dictionary<string, object> computedParameters = new Dictionary<string, object>();
            foreach (var pair in parameters)
            {
                computedParameters.Add(pair.Key, context.GetScopedObjectByName(pair.Value));
            }
            EventContext newContext = new EventContext(context.CurrentCharacter, context.GetScopedObjectByName("ROOT") as Character, computedParameters);
            return game.GetEventById(eventid).Evaluate(game, newContext, weights);
        }
    }

    class ObserveInformationExecute : IExecute
    {
        private string informationId;
        private Dictionary<string, string> parameters;
        private int chance;
        public ObserveInformationExecute(string informationId, Dictionary<string,string> parameters, int chance)
        {
            this.informationId = informationId;
            this.parameters = parameters;
            this.chance = chance;
        }

        public void Execute(EventResults result, Game game, EventContext context)
        {
            Dictionary<string, object> computedParameters = new Dictionary<string, object>();
            foreach (var pair in parameters)
            {
                computedParameters.Add(pair.Key, context.GetScopedObjectByName(pair.Value));
            }
            result.AddObservableInfo(new InformationInstance(game.GetInformationById(informationId), computedParameters, game.CurrentTime), chance, null);
        }

        public double Evaluate(Game game, EventContext context, Weights weights)
        {
            Dictionary<string, object> computedParameters = new Dictionary<string, object>();
            foreach (var pair in parameters)
            {
                computedParameters.Add(pair.Key, context.GetScopedObjectByName(pair.Value));
            }
            InformationInstance info = new InformationInstance(game.GetInformationById(informationId), computedParameters, game.CurrentTime);
            return weights.MeasureObserveInformation(info, game, context.CurrentCharacter);
        }
    }

    class TellInformationExecute : IExecute
    {
        private string about;
        private InformationType type;
        private IExecute operation;
        private int overhearChance;
        public TellInformationExecute(IExecute operation, int overhearChance, string about, InformationType type)
        {
            this.about = about;
            this.type = type;
            this.operation = operation;
            this.overhearChance = overhearChance;
        }
        public void Execute(EventResults result, Game game, EventContext context)
        {
            Character tellingCharacter = context.GetScopedObjectByName("ROOT") as Character;
            InformationInstance informationInstance = tellingCharacter.ChooseInformationAbout(type, context.GetScopedObjectByName(about) as Character);
            bool isNewInformation = context.CurrentCharacter.AddInformation(informationInstance);
            context.PushScope(informationInstance);
            operation.Execute(result, game, context);
            context.PopScope();

            result.AddObservableInfo(informationInstance, overhearChance, tellingCharacter);

            if (isNewInformation)
            {
                game.Log(context.CurrentCharacter.Name + " learned an information.");
                informationInstance.ExecuteOnTold(context.CurrentCharacter, tellingCharacter, game, context.Room);
            }
        }

        public double Evaluate(Game game, EventContext context, Weights weights)
        {
            // TODO: We need some way to be intelligent in ChooseInformationAbout for AI character before implementing this.
            return 0.0;
        }
    }

    class ChooseCharacterExecute : IExecute
    {
        private string scopeName;
        private IExecute operation;
        private ILogic requirements;
        public ChooseCharacterExecute(string scopeName, IExecute operation, ILogic requirements)
        {
            this.scopeName = scopeName;
            this.operation = operation;
            this.requirements = requirements;
        }

        public void Execute(EventResults result, Game game, EventContext context)
        {
            Character chosen = context.CurrentCharacter.ChooseCharacter(requirements, context);
            context.PushScope(chosen, scopeName);
            operation.Execute(result, game, context);
            context.PopScope();
        }
        public double Evaluate(Game game, EventContext context, Weights weights)
        {
            // TODO: We need some way to be intelligent in ChooseCharacter for AI character before implementing this.
            return 0.0;
        }
    }

    class AnyChildExecute : IExecute
    {
        private IExecute operation;
        private ILogic requirements;
        public AnyChildExecute(IExecute operation, ILogic requirements)
        {
            this.operation = operation;
            this.requirements = requirements;
        }

        public void Execute(EventResults result, Game game, EventContext context)
        {
            //TODO: Start with random index.
            for(int i = 0; i < context.CurrentCharacter.Children.Count; ++i)
            {
                Character character = context.CurrentCharacter.Children[i];
                context.PushScope(character);
                if (requirements.Evaluate(context, game))
                {
                    operation.Execute(result, game, context);

                    //AnyChild stops after a single interation.
                    context.PopScope();
                    return;
                }
                context.PopScope();
            }
        }

        public double Evaluate(Game game, EventContext context, Weights weights)
        {
            //TODO: Start with random index.
            // TODO: Should we be doing some average here instead of just stopping
            // at the first one that works?
            for (int i = 0; i < context.CurrentCharacter.Children.Count; ++i)
            {
                Character character = context.CurrentCharacter.Children[i];
                context.PushScope(character);
                if (requirements.Evaluate(context, game))
                {
                    double result = operation.Evaluate(game, context, weights);

                    //AnyChild stops after a single interation.
                    context.PopScope();
                    return result;
                }
                context.PopScope();
            }
            return 0.0;
        }
    }

    class EveryoneInRoomExecute : IExecute
    {
        private IExecute operation;
        private ILogic requirements;
        public EveryoneInRoomExecute(IExecute operation, ILogic requirements)
        {
            this.operation = operation;
            this.requirements = requirements;
        }

        public void Execute(EventResults result, Game game, EventContext context)
        {
            foreach (var character in context.Room.GetCharacters(context.CurrentCharacter))
            {
                context.PushScope(character);
                if (requirements.Evaluate(context, game))
                {
                    operation.Execute(result, game, context);
                }
                context.PopScope();
            }
        }

        public double Evaluate(Game game, EventContext context, Weights weights)
        {
            double result = 0.0;
            foreach (var character in context.Room.GetCharacters(context.CurrentCharacter))
            {
                context.PushScope(character);
                if (requirements.Evaluate(context, game))
                {
                    result += operation.Evaluate(game, context, weights);
                }
                context.PopScope();
            }
            return result;
        }
    }

    class SetScopeExecute : IExecute
    {
        private string scopeName;
        private IExecute operation;
        public SetScopeExecute(string scopeName, IExecute operation)
        {
            this.scopeName = scopeName;
            this.operation = operation;
        }

        public void Execute(EventResults result, Game game, EventContext context)
        {
            context.PushScope(context.GetScopedObjectByName(scopeName));
            operation.Execute(result, game, context);
            context.PopScope();
        }

        public double Evaluate(Game game, EventContext context, Weights weights)
        {
            context.PushScope(context.GetScopedObjectByName(scopeName));
            double result = operation.Evaluate(game, context, weights);
            context.PopScope();
            return result;
        }
    }

    class IfExecute : IExecute
    {
        private ILogic requirements;
        private IExecute thenExecute;
        private IExecute elseExecute;
        public IfExecute(ILogic requirements, IExecute thenExecute, IExecute elseExecute)
        {
            this.requirements = requirements;
            this.thenExecute = thenExecute;
            this.elseExecute = elseExecute;
        }

        public void Execute(EventResults result, Game game, EventContext context)
        {
            if(requirements.Evaluate(context, game))
            {
                thenExecute.Execute(result, game, context);
            }
            else
            {
                elseExecute.Execute(result, game, context);
            }
        }

        public double Evaluate(Game game, EventContext context, Weights weights)
        {
            if (requirements.Evaluate(context, game))
            {
                return thenExecute.Evaluate(game, context, weights);
            }
            else
            {
                return elseExecute.Evaluate(game, context, weights);
            }
        }
    }

    class GiveJobExecute : IExecute
    {
        private string jobId;
        public GiveJobExecute(string jobId)
        {
            this.jobId = jobId;
        }

        public void Execute(EventResults result, Game game, EventContext context)
        {
            game.GiveJobTo(game.GetJobById(jobId), context.CurrentCharacter);
        }

        public double Evaluate(Game game, EventContext context, Weights weights)
        {
            return weights.MeasureJob(context.CurrentCharacter, game.GetJobById(jobId));
        }
    }

    class ApplyOpinionModifierExecute : IExecute
    {
        private string identifier;
        private string character;
        public ApplyOpinionModifierExecute(string identifier, string character)
        {
            this.identifier = identifier;
            this.character = character;
        }

        public void Execute(EventResults result, Game game, EventContext context)
        {
            OpinionModifierInstance mod = game.CreateOpinionModifier(identifier, context.GetScopedObjectByName(character) as Character);
            context.CurrentCharacter.AddOpinionModifier(mod);
        }

        public double Evaluate(Game game, EventContext context, Weights weights)
        {
            return weights.MeasureOpinion(context.CurrentCharacter, context.GetScopedObjectByName(character) as Character, game.GetOpinionModifier(identifier));
        }
    }

    class PrestigeChangeExecute : IExecute
    {
        private int change;
        public PrestigeChangeExecute(int change)
        {
            this.change = change;
        }

        public void Execute(EventResults result, Game game, EventContext context)
        {
            context.CurrentCharacter.PrestigeChange(change);
        }

        public double Evaluate(Game game, EventContext context, Weights weights)
        {
            return weights.MeasurePrestige(context.CurrentCharacter, change);
        }
    }

    class SpendGoldExecute : IExecute
    {
        private int gold;
        public SpendGoldExecute(int gold)
        {
            this.gold = gold;
        }

        public void Execute(EventResults result, Game game, EventContext context)
        {
            context.CurrentCharacter.SpendGold(gold);
        }

        public double Evaluate(Game game, EventContext context, Weights weights)
        {
            return weights.MeasureGold(context.CurrentCharacter, -gold);
        }
    }

    class GetGoldExecute : IExecute
    {
        private int gold;
        public GetGoldExecute(int gold)
        {
            this.gold = gold;
        }

        public void Execute(EventResults result, Game game, EventContext context)
        {
            context.CurrentCharacter.GetGold(gold);
        }

        public double Evaluate(Game game, EventContext context, Weights weights)
        {
            return weights.MeasureGold(context.CurrentCharacter, gold);
        }
    }

    class SetVariableExecute : IExecute
    {
        private string varName;
        private ICalculate newValue;
        public SetVariableExecute(string varName, ICalculate newValue)
        {
            this.varName = varName;
            this.newValue = newValue;
        }

        public void Execute(EventResults result, Game game, EventContext context)
        {
            if (XmlHelper.IsSpecialName(varName))
                throw new InvalidOperationException("Cannot assign to special properties: " + varName);

            context.CurrentCharacter.SetVariable(varName, newValue.Calculate(context, game));
        }

        public double Evaluate(Game game, EventContext context, Weights weights)
        {
            return 0.0;
        }
    }

    class OffsetVariableExecute : IExecute
    {
        private string varName;
        private ICalculate offset;
        public OffsetVariableExecute(string varName, ICalculate offset)
        {
            this.varName = varName;
            this.offset = offset;
        }

        public void Execute(EventResults result, Game game, EventContext context)
        {
            if (XmlHelper.IsSpecialName(varName))
                throw new InvalidOperationException("Cannot assign to special properties: " + varName);

            context.CurrentCharacter.SetVariable(varName, context.CurrentCharacter.GetVariable(varName) + offset.Calculate(context, game));
        }

        public double Evaluate(Game game, EventContext context, Weights weights)
        {
            return 0.0;
        }
    }

    class MultiplyObserveChanceExecute : IExecute
    {
        private double multiplier;
        public MultiplyObserveChanceExecute(double multiplier)
        {
            this.multiplier = multiplier;
        }

        public void Execute(EventResults result, Game game, EventContext context)
        {
            context.CurrentCharacter.MultiplyObserveModifier(multiplier);
        }

        public double Evaluate(Game game, EventContext context, Weights weights)
        {
            return weights.MeasureObserveChance(context.CurrentCharacter, multiplier);
        }
    }

    class RandomExecute : IExecute
    {
        private IExecute[] outcomes;
        private double[] chances;

        public RandomExecute(IExecute[] outcomes, double[] chances)
        {
            this.outcomes = outcomes;
            this.chances = chances;
        }

        public void Execute(EventResults result, Game game, EventContext context)
        {
            const int RandomPrecision = 1000000;

            int val = game.GetRandom(RandomPrecision);
            for(int i = 0; i < outcomes.Length; ++i)
            {
                int myCutoff = (int)(chances[i] * RandomPrecision);
                if (val < myCutoff)
                {
                    outcomes[i].Execute(result, game, context);
                    return;
                }

                val -= myCutoff;
            }
        }

        public double Evaluate(Game game, EventContext context, Weights weights)
        {
            double result = 0.0;
            for (int i = 0; i < outcomes.Length; ++i)
            {
                result += outcomes[i].Evaluate(game, context, weights) * chances[i];
            }
            return result;
        }
    }

    class CreateChildExecute : IExecute
    {

        public void Execute(EventResults result, Game game, EventContext context)
        {
            game.CreateChild(context.CurrentCharacter, context.CurrentCharacter.Spouse);
        }
        public double Evaluate(Game game, EventContext context, Weights weights)
        {
            return 0.0;
        }
    }
}
