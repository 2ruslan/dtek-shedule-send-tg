using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DtekSheduleSendTg.DTEK
{
    public record DtekDateFromFilePrarms
    {
        public int StartX { get; set; }
        public int StartY { get; set; }
        public int FinishX { get; set; }
        public int FinishY { get; set; }
    }
}
