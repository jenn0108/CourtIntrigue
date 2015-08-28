using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CourtIntrigue
{
    class AICharacter : Character
    {
        public AICharacter(string name, int birthdate, Dynasty dynasty, int money, Game game, Gender gender) : base(name, birthdate, dynasty, money, game, gender)
        {
        }

        public override ActionDescriptor OnTick(Action[] soloActions, Dictionary<Character, Action[]> characterActions)
        {
            CharacterLog("In room with: " + string.Join(", ", characterActions.Select(p => p.Key.Name)));
            
            //We can describe an action using its action descriptor.
            List<ActionDescriptor> allActions = new List<ActionDescriptor>();

            //Solo actions are easy.  Each descriptor is just an action plus this character.
            allActions.AddRange(soloActions.Select(action => new ActionDescriptor(action, this, null)));
            
            //Pair actions require another character in addition to this character.
            foreach(var pair in characterActions)
            {
                var targetCharacter = pair.Key;
                //Each action for this character produces another ActionDescriptor using the action, this character and the target.
                allActions.AddRange(pair.Value.Select(action => new ActionDescriptor(action, this, targetCharacter)));
            }

            //Go find the best one.
            return GetBestAction(allActions);
        }

        public override int OnBeginDay(Room[] rooms)
        {
            return Game.GetRandom(rooms.Length);
        }

        public override int OnChooseOption(EventOption[] options, int[] willpowerCost, EventContext context, Event e)
        {
            int allowedCount = willpowerCost.Count(cost => cost <= WillPower);

            if(allowedCount < 1)
            {
                //Events must have at least one willpower free option or else characters may be locked out
                //of doing anything at all.
                throw new EventIncorrectException("Events must have at least one willpower free option");
            }

            if (options.Length == 1)
                return 0;

            int chosenIndex = GetBestOption(options, context);

            while(willpowerCost[chosenIndex] > WillPower)
            {
                chosenIndex = Game.GetRandom(options.Length);
            }
            EventOption chosen = options[chosenIndex];
            CharacterLog("Choosing " + EventHelper.ReplaceStrings(chosen.Label, context));
            return chosenIndex;
        }

        public override int OnChooseInformation(InformationInstance[] informations)
        {
            return Game.GetRandom(informations.Length);
        }

        public override int OnChooseCharacter(Character[] characters, IExecute operation, EventContext context, string chosenName)
        {
            return GetBestCharacter(characters, operation, context, chosenName);
        }

        public override void OnLearnInformation(InformationInstance info)
        {
            //Don't care
        }

        private int GetBestCharacter(Character[] characters, IExecute operation, EventContext context, string chosenName)
        {
            double bestValue = double.NegativeInfinity;
            List<int> bestCharacters = new List<int>();
            for (int iCharacter = 0; iCharacter < characters.Length; ++iCharacter)
            {
                context.PushScope(characters[iCharacter], chosenName);
                double characterValue = operation.Evaluate(Game, context, GetWeights());
                context.PopScope();
                if (characterValue > bestValue)
                {
                    bestCharacters.Clear();
                    bestCharacters.Add(iCharacter);
                    bestValue = characterValue;
                }
                else if (characterValue == bestValue)
                {
                    bestCharacters.Add(iCharacter);
                }
            }
            return bestCharacters[Game.GetRandom(bestCharacters.Count())];
        }

        private ActionDescriptor GetBestAction(IEnumerable<ActionDescriptor> actionDescriptors)
        {
            // For each action, evaluate the possible events and average their outcomes.
            double bestValue = double.NegativeInfinity;
            List<ActionDescriptor> bestActions = new List<ActionDescriptor>();

            //Consider each possibility in turn.
            foreach(var actionDescriptor in actionDescriptors)
            {
                EventContext context = new EventContext(actionDescriptor);

                double actionValue = 0.0;
                // Average the value of each event that can be triggered by the action.
                foreach (string eventId in actionDescriptor.Action.Events)
                {
                    Event ev = Game.GetEventById(eventId);
                    if (ev.ActionRequirements.Evaluate(context, Game))
                    {
                        actionValue += ev.Evaluate(Game, context, GetWeights()) / actionDescriptor.Action.Events.Count();
                    }
                }

                //Are we the best?
                if (actionValue > bestValue)
                {
                    bestActions.Clear();
                    bestActions.Add(actionDescriptor);
                    bestValue = actionValue;
                }
                else if (actionValue == bestValue)
                {
                    bestActions.Add(actionDescriptor);
                }
            }

            return bestActions[Game.GetRandom(bestActions.Count())];
        }

        private int GetBestOption(EventOption[] options, EventContext context)
        {
            double bestValue = double.NegativeInfinity;
            List<int> bestOptions = new List<int>();
            for (int iOption = 0; iOption < options.Length; ++iOption)
            {
                // Don't need to look at requirements because we're only give the options we can choose.
                double optionValue = options[iOption].DirectExecute.Evaluate(Game, context, GetWeights());
                if (optionValue > bestValue)
                {
                    bestOptions.Clear();
                    bestOptions.Add(iOption);
                    bestValue = optionValue;
                }
                else if (optionValue == bestValue)
                {
                    bestOptions.Add(iOption);
                }
            }
            return bestOptions[Game.GetRandom(bestOptions.Count())];
        }

    }
}
