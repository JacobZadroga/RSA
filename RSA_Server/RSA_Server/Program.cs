using System;
using System.Net;
using System.Net.Sockets;
using System.Numerics;
using System.Text;

namespace RSA_Server
{
    class Program
    {
        static int Main(string[] args)
        {
            StartServer();
            return 0;
        }

        public static void StartServer()
        {
            IPHostEntry host = Dns.GetHostEntry("localhost");
            IPAddress ip = host.AddressList[0];
            IPEndPoint localEnd = new IPEndPoint(ip, 8888);
            try
            {
                string data = null;
                byte[] bytes = null;
                Socket listener = new Socket(ip.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                listener.Bind(localEnd);
                listener.Listen(2);


                long privatekey = -1;
                int attempts = 0;
                int p = 0, q = 0, publickey = 0;
                long n = 0, carmichael = 0;
                publickey = 1327;
                while (privatekey == -1)
                {
                    p = generatePrime(80000000, 100000000);
                    q = generatePrime(80000000, 100000000);
                    n = (long)p * q;
                    carmichael = LCM(p - 1, q - 1);
                    privatekey = modInverse(publickey, carmichael);
                    attempts++;
                }
                Console.WriteLine("ATTEMPTS: " + attempts);
                Console.WriteLine("p: " + p + " | q: " + q);
                Console.WriteLine("N: " + n);
                Console.WriteLine("Carmichael's Totient: " + carmichael);
                Console.WriteLine("Public Key: " + publickey + " | Private Key: " + privatekey);


                Console.WriteLine("Waiting for connection...");
                Socket handler = listener.Accept();

                handler.Send(Encoding.ASCII.GetBytes("You have connected to the Server!"));
                System.Threading.Thread.Sleep(500);
                handler.Send(Encoding.ASCII.GetBytes("Public key : 1327"));
                System.Threading.Thread.Sleep(500);
                handler.Send(Encoding.ASCII.GetBytes("N : " + n));

                while (true)
                {
                    bytes = new byte[1024];
                    int bytesRec = handler.Receive(bytes);
                    data = Encoding.ASCII.GetString(bytes, 0, bytesRec);
                    if(data.IndexOf("<EOF>") > -1)
                    {
                        break;
                    }
                    Console.WriteLine("[Client] " + data);
                    Console.WriteLine("[Unscrambled] " + unscramble(data, privatekey, n));
                }

                Console.WriteLine("Ending Connection.");
                handler.Send(Encoding.ASCII.GetBytes("Ending Connection."));
                handler.Shutdown(SocketShutdown.Both);
                handler.Close();
            } catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

            Console.WriteLine("\nPress a Key...");
            Console.ReadKey();
        }

        public static int Receive()
        {

            return 0;
        }

        public static bool IsPrime(int n, int k) //Miller - Rabin tests
        {
            if ((n < 2) || (n % 2 == 0)) return (n == 2);

            int s = n - 1;
            while (s % 2 == 0) s >>= 1;

            Random r = new Random();
            for (int i = 0; i < k; i++)
            {
                //Console.WriteLine("Next");
                int a = r.Next(n - 1) + 1;
                int temp = s;
                long mod = 1;
                for (int j = 0; j < temp; ++j) mod = (mod * a) % n;
                while (temp != n - 1 && mod != 1 && mod != n - 1)
                {
                    mod = (mod * mod) % n;
                    temp *= 2;
                }

                if (mod != n - 1 && temp % 2 == 0) return false;
            }
            return true;
        }

        public static int generatePrime(int start, int end)
        {
            Random rng = new Random();
            int i = rng.Next(start, end);
            int primesTested = 0;
            while(!IsPrime(i, 3))
            {
                i = rng.Next(start, end);
                primesTested++;
            }
            Console.WriteLine(primesTested);
            return i;
        }

        public static long LCM(int x, int y)
        {
            long num1, num2;
            if (x > y)
            {
                num1 = x; num2 = y;
            }
            else
            {
                num1 = y; num2 = x;
            }

            for (int i = 1; i < num2; i++)
            {
                long mult = num1 * i;
                if (mult % num2 == 0)
                {
                    return mult;
                }
            }
            return num1 * num2;
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

        public static long gcd(long x, long y)
        {
            while(x != 0 && y != 0)
            {
                if(x > y)
                {
                    x %= y;
                } else
                {
                    y %= x;
                }
            }

            return x | y;
        }

        public static long modInverse(long bas, long mod)
        {
            long i = mod, v = 0, d = 1;
            if(gcd(bas, mod) > 1)
            {
                return -1;
            }
            while (bas > 0)
            {
                long t = i / bas, x = bas;
                bas = i % x;
                i = x;
                x = d;
                d = v - t * x;
                v = x;
            }
            v %= mod;
            if (v < 0) v = (v + mod) % mod;
            return v;
        }

        public static String unscramble(String s, long privkey, long n)
        {
            long nums = long.Parse(s);
            return modularexp(nums, privkey, n).ToString();
        }



    }
}
