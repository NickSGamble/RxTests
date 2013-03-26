using System;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;

namespace RxxSocketTest
{
    public class SocketLab
    {
        public static void Main()
        {
            byte[] serverArray = new byte[1024];
            IPAddress serverAddress = IPAddress.Parse(ConfigurationManager.AppSettings["serverAddress"]);
            int serverPort = int.Parse(ConfigurationManager.AppSettings["serverPort"]);
            IPEndPoint serverEndPoint = new IPEndPoint(serverAddress, serverPort);

            var task = GetListener(serverEndPoint);
            task.Wait();

            var server = Observable.Repeat(serverArray)
                         .Consume<byte[], int>(array =>
                             {
                                 return task.Result.ReceiveObservable(serverArray, 0, 1024, SocketFlags.None);
                             });

            Subject<long> totalServer = new Subject<long>(); 

            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            var serverDisposable =
                server.SubscribeOn(ThreadPoolScheduler.Instance)
                .ObserveOn(Scheduler.Immediate)
                .Subscribe(x =>
                {
                    if (x > 0) totalServer.OnNext(x);
                    else totalServer.OnCompleted();
                });
            
            var oneMegabit = 1048576;

            totalServer
                .SubscribeOn(NewThreadScheduler.Default)
                .Window(TimeSpan.FromSeconds(1))
                .Subscribe(x =>
                    {
                        x.SubscribeOn(ThreadPoolScheduler.Instance).Aggregate((seed, incr) => seed + incr)
                            .Subscribe(sum => Console.WriteLine("server speed: {0}", sum/oneMegabit));
                    });

            Console.ReadLine();

            stopWatch.Stop();
            serverDisposable.Dispose();

            Console.ReadLine();
        }

        private static async Task<Socket> GetListener(IPEndPoint serverEndPoint)
        {
            IObservable<Socket> serverSocket =
                from socket in ObservableSocket.Accept(
                    AddressFamily.InterNetwork,
                    SocketType.Stream,
                    ProtocolType.Tcp,
                    serverEndPoint,
                    count: 1).Do(_ => Console.WriteLine("server created"))
                select socket;

            var listener = await serverSocket.FirstAsync();
            return listener;
        }
    }
}