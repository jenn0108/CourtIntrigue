using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CourtIntrigue
{
    enum Relationship
    {
        Family,
        Rival,
        Powerful,
        Friend
    }

    class AICharacter : Character
    {
        private Dictionary<Character, Relationship> interestingCharacters = new Dictionary<Character, Relationship>();

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
            if(interestingCharacters.Count == 0)
            {
                //You are your own family.
                interestingCharacters.Add(this, Relationship.Family);
                //Add family
                if (Spouse != null)
                    interestingCharacters.Add(Spouse, Relationship.Family);
                if (Father != null)
                    interestingCharacters.Add(Father, Relationship.Family);
                if (Mother != null)
                    interestingCharacters.Add(Mother, Relationship.Family);
                foreach (var child in Children)
                {
                    interestingCharacters.Add(child, Relationship.Family);
                }
                bool foundRival = false;
                bool foundFriend = false;
                foreach(var character in Game.AllCharacters)
                {
                    //Don't add yourself
                    if (character == this)
                        continue;

                    //Adults should only rival/friend adults, likewise with children.
                    if (character.Age < 20 && Age >= 20)
                        continue;
                    if (character.Age >= 20 && Age < 20)
                        continue;

                    if (character.HasJob("KING_JOB") && !interestingCharacters.ContainsKey(character))
                        interestingCharacters.Add(character, Relationship.Powerful);

                    //Woman and men are separate.
                    if (character.Gender != Gender)
                        continue;

                    if (GetOpinionOf(character) > 0 && !foundFriend && !interestingCharacters.ContainsKey(character))
                    {
                        interestingCharacters.Add(character, Relationship.Friend);
                        foundFriend = true;
                    }
                    else if (GetOpinionOf(character) < 0 && !foundRival && !interestingCharacters.ContainsKey(character))
                    {
                        interestingCharacters.Add(character, Relationship.Rival);
                        foundRival = true;
                    }
                }

                CharacterLog("Interesting Characters:" + string.Join(", ", interestingCharacters.Keys));
            }
            return Game.GetRandom(rooms.Length);
        }

        public IEnumerable<KeyValuePair<Character, Relationship>> GetImportantCharacters()
        {
            return interestingCharacters;
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
            
            //Only options we have enough willpower for are valid choices
            bool[] allowed = new bool[options.Length];
            for (int i = 0; i < options.Length; ++i)
            {
                allowed[i] = willpowerCost[i] <= WillPower;
            }
            int[] bestIndexes = AIHelper.GetBest(options, allowed, GetWeights(this), (option, weights)=>
            {
                //We are only considering things theoretically: Don't make any changes to the context
                //we are given.  We want a new local context for each option so variable changes for
                //one option don't influence the others.
                EventContext localContext = new EventContext(context);
                double localResult = option.DirectExecute.Evaluate(Game, localContext, weights);
                //We need to take into account any prestige modifiers because we are throwing away
                //the local context now.
                return localResult + weights.MeasureAfter(localContext, Game);
            });

            int chosenIndex = bestIndexes[Game.GetRandom(bestIndexes.Length)];
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
            bool[] allowed = new bool[characters.Length];
            for(int i = 0; i < characters.Length; ++i)
            {
                allowed[i] = interestingCharacters.ContainsKey(characters[i]);
            }
            return GetBestCharacter(characters, allowed, operation, context, chosenName);
        }

        public override void OnLearnInformation(InformationInstance info)
        {
            //Don't care
        }

        private int GetBestCharacter(Character[] characters, bool[] allowed, IExecute operation, EventContext context, string chosenName)
        {
            int[] bestCharacters = AIHelper.GetBest(characters, allowed, GetWeights(this), (character, weights) =>
            {
                //We are only considering things theoretically: Don't make any changes to the context
                //we are given.  We want a new local context for each character so variable changes for
                //one character don't influence the others.
                EventContext localContext = new EventContext(context);
                localContext.PushScope(character, chosenName);
                double result =  operation.Evaluate(Game, localContext, weights);
                //We need to take into account any prestige modifiers because we are throwing away
                //the local context now.
                return result + weights.MeasureAfter(localContext, Game);
            });

            if (bestCharacters.Length == 0)
                return Game.GetRandom(characters.Length);

            return bestCharacters[Game.GetRandom(bestCharacters.Length)];
        }

        private ActionDescriptor GetBestAction(IEnumerable<ActionDescriptor> actionDescriptors)
        {
            ActionDescriptor[] bestActions = AIHelper.GetBest(actionDescriptors, GetWeights(this), (actionDescriptor, weights) =>
            {
                double actionValue = 0.0;
                // Average the value of each event that can be triggered by the action.
                foreach (string eventId in actionDescriptor.Action.Events)
                {
                    //We want a new local context for each event so variable changes for
                    //one event don't influence the others.
                    EventContext context = new EventContext(actionDescriptor);
                    Event ev = Game.GetEventById(eventId);
                    if (ev.ActionRequirements.Evaluate(context, Game))
                    {
                        actionValue += ev.Evaluate(Game, context, weights) / actionDescriptor.Action.Events.Count();
                    }
                }
                return actionValue;
            });

            return bestActions[Game.GetRandom(bestActions.Count())];
        }

    }

    static class AIHelper
    {
        public static Item[] GetBest<Item>(IEnumerable<Item> items, Weights weights, Func<Item, Weights, double> eval)
        {
            // For each action, evaluate the possible events and average their outcomes.
            double bestValue = double.NegativeInfinity;
            List<Item> bestItems = new List<Item>();

            //Consider each possibility in turn.
            foreach (var item in items)
            {
                double itemValue = eval(item, weights);

                //Are we the best?
                if (itemValue > bestValue)
                {
                    bestItems.Clear();
                    bestItems.Add(item);
                    bestValue = itemValue;
                }
                else if (itemValue == bestValue)
                {
                    bestItems.Add(item);
                }
            }

            return bestItems.ToArray();
        }

        public static int[] GetBest<Item>(Item[] items, bool[] allowed, Weights weights, Func<Item, Weights, double> eval)
        {
            // For each action, evaluate the possible events and average their outcomes.
            double bestValue = double.NegativeInfinity;
            List<int> bestItems = new List<int>();

            //Consider each possibility in turn.
            for (int iItem = 0; iItem < items.Length; ++iItem)
            {
                double itemValue = eval(items[iItem], weights);

                //Are we the best?
                if (itemValue > bestValue)
                {
                    bestItems.Clear();
                    bestItems.Add(iItem);
                    bestValue = itemValue;
                }
                else if (itemValue == bestValue)
                {
                    bestItems.Add(iItem);
                }
            }

            return bestItems.ToArray();
        }
    }
}
