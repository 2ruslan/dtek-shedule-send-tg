using DtekSheduleSendTg.Abstraction;
using System.Text;

namespace DtekSheduleSendTg
{
    public class Monitoring2Txt(string header) : IMonitoring
    {
        private readonly StringBuilder sb = new StringBuilder();
        private DateTime startDt;
        private DateTime checkpoint;

        private readonly Dictionary<string, int> counters = new();

        public void Start()
        {
            startDt = checkpoint = DateTime.Now;
            sb.Append($"<code>{header}");
            sb.AppendFormat("\r\nStart: {0}",  startDt.ToString("yyyy.MM.dd HH:mm:ss"));
        }

        public void CounterRgister(string name)
        {
            if (!counters.ContainsKey(name))
                counters[name] = 0;
        }

        public void Counter(string name)
        {
            if (counters.ContainsKey(name))
                counters[name]++;
            else 
                counters[name] = 1;
        }
         
        public void Append(string name, object value)
            => sb.AppendFormat("\r\n{0}: {1}", name, value);

        public void AddCheckpoint(string name)
        {
            var currentDt = DateTime.Now;
            sb.AppendFormat("\r\n{0}: {1} {2:0.00}s.",
                                name,
                                currentDt.ToString("HH:mm:ss"),
                                (currentDt - checkpoint).TotalSeconds
                            );
            checkpoint = currentDt;
        }

        public void Finish()
        {
            foreach (var item in counters)
                Append(item.Key, item.Value);

            var finishDt = DateTime.Now;
            sb.AppendFormat("\r\nFinish: {0}", finishDt.ToString("HH:mm:ss"));
            sb.AppendFormat("\r\nTotal: {0:0.00}s.", (finishDt - startDt).TotalSeconds );
            sb.Append("</code>");
        }

        public string GetInfo()
            => sb.ToString();
    }
}
