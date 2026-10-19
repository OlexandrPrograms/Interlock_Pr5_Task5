using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Interlock_Pr5
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;

            object machine = new object();
            int totalCups = 0;
            int totalRejects = 0;
            int beans = 0;

            Stat[] stats = new Stat[12];
            for (int i = 0; i < 12; i++)
            {
                stats[i] = new Stat { id = i + 1 };
            }

            Thread[] threads = new Thread[12];

            for (int i = 0; i < 12; i++)
            {
                int id = i + 1;
                threads[i] = new Thread(() =>
                {
                    Random r = new Random();
                    DateTime start = DateTime.Now;

                    while ((DateTime.Now - start).TotalSeconds < 10)
                    {
                        Thread.Sleep(r.Next(500, 1500));

                        bool taken = false;
                        DateTime waitStart = DateTime.Now;

                        try
                        {
                            Monitor.TryEnter(machine, 400, ref taken);
                            if (taken)
                            {
                                int wait = (int)(DateTime.Now - waitStart).TotalMilliseconds;
                                Interlocked.Add(ref stats[id - 1].waitTime, wait);

                                beans++;
                                if (beans >= 15)
                                {
                                    beans = 0;
                                    Console.WriteLine($"Співробітник #{id} засипає зерна...");
                                    Thread.Sleep(1000);
                                }

                                Thread.Sleep(r.Next(300, 700));

                                Interlocked.Increment(ref stats[id - 1].cups);
                                Interlocked.Increment(ref totalCups);
                                Console.WriteLine($"Співробітник #{id} випив каву");
                            }
                            else
                            {
                                Interlocked.Increment(ref stats[id - 1].rejects);
                                Interlocked.Increment(ref totalRejects);
                                Console.WriteLine($"Співробітник #{id} черга завелика");
                                Thread.Sleep(r.Next(1000, 2000));
                            }
                        }
                        finally
                        {
                            if (taken) Monitor.Exit(machine);
                        }
                    }
                });
                threads[i].Start();
            }

            foreach (Thread t in threads)
            {
                t.Join();
            }

            Console.WriteLine("\n--- ПІДСУМОК ---");
            Console.WriteLine($"Всього чашок: {totalCups}, Відмов: {totalRejects}");

            var sorted = stats.OrderByDescending(x => x.cups).ToArray();

            Console.WriteLine("ID\tЧашки\tВідмови\tЧас очікування");
            foreach (Stat s in sorted)
            {
                Console.WriteLine($"{s.id}\t{s.cups}\t{s.rejects}\t{s.waitTime} мс");
            }
        }
    }
}
