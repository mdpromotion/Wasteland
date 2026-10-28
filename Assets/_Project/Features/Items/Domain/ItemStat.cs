namespace _Project.Features.Items.Domain
{
    public readonly struct ItemStat
    {
        public string Label { get; }
        public string Value { get; }

        public ItemStat(string label, string value)
        {
            Label = label;
            Value = value;
        }
    }
}