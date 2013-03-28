using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reactive;
using Rxx;
using Rxx.Parsers;
using Rxx.Properties;
using Rxx.Parsers.Linq;
using Rxx.Parsers.Reactive;
using Rxx.Parsers.Reactive.Linq;
using QbservableProvider;

namespace RxxQbservableTest
{
    class Program
    {
        static void Main(string[] args)
        {
            IObservable<long> source = Observable.Interval(TimeSpan.FromSeconds(1));

            var service = source.ServeQbservableTcp(
                new IPEndPoint(IPAddress.Parse("10.6.40.209"), 3205));

            using (service.Subscribe(
                client => Console.WriteLine("Client shutdown."),
                ex => Console.WriteLine("Error: {0}", ex.Message),
                () => Console.WriteLine("This will never be printed because a service host never completes.")))
            {
                Console.ReadLine();
            }
        }
    }
}
