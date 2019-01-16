using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Net;

namespace GC2Toolkit
{
    class Program
    {
        static string Version = "Build 20170501_BemaniCN_Final";
        static void Main(string[] args)
        {
            IPAddress[] addressList = Dns.GetHostAddresses(Dns.GetHostName());
            MainServer Listener;
            try
            {
                Listener = new MainServer(Version);
                Thread ListenerThread = new Thread(new ThreadStart(Listener.listen));
                ListenerThread.Start();
            }
            catch(Exception e)
            {
                Console.WriteLine("Error when setting up server.");
                Console.WriteLine("  "+e.Message);
                Console.WriteLine(e.StackTrace);

                Console.WriteLine("\nIf this is a network-related error,please ensure you have a network connection ");
                Console.WriteLine("and port 80 is not used.");
                Console.WriteLine("Else, try restart computer and try again.");
                Console.ReadLine();
                return;
            }
            Console.Title = "GC2 FullUnlock Toolkit | " + Version;
            Console.WriteLine("GC2 FullUnlock Toolkit Online.");

            Console.WriteLine("Set gc2.gczero.com to following address in hosts to get started.");
            foreach (var address in addressList)
            {
                Console.WriteLine(address);
                
            }
            Console.WriteLine("-------------");
            
            //Console.WriteLine("Enter tutorial to get detail.");
            while (true)
            {
                Console.WriteLine("\nCurrent SongSlot:" + Listener.MaxStage + ",AvaterSlot:" + Listener.MaxAvater);
                Console.WriteLine("Enter help for command detail.");
                Console.Write("GC2Toolkit>");
                string Input = Console.ReadLine();
                if (Input == "exit") Environment.Exit(0);
                else if (Input == "help") Help();
                else if (Input == "reset") Listener.ResetTimer();
                else if (Input == "set") SetTimer(Listener);
                else if (Input == "clear") Console.Clear();
                else if (Input == "updatepak") Listener.PakUpdater();
                else if (Input == "songcount") SetSong(Listener);
                else if (Input == "avatercount") SetAvater(Listener);
                else if (Input == "default") SetDefault(Listener);
            }
        }

        static void Help()
        {
            Console.WriteLine("------\nCommand line help:");
            Console.WriteLine("exit:Exit this tool.");
            Console.WriteLine("reset:Reset login timer to Day 1.");
            Console.WriteLine("set:Set login timer to day you want.");
            Console.WriteLine("clear:Clear screen.");
            Console.WriteLine("songcount:Set song slot to be inserted(Default:400)");
            Console.WriteLine("avatercount:Set avater slot to be inserted(Default:86)");
            Console.WriteLine("default:Restore to default settings.\n------");
        }

        static void SetAvater(MainServer ToSet)
        {
            Console.WriteLine("Caution: too many avater slot will cause game crash!");
            Console.Write("How many avater you want to insert(Default & Tested:90) :");
            while (true)
            {
                try
                {
                    int NewAvaterCount = int.Parse(Console.ReadLine());
                    ToSet.MaxAvater = NewAvaterCount;
                    Console.Write("Avater slot has updated to " + NewAvaterCount.ToString());
                    Console.WriteLine("Restart game to take effect.");
                    return;
                }
                catch
                {
                    Console.WriteLine("Error happended when setting new avater slot.\nPlease try again.");
                    Console.ReadLine();
                }
            }
        }

        static void SetSong(MainServer ToSet)
        {
            Console.WriteLine("Caution: too many song slot will cause game crash!");
            Console.Write("How many song you want to insert(Default & Tested:400) :");
            while (true)
            {
                try
                {
                    int NewAvaterCount = int.Parse(Console.ReadLine());
                    ToSet.MaxAvater = NewAvaterCount;
                    Console.Write("Song slot has updated to " + NewAvaterCount.ToString());
                    Console.WriteLine(",restart game to take effect.");
                    Console.ReadLine();
                    return;
                }
                catch
                {
                    Console.WriteLine("Error happended when setting new song slot.\nPlease try again.");
                }
            }
        }

        static void SetDefault(MainServer ToSet)
        {
            ToSet.MaxAvater = 86;
            ToSet.MaxStage = 400;

            Console.WriteLine("Avater & Song slot has restored to default.\n");
        }

        static void SetTimer(MainServer ToSet)
        {
            Console.WriteLine("Current Day:" + ToSet.ItemGetCount.ToString() + "\n--------");
            Console.WriteLine("Which day matches you?");
            Console.WriteLine("Day 1:Follow/Day 2:Groove+/Day 3:Change/Day 4:Visible");
            Console.WriteLine("Day 5:Mirror/Day 6:Just/Day 7:Hidden/Day 8:Sudden");
            Console.WriteLine("Day 9:Stealth/Day 10:No Way\n--------");
            Console.WriteLine("Ps:Song in last 5 days is already unlocked.");
            
            while (true)
            {
                try
                {
                    int Input = int.Parse(Console.ReadLine());
                    if (Input <= 0 || Input >= 11) throw new Exception("Out of range");
                    ToSet.SetTimer(Input);
                    return;
                }
                catch
                {
                    Console.WriteLine("Input error.Please try again.");
                }
            }
            
        }

    }
}
