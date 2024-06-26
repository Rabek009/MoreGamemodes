namespace MoreGamemodes
{
    public class TextOptionItem : OptionItem
    {
        public FloatValueRule Rule;

        public TextOptionItem(int id, string name, TabGroup tab)
        : base(id, name, 0, tab, true)
        {
            IsText = true;
        }
        public static TextOptionItem Create(
            int id, string name, TabGroup tab
        )
        {
            return new TextOptionItem(
                id, name, tab
            );
        }
    }
}