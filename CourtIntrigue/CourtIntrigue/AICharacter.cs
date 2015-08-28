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

            // Flatten the pair actions into a list of KeyValuePairs so we can easily index it.
            var flattenedPairActions = characterActions.ToList().SelectMany(p => p.Value, (p,action) => new KeyValuePair<Character,Action>(p.Key, action));

            int num = GetBestAction(soloActions, flattenedPairActions.ToList());
            if (num < soloActions.Length)
            {
                return new ActionDescriptor(soloActions[num], this, null);
            }
            else
            {
                KeyValuePair<Character, Action> pair = flattenedPairActions.ElementAt(num - soloActions.Length);
                return new ActionDescriptor(pair.Value, this, pair.Key);
            }

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

        private int GetBestAction(Action[] soloActions, List<KeyValuePair<Character, Action>> pairActions)
        {
            // For each action, evaluate the possible events and average their outcomes.
            double bestValue = double.NegativeInfinity;
            List<int> bestActions = new List<int>();
            EventContext soloContext = new EventContext(this);
            // First go through all the solo actions.
            for (int iAction = 0; iAction < soloActions.Length; ++iAction)
            {
                double actionValue = 0.0;
                // Average the value of each event that can be triggered by the action.
                foreach (string eventId in soloActions[iAction].Events) {
                    Event ev = Game.GetEventById(eventId);
                    if (ev.ActionRequirements.Evaluate(soloContext, Game))
                    {
                        actionValue += Game.GetEventById(eventId).Evaluate(Game, soloContext, GetWeights()) / soloActions[iAction].Events.Count();
                    }
                }
                if (actionValue > bestValue)
                {
                    bestActions.Clear();
                    bestActions.Add(iAction);
                    bestValue = actionValue;
                }
                else if (actionValue == bestValue)
                {
                    bestActions.Add(iAction);
                }
            }

            for (int iAction = 0; iAction < pairActions.Count(); ++iAction)
            {
                Character character = pairActions[iAction].Key;
                Action action = pairActions[iAction].Value;
                // Ew this line is horrible, maybe we should make an exception and create an event context
                // with a target directly here...
                EventContext pairContext = new EventContext(new ActionDescriptor(action, this, character));

                double actionValue = 0.0;
                // Average the value of each event that can be triggered by the action.
                // First figure out which events we can actually do and only average those.
                foreach (string eventId in action.Events)
                {
                    Event ev = Game.GetEventById(eventId);
                    if (ev.ActionRequirements.Evaluate(pairContext, Game)) {
                        actionValue += Game.GetEventById(eventId).Evaluate(Game, pairContext, GetWeights()) / action.Events.Count();
                    }
                }
                if (actionValue > bestValue)
                {
                    bestActions.Clear();
                    bestActions.Add(iAction + soloActions.Length);
                    bestValue = actionValue;
                }
                else if (actionValue == bestValue)
                {
                    bestActions.Add(iAction + soloActions.Length);
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
