using QbservableProvider;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;

namespace JustChat
{
    class ChatClient
    {
        private readonly IPEndPoint serviceEndPoint;

        public ChatClient(IPEndPoint serviceEndPoint)
        {
            this.serviceEndPoint = serviceEndPoint;
        }

        public void Connect()
        {
            var client = new QbservableTcpClient<ChatServiceHooks>(serviceEndPoint);

            Console.WriteLine();
            Console.Write("Enter your user name -> ");

            string userName = Console.ReadLine();

            var myMessages = new Subject<string>();

            IObservable<string> outgoingMessages = myMessages;

            IObservable<string> query =
                (from hooks in client.Query(userName)
                     .Do(hooks => outgoingMessages.Subscribe(hooks.IncomingMessages))
                 from message in hooks.OutgoingMessages
                 select message);

            using (query.Subscribe(
                message => ConsoleTrace.WriteLine(ConsoleColor.Blue, message),
                ex => ConsoleTrace.WriteLine(ConsoleColor.Red, "Chat client error: " + ex.Message),
                () => ConsoleTrace.WriteLine(ConsoleColor.DarkCyan, "Chat client completed")))
            {
                Console.WriteLine();
                Console.WriteLine("Chat client started.  You may begin entering messages...");
                Console.WriteLine();
                Console.WriteLine("(Enter a blank line to stop)");
                Console.WriteLine();

                string message;
                while ((message = Console.ReadLine()).Length > 0)
                {
                    myMessages.OnNext(message);
                }
            }
        }
    }
}
