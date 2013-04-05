using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace JustChat
{
    class Program
    {
        static void Main(string[] args)
        {
            IPEndPoint serviceEndPoint = new IPEndPoint(IPAddress.Loopback, 25673);

            ChatService service = new ChatService(serviceEndPoint);
            ChatClient client = new ChatClient(serviceEndPoint);

            var trace = new TraceSource("Custom", SourceLevels.All);
            var disposable = service.Start(trace);
            client.Connect();
        }
    }
}
