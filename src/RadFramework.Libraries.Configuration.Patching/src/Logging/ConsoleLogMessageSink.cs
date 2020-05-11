using System;

namespace RadFramework.Libraries.Configuration.Patching.Logging
{
    /// <summary>
    /// Simple wrapper around Console.WriteLine.
    /// </summary>
    public class ConsoleLogMessageSink : ILogMessageSink
    {
        private object LockPoint { get; } = new object();

        public bool HasErrors { get; set; }
        public bool HasWarnings { get; set; }

        public void EnterBlock(Action<ILogMessageSink> executeBlock)
        {
            BlockSink blockSink = new BlockSink(this);

            executeBlock(blockSink);

            // lock all access to message while a block is being applied. (bulk lock)
            lock (LockPoint)
            {
                blockSink.Flush();
            }
        }

        public void Message(string message)
        {
            // lock all access to message so we do not interfere with blocks being flushed
            lock (LockPoint)
            {
                Console.WriteLine(message);
            }
        }

        public void Warning(string message)
        {
            // lock all access to message so we do not interfere with blocks being flushed
            lock (LockPoint)
            {
                HasWarnings = true;
                ConsoleColor bak = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"[WARNING]:{message}");
                Console.ForegroundColor = bak;
            }
        }

        public void Error(string message)
        {
            // lock all access to message so we do not interfere with blocks being flushed
            lock (LockPoint)
            {
                HasErrors = true;
                ConsoleColor bak = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Error.WriteLine($"[ERROR]:{message}");
                Console.ForegroundColor = bak;
            }
        }
    }
}