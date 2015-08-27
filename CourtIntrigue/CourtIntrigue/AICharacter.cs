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

            int num = Game.GetRandom(soloActions.Length + characterActions.Count);
            if (num < soloActions.Length)
            {
                return new ActionDescriptor(soloActions[num], this, null);
            }
            else
            {
                Character otherCharacter = characterActions.Keys.ElementAt(num - soloActions.Length);
                Action[] pairActions = characterActions[otherCharacter];
                return new ActionDescriptor(pairActions[Game.GetRandom(pairActions.Length)], this, otherCharacter);
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

            double bestValue = double.NegativeInfinity;
            List<int> bestOptions = new List<int>();
            for (int iOption = 0; iOption < options.Length; ++iOption)
            {
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
            int chosenIndex = bestOptions[Game.GetRandom(bestOptions.Count())];

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

        public override int OnChooseCharacter(Character[] characters)
        {
            return Game.GetRandom(characters.Length);
        }

        public override void OnLearnInformation(InformationInstance info)
        {
            //Don't care
        }

    }
}
