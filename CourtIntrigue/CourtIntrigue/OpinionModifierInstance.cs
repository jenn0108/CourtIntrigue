using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CourtIntrigue
{
    class OpinionModifierInstance
    {
        public Character Target { get; private set; }

        private OpinionModifier modifier;
        private int time;

        public string Description {  get { return modifier.Description; } }

        public OpinionModifierInstance(Character target, OpinionModifier modifier, int time)
        {
            this.Target = target;
            this.modifier = modifier;
            this.time = time;
        }

        public int GetChange(int currentTime)
        {
            double proportion = 1.0 - (currentTime - time) / (double)(modifier.Duration * Game.TICKS_PER_DAY);
            return (int)Math.Round(proportion  * modifier.Change);
        }

        public bool IsExpired(int currentTime)
        {
            return time + modifier.Duration * Game.TICKS_PER_DAY <= currentTime;
        }

        public override string ToString()
        {
            return string.Format("{0} {1} ({2})", modifier.Change, modifier.Identifier, time);
        }
    }
}
