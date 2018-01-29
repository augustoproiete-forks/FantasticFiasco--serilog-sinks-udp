﻿using System;
using Serilog.Events;
using Xunit;
using Serilog.Support;
using Shouldly;
using System.IO;
using System.Xml.Linq;

namespace Serilog.Sinks.Udp.TextFormatters
{
    public class Log4netTextFormatterTest
    {
        private static readonly XNamespace Namespace = "http://logging.apache.org/log4net/schemas/log4net-events-1.2/";

        private readonly Log4netTextFormatter formatter;
        private readonly TextWriter output;

        public Log4netTextFormatterTest()
        {
            formatter = new Log4netTextFormatter();
            output = new StringWriter();
        }

        [Fact]
        public void Logger()
        {
            // Arrange
            var logEvent = Some.LogEvent();
            logEvent.AddOrUpdateProperty(new LogEventProperty("SourceContext", new ScalarValue("source context")));

            // Act
            formatter.Format(logEvent, output);

            // Assert
            Deserialize().Root.Attribute("logger").Value.ShouldBe("source context");
        }

        [Fact]
        public void Timestamp()
        {
            // Act
            formatter.Format(Some.LogEvent(), output);

            // Assert
            var timestamp = Deserialize().Root.Attribute("timestamp").Value;
            DateTime.TryParse(timestamp, out DateTime _).ShouldBeTrue();
        }

        [Theory]
        [InlineData(LogEventLevel.Verbose, "TRACE")]
        [InlineData(LogEventLevel.Debug, "DEBUG")]
        [InlineData(LogEventLevel.Information, "INFO")]
        [InlineData(LogEventLevel.Warning, "WARN")]
        [InlineData(LogEventLevel.Error, "ERROR")]
        [InlineData(LogEventLevel.Fatal, "FATAL")]
        public void Level(LogEventLevel actual, string expected)
        {
            // Act
            formatter.Format(Some.LogEvent(level: actual), output);

            // Assert
            Deserialize().Root.Attribute("level").Value.ShouldBe(expected);
        }

        [Fact]
        public void Thead()
        {
            // Arrange
            var logEvent = Some.LogEvent();
            logEvent.AddOrUpdateProperty(new LogEventProperty("ThreadId", new ScalarValue("1")));

            // Act
            formatter.Format(logEvent, output);

            // Assert
            Deserialize().Root.Attribute("thread").Value.ShouldBe("1");
        }

        [Fact]
        public void Username()
        {
            // Arrange
            var logEvent = Some.LogEvent();
            logEvent.AddOrUpdateProperty(new LogEventProperty("EnvironmentUserName", new ScalarValue("some user")));

            // Act
            formatter.Format(logEvent, output);

            // Assert
            Deserialize().Root.Attribute("username").Value.ShouldBe("some user");
        }

        [Fact]
        public void Domain()
        {
            // Arrange
            var logEvent = Some.LogEvent();
            logEvent.AddOrUpdateProperty(new LogEventProperty("ProcessName", new ScalarValue("some domain")));

            // Act
            formatter.Format(logEvent, output);

            // Assert
            Deserialize().Root.Attribute("domain").Value.ShouldBe("some domain");
        }

        [Fact]
        public void Class()
        {
            // Arrange
            var logEvent = Some.LogEvent();
            logEvent.AddOrUpdateProperty(new LogEventProperty("SourceContext", new ScalarValue("source context")));

            // Act
            formatter.Format(logEvent, output);

            // Assert
            Deserialize().Root.Element(Namespace + "locationInfo").Attribute("class").Value.ShouldBe("source context");
        }

        [Fact]
        public void Method()
        {
            // Arrange
            var logEvent = Some.LogEvent();
            logEvent.AddOrUpdateProperty(new LogEventProperty("Method", new ScalarValue("Void Method()")));

            // Act
            formatter.Format(logEvent, output);

            // Assert
            Deserialize().Root.Element(Namespace + "locationInfo").Attribute("method").Value.ShouldBe("Void Method()");
        }
        
        [Fact]
        public void MachineName()
        {
            // Arrange
            var logEvent = Some.LogEvent();
            logEvent.AddOrUpdateProperty(new LogEventProperty("MachineName", new ScalarValue("MachineName")));

            // Act
            formatter.Format(logEvent, output);

            // Assert
            Deserialize().Root.Element(Namespace + "properties").Element(Namespace + "data").Attribute("value").Value.ShouldBe("MachineName");
        }

        [Fact]
        public void Message()
        {
            // Arrange
            var logEvent = Some.LogEvent(message: "Some message");

            // Act
            formatter.Format(logEvent, output);

            // Assert
            Deserialize().Root.Element(Namespace + "message").Value.ShouldBe("Some message");
        }

        [Fact]
        public void Exception()
        {
            // Arrange
            var logEvent = Some.LogEvent(exception: new DivideByZeroException());

            // Act
            formatter.Format(logEvent, output);

            // Assert
            Deserialize().Root.Element(Namespace + "throwable").Value.ShouldNotBeNull();
        }

        private XDocument Deserialize()
        {
            return XDocument.Parse(output.ToString());
        }
    }
}
