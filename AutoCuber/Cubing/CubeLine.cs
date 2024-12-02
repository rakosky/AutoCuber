namespace AutoCuber.Cubing
{
    public class CubeLine
    {
        public string Type { get; set; } = Unknown;
        public int Value { get; set; } = 0;
        public CubeLine()
        {

        }
        public CubeLine(string rawText)
        {
            if (!rawText.Contains("%") && !rawText.Contains("boss", StringComparison.CurrentCultureIgnoreCase))
                return;

            var rawsplit = rawText.Split('+');
            if (rawsplit.Length < 2)
                return;

            string type = rawsplit[0];

            foreach (var t in AllTypes)
            {
                if (type.Replace(" ", "").ToUpper().Contains(t.Replace(" ", "").ToUpper()))
                {
                    Type = t;
                    break;
                }
            }

            if (Type is null)
                return;

            string value = string.Concat(rawsplit[1]
                .Where(char.IsDigit));


            Value = int.Parse(value);
        }

        public override string? ToString()
        {
            return $"Type: {Type}, Value: {Value}";
        }

        public static string Unknown = new string("?");
        public static string Crit = new string("Critical Damage");
        public static string ATT = new string("ATT");
        public static string AllStats = new string("All Stats");
        public static string STR = new string("STR");
        public static string LUK = new string("LUK");
        public static string DEX = new string("DEX");
        public static string INT = new string("INT");
        public static string IED = new string("Ignore Monster");
        public static string BossDmg = new string("Boss Monster");
        public static string Damage = new string("Damage");
        public static string Drop = new string("Drop");
        public static string Meso = new string("Meso");

        public static string[] AllTypes = [Crit, ATT, AllStats, STR, LUK, DEX, INT, IED, BossDmg, Damage, Drop, Meso];
    }
}
