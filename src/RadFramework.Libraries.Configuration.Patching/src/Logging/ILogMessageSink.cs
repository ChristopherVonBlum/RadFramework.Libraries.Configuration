using System;

namespace RadFramework.Libraries.Configuration.Patching.Logging
{
    /// <summary>
    /// A basic logging abstraction.
    /// </summary>
    public interface ILogMessageSink
    {
        bool HasErrors { get; }
        bool HasWarnings { get; }

        /// <summary>
        /// The log messages executed on the lambdas ILogMessageSink argument are guaranteed to appear as a block in a sequential log sink.
        /// This makes it possible to maintain logical blocks while using threading.
        /// </summary>
        /// <param name="executeBlock"></param>
        void EnterBlock(Action<ILogMessageSink> executeBlock);

        /// <summary>
        /// Writes a message to the log...
        /// </summary>
        /// <param name="message"></param>
        void Message(string message);

        /// <summary>
        /// Writes a warning to the log...
        /// </summary>
        /// <param name="message"></param>
        void Warning(string message);

        /// <summary>
        /// Writes a warning to the log...
        /// </summary>
        /// <param name="message"></param>
        void Error(string message);
    }
}
