using System;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;

namespace RxxSocketTest.Client
{
    class Program
    {
        static void Main(string[] args)
        {
            byte[] clientArray = new byte[1024];
            IPAddress serverAddress = IPAddress.Parse(ConfigurationManager.AppSettings["serverAddress"]);
            int serverPort = int.Parse(ConfigurationManager.AppSettings["serverPort"]);
            IPEndPoint serverEndPoint = new IPEndPoint(serverAddress, serverPort);

            var task = GetConnectedClient(serverEndPoint);
            task.Wait();

            var client =
                     Observable
                        .Repeat(clientArray)
                        .Consume<byte[], int>(array =>
                            {
                                return task.Result.SendToObservable(array, 0, 1024, SocketFlags.None, serverEndPoint);
                            });
                            
            Subject<long> totalClient = new Subject<long>();
            Stopwatch stopWatch = new Stopwatch();

            stopWatch.Start();
            var clientDisposable =
                client.SubscribeOn(ThreadPoolScheduler.Instance)
                .ObserveOn(Scheduler.Immediate)
                .Subscribe(x =>
                    {
                        if (x > 0) totalClient.OnNext(x);
                        else totalClient.OnCompleted();
                    });

            var oneMegabit = 1048576;

            totalClient
                .SubscribeOn(ThreadPoolScheduler.Instance)
                .Window(TimeSpan.FromSeconds(1))
                .Subscribe(x =>
                {
                    x.SubscribeOn(ThreadPoolScheduler.Instance).Aggregate((seed, incr) => seed + incr)
                        .Subscribe(sum => Console.WriteLine("client speed: {0}", sum / oneMegabit));
                });

            Console.ReadLine();

            stopWatch.Stop();
            clientDisposable.Dispose();

            Console.ReadLine();
        }

        private static async Task<Socket> GetConnectedClient(IPEndPoint serverEndPoint)
        {
            IObservable<Socket> clientSocket =
               from socket in ObservableSocket.Connect(
                   AddressFamily.InterNetwork,
                   SocketType.Stream,
                   ProtocolType.Tcp,
                   serverEndPoint)
               select socket;

            var connectedClient = await clientSocket.FirstAsync();
            return connectedClient;
        }
    }
}
