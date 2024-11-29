using AutoCuber.AutoFlamer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoCuber.Flaming
{
    public class RequestedFlame
    {
        public List<FlameLine> FlameScoreValues { get; set; }
        public int RequestedFlameScore { get; set; }

        public void Validate()
        {
            if (FlameScoreValues is null || FlameScoreValues.Count == 0)
            {
                throw new Exception();
            }

            if (FlameScoreValues
                .GroupBy(f => f.Stat)
                .Any(g => g.Count() > 1))
            {

                throw new Exception();
            }

            if (RequestedFlameScore <= 0)
            {
                throw new Exception();

            }
        }
    }
}
