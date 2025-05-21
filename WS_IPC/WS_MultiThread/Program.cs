using System;
using System.Threading;

namespace WS_MultiThread
{
    internal class Program
    {
        static int _nb_thread_in_progress;
        static int _countExclusif_access;
        static Mutex _mutex = new Mutex();
        static Semaphore _semaphore = new Semaphore(3, 3); // max 3 
        static CountdownEvent _countdown;
        static int _maxThread = 300;
        static void Main(string[] args)
        {
            _countdown = new CountdownEvent(_maxThread);
            for (int i = 0; i < _maxThread; i++)
            {
                string name = "Thread_" + i;
                ThreadPool.QueueUserWorkItem(state => FctA(name));
                System.Threading.Thread.Sleep(10);
            }
            // Attendre que tous les threads aient terminé
            _countdown.Wait();

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Tous les threads ont terminé !");
            Console.ResetColor();
        }

        static void FctA(string name)
        {
            int inProgress = Interlocked.Increment(ref _nb_thread_in_progress);
            Console.WriteLine("Thread {0} is at the start of FctA: {1}", name, inProgress);

            // Tentative avec délai d’attente
            bool entered = _semaphore.WaitOne(200); // 200 ms max

            if (entered)
            {
                try
                {
                    Put_Exclusive_access(name);
                }
                finally
                {
                    _semaphore.Release(); // libère la place même en cas d’erreur
                }
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Thread {0} was too impatient and skipped the exclusive access", name);
                Console.ForegroundColor = ConsoleColor.White;
            }

            inProgress = Interlocked.Decrement(ref _nb_thread_in_progress);
            Console.WriteLine("Thread {0} is at the end of FctA: {1}", name, inProgress);

            // Signaler que ce thread est terminé
            _countdown.Signal();
        }


        static void Put_Exclusive_access(string name)
        {
            _mutex.WaitOne(); // Entrée critique
            try { 
            Console.WriteLine("Thread {0} is entering the exclusive access zone ", name);
            Interlocked.Increment(ref _countExclusif_access);
            System.Threading.Thread.Sleep(50);

            if (_countExclusif_access > 1)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Error, there are {0} in the exclusive access zone ", _countExclusif_access);
                Console.ForegroundColor = ConsoleColor.White;
            }

            System.Threading.Thread.Sleep(50);
            Interlocked.Decrement(ref _countExclusif_access);
            Console.WriteLine("Thread {0} is leaving the exclusive access zone ", name);
            }
            finally
            {
                _mutex.ReleaseMutex(); // Sortie critique
            }
        }

    }
}
