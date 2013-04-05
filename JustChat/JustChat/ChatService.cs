using QbservableProvider;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Reactive.Subjects;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reactive.Concurrency;

namespace JustChat
{
    class ChatService
    {
        private readonly IPEndPoint serviceEndPoint;

        public ChatService(IPEndPoint serviceEndPoint)
        {
            this.serviceEndPoint = serviceEndPoint;
        }

        public IDisposable Start(TraceSource trace)
        {
            var messageDispatch = new Subject<string>();

            messageDispatch.Subscribe(message => ConsoleTrace.WriteLine(ConsoleColor.Green, message));

            var service = QbservableTcpServer.CreateService<string, ChatServiceHooks>(
                serviceEndPoint,
                new QbservableServiceOptions() { EnableDuplex = true, AllowExpressionsUnrestricted = true },
                request =>
                    (from userName in request
                     from hooks in Observable.Create<ChatServiceHooks>(
                        observer =>
                        {
                            messageDispatch.OnNext(userName + " is online.");

                            var hooks = new ChatServiceHooks(userName, messageDispatch);

                            Scheduler.CurrentThread.Schedule(() => observer.OnNext(hooks));

                            return () => messageDispatch.OnNext(userName + " is offline.");
                        })
                     select hooks)
                    .TraceSubscriptions(trace, "Chat Service"));

            return service.Subscribe(
                terminatedClient =>
                {
                    foreach (var ex in terminatedClient.Exceptions)
                    {
                        ConsoleTrace.WriteLine(ConsoleColor.Magenta, "Chat service error: " + ex.SourceException.Message);
                    }

                    ConsoleTrace.WriteLine(ConsoleColor.Yellow, "Chat client shutdown: " + terminatedClient.Reason);
                },
                ex => ConsoleTrace.WriteLine(ConsoleColor.Red, "Chat service fatal error: " + ex.Message),
                () => Console.WriteLine("This will never be printed because a service host never completes."));
        }
    }
}
