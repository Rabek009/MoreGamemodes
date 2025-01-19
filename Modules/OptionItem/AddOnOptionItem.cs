namespace MoreGamemodes
{
    public class AddOnOptionItem : OptionItem
    {
        public IntegerValueRule Rule;

        public AddOnOptionItem(int id, AddOns addOn, TabGroup tab, bool isSingleValue)
        : base(id, AddOnsHelper.AddOnNames[addOn], 0, tab, isSingleValue)
        {
            Rule = new(0, 105, 5);
            SetColor(AddOnsHelper.AddOnColors[addOn]);
            SetValueFormat(OptionFormat.Percent);
        }
        public static AddOnOptionItem Create(
            int id, AddOns addOn, TabGroup tab, bool isSingleValue
        )
        {
            return new AddOnOptionItem(
                id, addOn, tab, isSingleValue
            );
        }

        public override int GetInt() => Rule.GetValueByIndex(CurrentValue);
        public override float GetFloat() => Rule.GetValueByIndex(CurrentValue);
        public override string GetString()
        {
            if (Rule.GetValueByIndex(CurrentValue) > 100) return "Always";
            return ApplyFormat(Rule.GetValueByIndex(CurrentValue).ToString());
        }
        public override int GetValue()
            => Rule.RepeatIndex(base.GetValue());

        public override void SetValue(int value)
        {
            base.SetValue(Rule.RepeatIndex(value));
        }
    }
}