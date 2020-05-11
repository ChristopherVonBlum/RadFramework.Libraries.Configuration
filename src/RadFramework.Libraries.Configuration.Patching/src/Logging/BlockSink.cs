using System;
using System.Collections.Generic;
using System.Linq;

namespace RadFramework.Libraries.Configuration.Patching.Logging
{
    /// <summary>
    /// The implementation type for ILogMessageSink.EnterBlocks argument.
    /// </summary>
    public class BlockSink : ILogMessageSink
    {
        private readonly ILogMessageSink _logMessageSink;
        private List<LogMessage> messages = new List<LogMessage>();
        private object LockPoint { get; } = new object();

        public BlockSink(ILogMessageSink logMessageSink)
        {
            _logMessageSink = logMessageSink;
        }

        public bool HasErrors => _logMessageSink.HasErrors || messages.OfType<ErrorMessage>().Any();
        public bool HasWarnings => _logMessageSink.HasWarnings || messages.OfType<WarningMessage>().Any();

        public void EnterBlock(Action<ILogMessageSink> executeBlock)
        {
            BlockSink blockSink = new BlockSink(this);

            executeBlock(blockSink);

            lock (LockPoint)
            {
                blockSink.Flush();
            }
        }

        public void Message(string message)
        {
            lock (LockPoint)
            {
                messages.Add(new LogMessage { Message = message });
            }
        }

        public void Warning(string message)
        {
            lock (LockPoint)
            {
                messages.Add(new WarningMessage { Message = message });
            }
        }

        public void Error(string message)
        {
            lock (LockPoint)
            {
                messages.Add(new ErrorMessage { Message = message });
            }
        }

        public void Flush()
        {
            List<LogMessage> messages = this.messages;
            this.messages = new List<LogMessage>();
            messages.ForEach(l =>
            {
                if(l is ErrorMessage)
                {
                    _logMessageSink.Error(l.Message);
                    return;
                }

                if (l is WarningMessage)
                {
                    _logMessageSink.Warning(l.Message);
                    return;
                }

                _logMessageSink.Message(l.Message);
            });
            messages.Clear();
        }
    }
}