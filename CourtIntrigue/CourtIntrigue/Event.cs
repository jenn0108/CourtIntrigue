using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CourtIntrigue
{
    class Event
    {
        public string Identifier { get; private set; }
        public string Description { get; private set; }
        public ILogic ActionRequirements { get; private set; }
        public EventOption[] Options { get; private set; }
        public IExecute DirectExecute { get; private set; }
        public Parameter[] Parameters { get; private set; }

        public Event(string id, string desc, ILogic requirements, IExecute dirExec, EventOption[] options, Parameter[] parameters)
        {
            Identifier = id;
            Description = desc;
            ActionRequirements = requirements;
            DirectExecute = dirExec;
            Options = options;
            Parameters = parameters;
        }

        public void Execute(EventResults result, Game game, EventContext context)
        {
            game.Log(context.CurrentCharacter.Fullname + ": Event:" + this.CreateActionDescription(context));

            //DirectExecute always happens if it is present.
            if (DirectExecute != null)
                DirectExecute.Execute(result, game, context);

            EventOption[] options = GetAvailableOptions(context, game);
            if (options.Length > 0)
            {
                int[] willpowerCost = new int[options.Length];
                for(int i = 0; i < options.Length; ++i)
                {
                    int maxCost = options[i].GetCostToTake(context.CurrentCharacter) - options[i].GetCostToAvoid(context.CurrentCharacter);
                    for(int j = 0; j < options.Length; ++j)
                    {
                        if (i == j)
                            continue;

                        maxCost = Math.Max(maxCost, options[j].GetCostToAvoid(context.CurrentCharacter));
                    }
                    willpowerCost[i] = maxCost;
                }
                //If there are options, the character must choose one.
                int chosenIndex = context.CurrentCharacter.ChooseOption(options, willpowerCost, context, this);
                EventOption chosen = options[chosenIndex];
                if(chosen != null && chosen.DirectExecute != null)
                {
                    context.CurrentCharacter.SpendWillpower(willpowerCost[chosenIndex]);
                    //Execute the option activity.
                    chosen.DirectExecute.Execute(result, game, context);
                }
            }
        }

        public bool EvaluateActionRequirements(ActionDescriptor actionDescriptor, Game game)
        {
            return ActionRequirements.Evaluate(new EventContext(actionDescriptor), game);
        }

        public double Evaluate(Game game, EventContext context, Weights weights)
        {
            double result = 0.0;

            //DirectExecute always happens if it is present.
            if (DirectExecute != null)
                result += DirectExecute.Evaluate(game, context, weights);

            EventOption[] options = GetAvailableOptions(context, game);
            if (options.Length > 0)
            {
                //Get the weights that will be used to decide the best option.
                Weights optionWeights = context.CurrentCharacter.GetWeights(weights.Perspective);

                //Evaluate each option from the perpsective of the character choosing.
                EventOption[] bestOptions = AIHelper.GetBest(options, optionWeights, (option, w) =>
                {
                    //We are only considering things theoretically: Don't make any changes to the context
                    //we are given.  We want a new local context for each option so variable changes for
                    //one option don't influence the others.
                    EventContext localContext = new EventContext(context);
                    double localResult = option.DirectExecute.Evaluate(game, localContext, w);
                    //We need to take into account any prestige modifiers because we are throwing away
                    //the local context now.
                    return localResult + w.MeasureAfter(localContext, game);
                });

                //The current character will choose one of the best (or so we think)
                //We need to evaluate the options from our perspective and average those values.
                foreach(var best in bestOptions)
                {
                    //We are only considering things theoretically: Don't make any changes to the context
                    //we are given.  We want a new local context for each option so variable changes for
                    //one option don't influence the others.
                    EventContext localContext = new EventContext(context);
                    result += best.DirectExecute.Evaluate(game, localContext, weights);
                    //We need to take into account any prestige modifiers because we are throwing away
                    //the local context now.
                    result += weights.MeasureAfter(localContext, game);
                }
                result /= bestOptions.Length;
            }
            return result;
        }

        public EventOption[] GetAvailableOptions(EventContext context, Game game)
        {
            List<EventOption> options = new List<EventOption>();
            foreach (var option in Options)
            {
                if (option.Requirements.Evaluate(context, game))
                {
                    options.Add(option);
                }
            }
            return options.ToArray();
        }

        public string CreateActionDescription(EventContext a)
        {
            return EventHelper.ReplaceStrings(Description, a);
        }

        public override string ToString()
        {
            return Identifier;
        }
    }

    struct Adversion
    {
        public string TraitId;
        public int Cost;

        public Adversion(string traitId, int cost)
        {
            TraitId = traitId;
            Cost = cost;
        }
    }

    struct Desire
    {
        public string TraitId;
        public int Cost;

        public Desire(string traitId, int cost)
        {
            TraitId = traitId;
            Cost = cost;
        }
    }

    class EventOption
    {
        public string Label { get; private set; }
        public IExecute DirectExecute { get; private set; }
        public ILogic Requirements { get; private set; }
        public Adversion[] Adversions { get; private set; }
        public Desire[] Desires { get; private set; }

        public EventOption(string label, IExecute dirExec, ILogic requirements, Adversion[] adversions, Desire[] desires)
        {
            Label = label;
            DirectExecute = dirExec;
            Requirements = requirements;
            Adversions = adversions;
            Desires = desires;
        }

        public int GetCostToAvoid(Character c)
        {
            int cost = 0;
            foreach(var d in Desires)
            {
                if (c.HasTrait(d.TraitId))
                    cost += d.Cost;
            }
            return cost;
        }

        public int GetCostToTake(Character c)
        {
            int cost = 0;
            foreach (var a in Adversions)
            {
                if (c.HasTrait(a.TraitId))
                    cost += a.Cost;
            }
            return cost;
        }
    }

    struct ObservableInformation
    {
        public InformationInstance Info;
        public int Chance;
        public Character Teller;
    }

    class EventResults
    {
        public bool TargetGetsTurn { get; private set; }
        public bool ContinueTurn { get; private set; }
        public double ObserveModifier { get; set; }

        private List<ObservableInformation> information = new List<ObservableInformation>();
        public IEnumerable<ObservableInformation> ObservableInformation
        {
            get { return information; }
        }
        public bool HasInformation
        {
            get { return information.Count > 0; }
        }

        public EventResults()
        {
            TargetGetsTurn = false;
            ContinueTurn = false;
            ObserveModifier = 1.0;
        }

        public void GiveTargetTurn()
        {
            TargetGetsTurn = true;
        }

        public void Continue()
        {
            ContinueTurn = true;
        }

        public void AddObservableInfo(InformationInstance ii, int chance, Character teller)
        {
            ObservableInformation info = new ObservableInformation()
            {
                Info = ii,
                Chance = chance,
                Teller = teller
            };
            information.Add(info);
        }
    }
}
