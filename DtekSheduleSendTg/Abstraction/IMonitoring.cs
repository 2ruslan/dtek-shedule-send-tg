using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;

namespace DtekSheduleSendTg.Abstraction
{
    public interface IMonitoring
    {
        void Start();
        void CounterRgister(string name);
        void Counter(string name);
        void Append(string name, object value);
        void AddCheckpoint(string name);
        void Finish();
        string GetInfo();
    }
}
