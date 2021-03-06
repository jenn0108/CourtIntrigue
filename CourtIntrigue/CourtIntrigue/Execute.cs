﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace CourtIntrigue
{
    static class Execute
    {
        public static IExecute NOOP = new NoOpExecute();
        public static IExecute DEBUG = new DebugExecute();
        public static IExecute CONTINUE_TURN = new ContinueTurnExecute();
    }

    class Weights
    {
        public Character Perspective { get; private set; }

        public Weights(Character perspective)
        {
            Perspective = perspective;
        }

        public double MeasureGold(Character currentCharacter, int gold)
        {
            if (Perspective == currentCharacter)
                return gold * 1.0;
            else
                return 0.0;
        }

        public double MeasurePrestige(Character currentCharacter, int change)
        {
            if (Perspective == currentCharacter)
                return change * 100.0;
            else
                return 0.0;
        }

        public double MeasureOpinion(Character charWithOpinion, Character opinionOf, OpinionModifier mod)
        {
            if (Perspective == opinionOf)
                return mod.Change * 1.0;
            else
                return 0.0;
        }

        public double MeasureJob(Character currentCharacter, Job job)
        {
            if (Perspective == currentCharacter)
                return 1000.0; //Jobs are great.  Really we should measure prestige modifiers since jobs can give you one.
            else
                return 0.0;
        }

        public double MeasureAllowEventSelection(Character currentCharacter)
        {
            if (Perspective == currentCharacter)
                return 0.0; // Nobody cares about getting their turn back.
            else
                return 0.0;
        }

        public double MeasureObserveInformation(InformationInstance info, Game game, Character observingCharacter)
        {
            // 1. Is info about the perspective character
            // then we like it as much as we like whatever the OnObserve of that information does.
            if (info.IsAbout(Perspective))
            {
                return info.EvaluateOnObserve(Perspective, Perspective, game);
            }
            else if (observingCharacter == Perspective)
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
            // Not caring about these right now because figuring out good weights is difficult
            // and having bad weights really messes with the AI (ie. Steward thinking Eavesdrop is
            // always better than getting taxes).
            return 0.0;
        }

        public double MeasureAfter(EventContext context, Game game)
        {
            //Do the changed variables belong to us?
            if(Perspective is AICharacter && context.HasChanges())
            {
                double result = 0.0;
                AICharacter me = Perspective as AICharacter;
                foreach(var pair in me.GetImportantCharacters())
                {
                    double change = EvaluateChangedModifiers(pair.Key, context, game);
                    if (change != 0.0)
                    {
                        if (pair.Value == Relationship.Self)
                            result += change;
                        else if (pair.Value == Relationship.Family)
                            result += change * 0.9;
                        else if (pair.Value == Relationship.Friend)
                            result += change * 0.5;
                        else if (pair.Value == Relationship.Rival)
                            result -= change;
                    }
                }
                return result;
            }

            return 0.0;
        }

        private double EvaluateChangedModifiers(Character character, EventContext context, Game game)
        {
            context.PushScope(character);
            List<PrestigeModifier> addedModifiers = new List<PrestigeModifier>();
            List<PrestigeModifier> removedModifiers = new List<PrestigeModifier>();
            game.GetChangedModifiers(context, addedModifiers, removedModifiers);

            double result = 0.0;
            if (addedModifiers.Count > 0 || removedModifiers.Count > 0)
            {
                foreach (var added in addedModifiers)
                {
                    result += added.DailyChange;
                }
                foreach (var removed in removedModifiers)
                {
                    result -= removed.DailyChange;
                }

            }
            context.PopScope();
            return result * 100.0 * 10.0;
        }

        // Returns true if the perspective is the current character.
        // This allows us to average other people's decisions and be smart
        // about what our own will be.
        public bool IsMyDecision(Character currentCharacter)
        {
            return Perspective == currentCharacter;
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

    class ContinueTurnExecute : IExecute
    {
        public void Execute(EventResults result, Game game, EventContext context)
        {
            result.Continue();
        }

        public double Evaluate(Game game, EventContext context, Weights weights)
        {
            //The AI needs to understand that it'll get to go again.
            return 0.0;
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
            EventContext newContext = new EventContext(context.CurrentCharacter, computedParameters);
            game.GetEventById(eventid).Execute(result, game, newContext);

            //We need to commit any changes that were performed on the new context to our old one.
            newContext.CommitTo(context);
        }

        public double Evaluate(Game game, EventContext context, Weights weights)
        {
            Dictionary<string, object> computedParameters = new Dictionary<string, object>();
            foreach (var pair in parameters)
            {
                computedParameters.Add(pair.Key, context.GetScopedObjectByName(pair.Value));
            }
            EventContext newContext = new EventContext(context.CurrentCharacter, computedParameters);
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
                informationInstance.ExecuteOnTold(context.CurrentCharacter, game, context.CurrentCharacter.CurrentRoom);
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
            Character chosen = context.CurrentCharacter.ChooseCharacter(game.FilterCharacters(requirements, context), operation, context, scopeName);
            context.PushScope(chosen, scopeName);
            operation.Execute(result, game, context);
            context.PopScope();
        }
        public double Evaluate(Game game, EventContext context, Weights weights)
        {
            //Get the weights for the character that will be choosing the character.
            Weights charWeights = context.CurrentCharacter.GetWeights(weights.Perspective);

            Character[] availableCharacters = game.FilterCharacters(requirements, context).ToArray();
            bool[] allowed = new bool[availableCharacters.Length];
            if (context.CurrentCharacter is AICharacter)
            {
                //AICharacter will only select characters from his important character's list.
                var important = (context.CurrentCharacter as AICharacter).GetImportantCharacters().Select(pair => pair.Key).ToList();
                for (int i = 0; i < availableCharacters.Length; ++i)
                {
                    allowed[i] = important.Contains(availableCharacters[i]);
                }
            }
            else
            {
                //The player could select anything.  We don't know anything about his preferences.
                for (int i = 0; i < availableCharacters.Length; ++i)
                {
                    allowed[i] = true;
                }
            }

            //Figure out which ones we think he will choose.
            int[] bestIndices = AIHelper.GetBest(availableCharacters, allowed, charWeights, (character, localWeights) =>
            {
                //We are only considering things theoretically: Don't make any changes to the context
                //we are given.  We want a new local context for each character so variable changes for
                //one character don't influence the others.
                EventContext localContext = new EventContext(context);
                localContext.PushScope(character, scopeName);
                double directResult = operation.Evaluate(game, localContext, localWeights);
                //We need to take into account any prestige modifiers because we are throwing away
                //the local context now.
                return directResult + localWeights.MeasureAfter(localContext, game);
            });
            //Evaluate each of those character using our weights
            double result = 0.0;
            foreach(var bestIndex in bestIndices)
            {
                Character best = availableCharacters[bestIndex];
                //We are only considering things theoretically: Don't make any changes to the context
                //we are given.  We want a new local context for each character so variable changes for
                //one character don't influence the others.
                EventContext localContext = new EventContext(context);
                localContext.PushScope(best, scopeName);
                result += operation.Evaluate(game, localContext, weights);
                //We need to take into account any prestige modifiers because we are throwing away
                //the local context now.
                result += weights.MeasureAfter(localContext, game);
            }
            return result / bestIndices.Length;
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
            foreach (var character in context.CurrentCharacter.CurrentRoom.GetCharacters(context.CurrentCharacter))
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
            foreach (var character in context.CurrentCharacter.CurrentRoom.GetCharacters(context.CurrentCharacter))
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

            context.SetVariable(context.CurrentCharacter, varName, newValue.Calculate(context, game));
        }

        public double Evaluate(Game game, EventContext context, Weights weights)
        {
            if (XmlHelper.IsSpecialName(varName))
                throw new InvalidOperationException("Cannot assign to special properties: " + varName);

            context.SetVariable(context.CurrentCharacter, varName, newValue.Calculate(context, game));
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

            context.OffsetVariable(context.CurrentCharacter, varName, offset.Calculate(context, game));
        }

        public double Evaluate(Game game, EventContext context, Weights weights)
        {
            if (XmlHelper.IsSpecialName(varName))
                throw new InvalidOperationException("Cannot assign to special properties: " + varName);

            context.OffsetVariable(context.CurrentCharacter, varName, offset.Calculate(context, game));
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

    class MoveToExecute : IExecute
    {
        private string roomId;
        public MoveToExecute(string id)
        {
            roomId = id;
        }

        public void Execute(EventResults result, Game game, EventContext context)
        {
            Room targetRoom = game.GetRoomById(roomId);
            if (context.CurrentCharacter.CurrentRoom.Priority < targetRoom.Priority)
                throw new Exception("Can only move from a higher priority room to a lower one.");
            context.CurrentCharacter.CurrentRoom = targetRoom;
        }

        public double Evaluate(Game game, EventContext context, Weights weights)
        {
            return 0.0;
        }
    }
}
