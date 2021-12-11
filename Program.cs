using System;
using System.Collections.Generic;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace LR4
{
    class Program
    {
        static void Main(string[] args)
        {
            //byte[] bytes = new Byte[100];
            Random rnd = new Random();
            //rnd.GetBytes(bytes);
            BigInteger q = 0, N = 0, m = 0;
            bool prime = false;
            while (prime == false)
            {
                q = GenPrime(100); 
                //Console.Write("q = {0} ", q);
                N = 2 * q + 1; m++;
                //Console.WriteLine("N = {0}  m = {1}", N, m);
                if (primeCheck(N) == false) prime = false;
                else prime = true;
            }
            //Console.WriteLine("простое {0}", N);
            BigInteger g = GenPrime(N);
            BigInteger k = '3';
            int s = rnd.Next(10000, 2000000);
            String I = "user";
            String p = "324095709"; 
            BigInteger x = BigInteger.Parse(HashFunction2(s.ToString(), p)); 
            BigInteger v = BigInteger.ModPow(g, x, N);
            Server server = new Server(I, s, v, N, g);
            BigInteger a = GetRandom();
            BigInteger A = BigInteger.ModPow(g, a, N); 
            BigInteger B = server.server_Auth(A); 
            if (B.Equals(BigInteger.Zero)) Console.WriteLine("Клиент: B = 0, соединение отказано"); 
            BigInteger u = BigInteger.Parse(HashFunction2(A.ToString(), B.ToString())); 
            if (u.Equals(BigInteger.Zero)) Console.WriteLine("Клиент: u = 0, соединение приостановлено");
            server.server_U();
            Console.WriteLine("Клиент: B = {0}", B);
            Console.WriteLine("Клиент: k = {0}", k);
            Console.WriteLine("Клиент: v = {0}", v);
            Console.WriteLine("Клиент: a = {0}", a);
            Console.WriteLine("Клиент: u = {0}", u);
            Console.WriteLine("Клиент: x = {0}", x);
            Console.WriteLine("Клиент: N = {0}", N);
            BigInteger S = BigInteger.ModPow((B - k * v), (a + u * x), N); Console.WriteLine("Клиент: S = {0} \n", S);
            String K = ComputeSHA256Hash(S.ToString());
            server.server_SK();
            BigInteger hashN = BigInteger.Parse(ComputeSHA256Hash(N.ToString()));
            BigInteger hashG = BigInteger.Parse(ComputeSHA256Hash(g.ToString()));
            String hashI = ComputeSHA256Hash(I); 
            String M = HashFunction6((hashN ^ hashG).ToString(), hashI, s.ToString(), A.ToString(), B.ToString(), K);
            Console.WriteLine("Клиент: M = {0}", M);
            String server_R = server.server_M(M);
            if (server_R.Equals("0")) Console.WriteLine("Клиент: Сервер приостановил соединение из-за того, что M не равны");
            else
            {
                String R = Program.HashFunction3(A.ToString(), M, K);
                if (R.Equals(server_R)) Console.WriteLine("Соединение установлено");
                else Console.WriteLine("Клиент: R не равны, соединение приостановлено");
            }
        }
        public static string HashFunction6(string NxorG, string hashI, string s, string A, string B, string K)
        {
            string hash = String.Empty;
            NxorG = NxorG.Substring(0, NxorG.Length / 3); //берем первую треть
            hashI = hashI.Substring(0, hashI.Length / 2); //берем половину
            hash = NxorG + hashI; // складываем и получаем какие-то 2/3?
            s = s.Substring(2, s.Length / 2); //берем какую-то часть
            hash = hash.Insert(hash.Length / 2, s); //вставляем s в середину
            A = A.Substring(1, A.Length / 2); //берем какую-то часть
            B = B.Substring(0, B.Length / 2); //берем какую-то часть
            A = A.Insert(0, B); //в А вставляем B
            hash += A;
            K = K.Substring(0, K.Length / 3); //берем 
            hash = hash.Insert(hash.Length / 2, K); //вставляем в середину
            /*Console.WriteLine("NxorG = {0}", NxorG);
            Console.WriteLine("hashI = {0}", hashI);
            Console.WriteLine("s = {0}", s);
            Console.WriteLine("A = {0}", A);
            Console.WriteLine("B = {0}", B);
            Console.WriteLine("K = {0}", K);*/
            return hash;
        }
        public static string ComputeSHA256Hash(string input)
        {
            var crypt = new SHA256Managed();
            string hash = String.Empty;
            byte[] crypto = crypt.ComputeHash(Encoding.UTF8.GetBytes(input));
            foreach (byte theByte in crypto)
            {
                hash += theByte.ToString("x2");
            }
            return Regex.Replace(hash, "(?i)[a-z]", "");
            // Regex.Replace(гдеИщем, чтоИщем, наЧтоМеняем) = найти и заменить
            // (?i) = искать игнорируя регистр букв
            // [a-z] = любой сивол заключенный в квадратные скобки
        }
        public static string HashFunction2(string item1Hash, string item2Hash)
        {
            string hash = String.Empty;
            //item1Hash = item1Hash.Substring(0, item1Hash.Length / 3); //берем первую треть
            //item2Hash = item2Hash.Substring(0, item2Hash.Length / 2); //берем первую половину
            hash = item1Hash + item2Hash;
            //Console.WriteLine("item1Hash = {0}", item1Hash);
            //Console.WriteLine("item2Hash = {0}", item2Hash);
            return Regex.Replace(hash, "(?i)[a-z]", "");
        }
        public static string HashFunction3(string item1Hash, string item2Hash, string item3Hash)
        {
            string hash = String.Empty;
            item1Hash = item1Hash.Substring(0, item1Hash.Length / 3); //берем первую треть
            item2Hash = item2Hash.Substring(0, item2Hash.Length / 2); //берем первую половину
            hash = item1Hash + item2Hash;
            hash += item3Hash.Substring(2, item3Hash.Length / 2);
            return Regex.Replace(hash, "(?i)[a-z]", "");
        }
        public static bool primeCheck(BigInteger n)
        {
            if (n == 1)
                return false;
            if (n == 2)
                return false;
            if (n % 2 == 0)
                return false;
            BigInteger s = 0, d = n - 1;
            int k = 200;
            while (d % 2 == 0)
            {
                d /= 2;
                s++;
            }
            var rand = new Random();
            for (int i = 0; i < k; i++)
            {
                BigInteger a = rand.Next();
                BigInteger x = BigInteger.ModPow(a, d, n);
                if (x == 1 || x == n - 1)
                    continue;
                for (BigInteger j = 0; j < s - 1; j++)
                {
                    x = (x * x) % n;
                    if (x == 1)
                        return false;
                    if (x == n - 1)
                        break;
                }
                if (x != n - 1)
                    return false;

            }
            return true;
        }
        public static BigInteger GenPrime(BigInteger n) //поиск простого числа из n-го количества
        {
            bool[] mass = new bool[(int)n]; //булевый массив
            List<BigInteger> array = new List<BigInteger>();
            for (int i = 0; i < n; i++)
            {
                mass[i] = true;
            }
            for (int p = 2; p < n; p++)
            {
                if (mass[p])
                {
                    for (int i = p * p; i < n; i += p)
                    {
                        mass[i] = false; //заменяем значения массива
                    }
                }
            }
            int j = 0;
            for (int i = 2; i < n; i++)
            {
                if (mass[i]) { array.Add(i); } //получаем массив из простых чисел
            }
            //foreach (int i in array) 
                //Console.Write("{0}, ", i);
            var num = array[new Random().Next(0, array.Count)]; //из массива простых чисел рандомно выбираем ОДНО число
            return num;
        }
        public static int GetRandom() //вывод рандомного числа
        {
            Random rnd = new Random();
            int value = rnd.Next(1, 10000);
            return value;
        }
    }
    class Server
    {
        private int s;
        private String I, K, R, M;
        private BigInteger v, A, N, g, u, B, S, b;
        private BigInteger k = '3';
        RandomNumberGenerator rnd = RNGCryptoServiceProvider.Create();
        public Server(String username, int salt, BigInteger passver, BigInteger N, BigInteger g)
        {
            this.I = username;
            this.s = salt;
            this.v = passver;
            this.N = N;
            this.g = g;
        }
        public BigInteger server_Auth(BigInteger A)
        {
            if (A.Equals(BigInteger.Zero)) Console.WriteLine("Сервер: A=0, соединение отказано");
            this.A = A;
            this.b = Program.GetRandom();
            this.B = (k * v + BigInteger.ModPow(g, b, N)) % N;
            return B;
        }
        public void server_U()
        {
            this.u = BigInteger.Parse(Program.HashFunction2(A.ToString(), B.ToString()));
            if (u.Equals(BigInteger.Zero)) Console.WriteLine("Сервер: u=0, соединение приостановлено");
        }
        public void server_SK()
        {
            Console.WriteLine("Сервер: A = {0}", A);
            Console.WriteLine("Сервер: v = {0}", v);
            Console.WriteLine("Сервер: b = {0}", b);
            Console.WriteLine("Сервер: N = {0}", N);
            this.S = BigInteger.ModPow(BigInteger.Multiply(A,BigInteger.ModPow(v, u, N)), b, N); Console.WriteLine("Сервер: S = {0}\n", S);
            this.K = Program.ComputeSHA256Hash(S.ToString());
        }
        public String server_M(String Client_M)
        {
            BigInteger hashN = BigInteger.Parse(Program.ComputeSHA256Hash(N.ToString()));
            BigInteger hashG = BigInteger.Parse(Program.ComputeSHA256Hash(g.ToString()));
            String hashI = Program.ComputeSHA256Hash(I); 
            this.M = Program.HashFunction6((hashN ^ hashG).ToString(), hashI, s.ToString(), A.ToString(), B.ToString(), K);
            Console.WriteLine("Сервер: M = {0}", M);
            if (!M.Equals(Client_M))
            {
                Console.WriteLine("Сервер: М - клиент и М - сервер не равны, соединение приостановлено");
                return "0";
            }
            else
            {
                R = Program.HashFunction3(A.ToString(), M, K);
                return R;
            }
        }
    }
}
