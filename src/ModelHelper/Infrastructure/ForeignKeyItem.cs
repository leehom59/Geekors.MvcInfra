namespace ModelHelper.Infrastructure
{
    public class ForeignKeyItem
    {
        public ForeignKeyItem()
        {
        }

        public ForeignKeyItem(string value, string text)
        {
            Value = value;
            Text = text;
        }

        public string Value { get; set; }
        public string Text { get; set; }
    }
}