using Microsoft.VisualBasic;

namespace AutoCuber.AutoFlamer
{
    public class FlameLine
    {
        public string Stat { get; set; } = FlameStats.Unknown;
        public double Value { get; set; }

        public FlameLine()
        {
            
        }

        public FlameLine(string rawText)
        {
            var rawsplit = rawText.Split('+');
            if (rawsplit.Length < 2)
                return;
            string type = rawsplit[0];

            foreach (var t in FlameStats.AllTypes)
            {
                if (type.Replace(" ", "").ToUpper().Contains(t.Replace(" ", "").ToUpper()))
                {
                    Stat = t;
                    break;
                }
            }

            if (Stat is null)
                return;

            string value = string.Concat(rawsplit[1]
                .Where(char.IsDigit));


            Value = int.Parse(value);
        }

        public override string? ToString()
        {
            return $"Stat: {Stat}, Value: {Value}";
        }

    }
}
