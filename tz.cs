using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using System.IO;
 
namespace SocketTcpClient
{
    class Program
    {
        static string address = "";
        static int port = 0;

        static List<int> list = new List<int>();
        static object locker = new object();

        static int numberOfThreads = 128;

        static int endCounter = 0;
        static object lockerEnd = new object();


        /////////////////////////////

        static void Main(string[] args)
        {
            for (int i = 1; i <= numberOfThreads; ++i)
            {
                Thread thread = new Thread(new ParameterizedThreadStart(ThreadVoid));
                thread.Start(i.ToString() + "." + numberOfThreads.ToString());
            }
        }

        /////////////////////////////


        static bool DigitFound(string str)
        {
            foreach (char i in str)
                if (i >= '0' && i <= '9')
                    return true;
            return false;
        }

        static int GetNumber(string response)
        {
            return int.Parse(string.Join("", response.Where(x => char.IsDigit(x))));
        }

        static void AllEnded()
        {
            Console.WriteLine("\n\n-----: ANSWER :-----");
            list.Sort();
            Console.WriteLine((list[1008] + list[1009]) / 2f);
            Console.Read();
        }

        static void ThreadVoid(object arg)
        {
            try
            {
                string args = (string)arg;
                int i = args.IndexOf(".");
                int mod = int.Parse(args.Substring(0, i));
                int numberOfThreads = int.Parse(args.Substring(i + 1, args.Length - i - 1));;
                
                while (mod <= 2018)
                {
                    IPEndPoint ipPoint = new IPEndPoint(IPAddress.Parse(address), port);
                    Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    
                    try
                    {
                        socket.Connect(ipPoint);
                    }
                    catch
                    {
                        Console.WriteLine("-----: ERROR TO CONNECT :-----");
                        Thread.Sleep(200);
                        continue;
                    }

                    byte[] request = Encoding.GetEncoding("KOI8-R").GetBytes(mod.ToString() + "\n");

                    try
                    {
                        socket.Send(request);
                    }
                    catch
                    {
                        Console.WriteLine("-----: ERROR TO SEND :-----");
                        Thread.Sleep(200);
                        continue;
                    }
                    
                    Console.WriteLine("-----: " + mod.ToString() + " :-----");

                    var data = new byte[200];
                    StringBuilder builder = new StringBuilder();
                    int bytes = 0;
                    bool digitFound = false;
                    int countEmptyResponses = 0;

                    while (countEmptyResponses <= 1)
                    {
                        Random rnd = new Random();                        
                        Thread.Sleep(1600 + rnd.Next(0, 400));

                        try
                        {
                            bytes = socket.Receive(data, data.Length, 0);
                        }
                        catch
                        {
                            Console.WriteLine("-----: ERROR TO RECEIVE :-----");
                            mod -= numberOfThreads;
                            break;
                        }
                        

                        var koi8rString = Encoding.GetEncoding("KOI8-R").GetString(data, 0, bytes);
                        digitFound = digitFound || DigitFound(koi8rString);

                        builder.Append(koi8rString);

                        string str = builder.ToString();
                        
                        if (string.IsNullOrEmpty(koi8rString))
                            countEmptyResponses++;

                        if (digitFound && !string.IsNullOrEmpty(koi8rString) && !("1234567890".Contains(koi8rString[koi8rString.Length - 1].ToString())))
                        {
                            lock (locker)
                            {
                                list.Add(GetNumber(str));
                            }
                            break;
                        }
                    }

                    mod += numberOfThreads;

                    if (countEmptyResponses > 1)
                        mod -= numberOfThreads;

                    socket.Shutdown(SocketShutdown.Both);
                    socket.Close();
                }

                lock (lockerEnd)
                {
                    endCounter++;
                    if (endCounter == numberOfThreads)
                        AllEnded();
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine("-----: ERROR OF THREAD :-----");
                Console.WriteLine(ex);
                Console.WriteLine("-----------------------------");
            }
        }
    }
}