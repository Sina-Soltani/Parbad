using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Parbad.Internal;
using Parbad.Options;
using System;
using NUnit.Framework;

namespace Parbad.Tests;

[TestFixture]
public class LoggerTests
{
    [Test]
    public void No_Logs_Must_Be_Written_When_Logger_Is_Disabled()
    {
        var options = new OptionsWrapper<ParbadOptions>(new ParbadOptions
                                                        {
                                                            EnableLogging = false
                                                        });

        var fakeLogger = new FakeLogger<LoggerTests>();

        IParbadLogger<LoggerTests> parbadLogger = new ParbadLogger<LoggerTests>(fakeLogger, options);

        parbadLogger.LogInformation("Test");

        Assert.That(fakeLogger.LogReceived, Is.False);
    }

    [Test]
    public void Logs_Must_Be_Written_When_Logger_Is_Enabled()
    {
        var options = new OptionsWrapper<ParbadOptions>(new ParbadOptions
                                                        {
                                                            EnableLogging = true
                                                        });

        var fakeLogger = new FakeLogger<LoggerTests>();

        IParbadLogger<LoggerTests> parbadLogger = new ParbadLogger<LoggerTests>(fakeLogger, options);

        parbadLogger.LogInformation("Test");

        Assert.That(fakeLogger.LogReceived, Is.True);
    }
}

internal class FakeLogger<T> : ILogger<T>, IDisposable
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