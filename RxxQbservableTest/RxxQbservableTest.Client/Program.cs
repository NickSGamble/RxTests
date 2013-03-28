using QbservableProvider;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RxxQbservableTest.Client
{
    class Program
    {
        static void Main(string[] args)
        {
            var client = new QbservableTcpClient<long>(new IPEndPoint(IPAddress.Parse("10.6.40.209"), 3205));
            
            using (client.Query().Subscribe(
                value => Console.WriteLine("Client observed: " + value),
                ex => Console.WriteLine("Error: {0}", ex.Message),
                () => Console.WriteLine("Completed")))
            {
                Console.ReadLine();
            }
        }
    }
}
