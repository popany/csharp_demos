using NUnit.Framework;
using unit_test.Utils;
using System.Linq;
using System.Configuration;
using System.IO;

namespace unit_test.TestFixtures
{
    public class ThreadWork
    {
        public static void DoWork()
        {
            try
            {
                for (int i = 0; i < 100; i++)
                {
                    Log.Info("Thread - working.");
                    System.Threading.Thread.Sleep(100);
                }
            }
            catch (System.Threading.ThreadAbortException e)
            {
                Log.Info("Thread - caught ThreadAbortException - resetting.");
                Log.Error(string.Format("Exception message: {0}", e.Message));
                System.Threading.Thread.ResetAbort();
            }
            Log.Info("Thread - still alive and working.");
            System.Threading.Thread.Sleep(1000);
            Log.Info("Thread - finished working.");
        }
    }

    class TestThread
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void TestThreadAbort()
        {
            System.Threading.ThreadStart threadDelegate = new System.Threading.ThreadStart(ThreadWork.DoWork);
            System.Threading.Thread thread = new System.Threading.Thread(threadDelegate);
            thread.Start();
            System.Threading.Thread.Sleep(100);
            Log.Info("Main - aborting my thread.");
            thread.Abort();
            thread.Join();
            Log.Info("Main ending.");
        }
    }
}

