namespace MoreGamemodes
{
    public class RoleOptionItem : OptionItem
    {
        public IntegerValueRule Rule;

        public RoleOptionItem(int id, CustomRoles role, TabGroup tab, bool isSingleValue)
        : base(id, CustomRolesHelper.RoleNames[role], 0, tab, isSingleValue)
        {
            Rule = new(0, 105, 5);
            SetColor(CustomRolesHelper.RoleColors[role]);
            SetValueFormat(OptionFormat.Percent);
        }
        public static RoleOptionItem Create(
            int id, CustomRoles role, TabGroup tab, bool isSingleValue
        )
        {
            return new RoleOptionItem(
                id, role, tab, isSingleValue
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