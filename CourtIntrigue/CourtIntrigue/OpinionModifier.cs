namespace CourtIntrigue
{
    internal class OpinionModifier
    {
        public int Change { get; private set; }
        public string Description { get; private set; }
        public int Duration { get; private set; }
        public string Identifier { get; private set; }
        public string Label { get; private set; }

        public OpinionModifier(string identifier, string label, string description, int duration, int change)
        {
            Identifier = identifier;
            Label = label;
            Description = description;
            Duration = duration;
            Change = change;
        }
    }
}