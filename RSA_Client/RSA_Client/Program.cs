using System;
using System.Net;
using System.Net.Sockets;
using System.Numerics;
using System.Text;

namespace RSA_Client
{
    class Program
    {
        public static int Main(String[] args)
        {
            StartClient();
            return 0;
        }


        public static void StartClient()
        {
            byte[] bytes = new byte[1024];

            try
            {
                System.Net.IPHostEntry host = Dns.GetHostEntry("localhost");
                IPAddress ipAddress = host.AddressList[0];
                IPEndPoint remoteEP = new IPEndPoint(ipAddress, 8888);

                // Create a TCP/IP  socket.    
                Socket sender = new Socket(ipAddress.AddressFamily,
                    SocketType.Stream, ProtocolType.Tcp);

                // Connect the socket to the remote endpoint. Catch any errors.    
                try
                {
                    // Connect to Remote EndPoint  
                    sender.Connect(remoteEP);

                    Console.WriteLine("Socket connected to {0}",
                        sender.RemoteEndPoint.ToString());

                    int bytesRec = sender.Receive(bytes);
                    Console.WriteLine("[SERVER] {0}", Encoding.ASCII.GetString(bytes, 0, bytesRec));

                    bytesRec = sender.Receive(bytes);
                    //Console.WriteLine("test");
                    String trns = Encoding.ASCII.GetString(bytes, 0, bytesRec);
                    Console.WriteLine(trns);
                    trns = trns.Substring(trns.IndexOf(":") + 2);
                    int publickey = Int32.Parse(trns);

                    bytesRec = sender.Receive(bytes);
                    //Console.WriteLine("test");
                    trns = Encoding.ASCII.GetString(bytes, 0, bytesRec);
                    Console.WriteLine(trns);
                    trns = trns.Substring(trns.IndexOf(":") + 2);
                    long n = long.Parse(trns);

                    //Console.WriteLine("public key: " + publickey + " | N: " + n);

                    while (true)
                    {
                        String s = Console.ReadLine();
                        int bytesSent;
                        byte[] msg;
                        if (s.Contains("<EOF>"))
                        {
                            msg = Encoding.ASCII.GetBytes(s);
                            bytesSent = sender.Send(msg);
                            break;
                        }
                        s = scramble(s, publickey, n);
                        msg = Encoding.ASCII.GetBytes(s);    
                        bytesSent = sender.Send(msg);
                        
                    }
                    // Encode the data string into a byte array.
                    

                    // Receive the response from the remote device.    
                    bytesRec = sender.Receive(bytes);
                    Console.WriteLine("[SERVER] {0}",
                        Encoding.ASCII.GetString(bytes, 0, bytesRec));

                    long mili = factor(n);
                    Console.WriteLine("Time to factor : " + mili + "ms");
                    double t = (n / (double)(mili / 1000.0));
                    Console.WriteLine("Operation time of " + t + " checks / second");
                    //Console.WriteLine("Checking an average of 53 bits a second.");
                    // Release the socket.    
                    sender.Shutdown(SocketShutdown.Both);
                    sender.Close();
                    Console.ReadLine();
                }
                catch (ArgumentNullException ane)
                {
                    Console.WriteLine("ArgumentNullException : {0}", ane.ToString());
                }
                catch (SocketException se)
                {
                    Console.WriteLine("SocketException : {0}", se.ToString());
                }
                catch (Exception e)
                {
                    Console.WriteLine("Unexpected exception : {0}", e.ToString());
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        public static BigInteger modularexp(long bas, long exp, long mod)
        {
            BigInteger num = new BigInteger(0);
            if (bas == 0) return 0;
            if (exp == 0) return 1;

            //long num;
            if (exp % 2 == 0)
            {
                num = modularexp(bas, exp / 2, mod);
                num = (num * num) % mod;
            }
            else
            {
                num = bas % mod;
                num = (num * modularexp(bas, exp - 1, mod) % mod) % mod;
            }

            return num;
        }

        public static String scramble(String s, int pk, long n)
        {
            long nums = long.Parse(s);
            return modularexp(nums, pk, n).ToString();

        }

        public static long factor(long n)
        {
            long upto = (long) Math.Sqrt(n);
            var stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();
            for(int i = 3; i < upto; i++)
            {
                if(n % i == 0)
                {
                    stopwatch.Stop();
                    Console.WriteLine("Found: " + i + " | " + (n / i));
                    return stopwatch.ElapsedMilliseconds;
                }
            }
            return -1;
            
        }
    }
}
