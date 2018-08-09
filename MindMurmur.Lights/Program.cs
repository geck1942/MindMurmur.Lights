using System;
using System.Reactive.Linq;
using EasyNetQ;
using MindMurmur.Domain.Messages;
using MindMurmur.Lights.Control;

namespace MindMurmur.Lights
{
    class Program
    {
        static void Main(string[] args)
        {
            var keepRunning = true;
            //initialize config
            Config.Init();
            //StartTimerForTestEvents();
            StartLightManagerTest();
            //StartLightManager();
            Console.WriteLine("Listening for messages. Hit < return > to quit.");
        }

        public static void StartLightManager()
        {
            Console.WriteLine("Shows use of Start to start on a background thread:");
            //StartTimerForTestEvents();
            var o = Observable.Start(async () =>
            {
                Console.WriteLine("StartLightManager...");
                var manager = new LightManager(new DMX(Config.MaxChannels));
                await manager.Start();
                Console.WriteLine("Background work completed.");
            }).Finally(() => Console.WriteLine("Main thread completed."));
            Console.WriteLine("\r\n\t In Main Thread...\r\n");
            o.Wait();   // Wait for completion of background operation.
        }

        public static void StartLightManagerTest()
        {
            Console.WriteLine("Press any key EXCEPT FOR < return > to start testing.  Press < return > when given the option to stop testing.");
            var manager = new LightManager(new DMX(Config.MaxChannels));
            manager.HitchToTheBus();//add subscriptions to the bus

            while (Console.ReadKey().Key != ConsoleKey.Enter)
            {
                var o = Observable.Start(async () =>
                {
                    Console.WriteLine("StartLightManager...");
                    await manager.RunTestsTask();
                    Console.WriteLine("Background work completed.");
                }).Finally(() => Console.WriteLine("Main thread completed."));
                Console.WriteLine("\r\n\t Press Enter to ...\r\n");
                o.Wait();   // Wait for completion of background operation.
            }
        }

        public static void StartTimerForTestEvents()
        {
            IBus bus = RabbitHutch.CreateBus("host=localhost");

            Observable
                .Timer(TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(15))
                .Subscribe(
                    x =>
                    {
                        var random = new Random();
                        var mediationStateCmd = new MeditationStateCommand()
                        {
                            CommandId = Guid.NewGuid().ToString(),
                            State = Convert.ToInt16(random.Next(-1, 5))
                        };
                        var heartCmd = new HeartRateCommand()
                        {
                            CommandId = Guid.NewGuid().ToString(),
                            HeartRate = Convert.ToInt16(random.Next(60, 170))
                        };

                        bus.Publish(mediationStateCmd);
                        Console.BackgroundColor = ConsoleColor.DarkRed;
                        Console.WriteLine("Published test MeditationStateCommand.");

                        bus.Publish(heartCmd);
                        Console.WriteLine("Published test HeartRateCommand.");
                        Console.BackgroundColor = ConsoleColor.Black;
                        Console.ForegroundColor = ConsoleColor.White;
                    });
            Console.WriteLine("StartTimerForTestEvents()....");
        }
    }

}
