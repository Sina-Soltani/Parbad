using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NUnit.Framework;
using Parbad.Internal;
using Parbad.Options;
using System;

namespace Parbad.Tests
{
    public class LoggerTests
    {
        [Test]
        public void No_Logs_Must_Be_Written_When_Logger_Is_Disabled()
        {
            var options = new OptionsWrapper<ParbadOptions>(new ParbadOptions
            {
                EnableLogging = false
            });

            var logger = new TestLogger<LoggerTests>();

            IParbadLogger<LoggerTests> parbadLogger = new ParbadLogger<LoggerTests>(logger, options);

            parbadLogger.Log(logging => logging.LogInformation("Test"));

            Assert.IsFalse(logger.LogReceived);
        }

        [Test]
        public void Logs_Must_Be_Written_When_Logger_Is_Enabled()
        {
            var options = new OptionsWrapper<ParbadOptions>(new ParbadOptions
            {
                EnableLogging = true
            });

            var logger = new TestLogger<LoggerTests>();

            IParbadLogger<LoggerTests> parbadLogger = new ParbadLogger<LoggerTests>(logger, options);

            parbadLogger.Log(logging => logging.LogInformation("Test"));

            Assert.IsTrue(logger.LogReceived);
        }
    }

    internal class TestLogger<T> : ILogger<T>, IDisposable
    {
        public bool LogReceived { get; private set; }

        public IDisposable BeginScope<TState>(TState state)
        {
            return this;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            LogReceived = true;
        }

        public void Dispose()
        {
        }
    }
}
