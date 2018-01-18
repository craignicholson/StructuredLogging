﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Program.cs" company="Craig Nicholson">
//   MIT
// </copyright>
// <summary>
//   Defines the Program type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace StructuredLogging
{
    using System;
    using System.Collections.ObjectModel;
    using System.Configuration;

    /// <summary>
    /// The program.
    /// </summary>
    public class Program
    {
        /// <summary>
        /// The logger we will use to log transactions to help with debugging.
        /// </summary>
        private static readonly log4net.ILog Logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// The host name for RabbitMQ, which is the server name or IP address.
        /// We use this to connect the the worker queue, and for publishing to EventAnalysisAPI exchange.
        /// </summary>
        private static readonly string EntityName = ConfigurationManager.AppSettings["EntityName"] ?? "Set app.config EntityName to the customer's name please.";

        /// <summary>
        /// The my log message.
        /// </summary>
        private static readonly StructuredMessage MyLogMessage = new StructuredMessage();

        /// <summary>
        /// The main.
        /// </summary>
        /// <param name="args">
        /// The args.
        /// </param>
        public static void Main(string[] args)
        {
            LogInfo();
            Console.ReadLine();
        }

        /// <summary>
        /// The log samples.
        /// </summary>
        private static void LogSamples()
        {
            // The global context is shared by all threads in the current AppDomain. This context is thread
            // safe for use by multiple threads concurrently.  We can create these as needed and add these
            // into layout pattern or have the ability to remove from the conversionPattern.
            // https://logging.apache.org/log4net/release/manual/contexts.html
            // Adding these into the conversionPattern _-> %property{EntityName} and  %property{AllocatedBytes}
            log4net.GlobalContext.Properties["EntityName"] = EntityName;
            log4net.GlobalContext.Properties["AllocatedBytes"] = new StructuredMessage.GcAllocatedBytesHelper();

            Logger.Info(MyLogMessage.GetMessage(Guid.NewGuid().ToString(), "LogSamples"));
            Logger.InfoFormat(MyLogMessage.GetMessage(Guid.NewGuid().ToString(), "LogSamples"));

            Logger.Debug(MyLogMessage.GetMessage(Guid.NewGuid().ToString(), "LogSamples"));
            Logger.DebugFormat(MyLogMessage.GetMessage(Guid.NewGuid().ToString(), "LogSamples"));

            Logger.Error(MyLogMessage.GetMessage(Guid.NewGuid().ToString(), "LogSamples"));
            Logger.ErrorFormat(MyLogMessage.GetMessage(Guid.NewGuid().ToString(), "LogSamples"));

            Logger.Fatal(MyLogMessage.GetMessage(Guid.NewGuid().ToString(), "LogSamples"));
            Logger.FatalFormat(MyLogMessage.GetMessage(Guid.NewGuid().ToString(), "LogSamples"));

            Logger.Warn(MyLogMessage.GetMessage(Guid.NewGuid().ToString(), "LogSamples"));
            Logger.WarnFormat(MyLogMessage.GetMessage(Guid.NewGuid().ToString(), "LogSamples"));
        }

        private static void LogInfo()
        {
            var tz = TimeZoneInfo.GetSystemTimeZones();

            // You can use a correlationId to batch log messages together when a series of actions
            // represent the same unit of work.  If you are processing data, scrubbing, transforming
            // and inserting these types of transactions have many steps and could post the data to many
            // systems and know what is affected down stream is help full.
            var correlationId = new Guid().ToString();
            Logger.Info(
                MyLogMessage.GetMessage(
                    correlationId: correlationId,
                    methodName: "Program.LogInfoMethod",
                    message: "Running LogInfo Test",
                    error: "IF ONE EXISTS",
                    stackTrace: "IF YOU WANT IT",
                    exception: new Exception("Test Exception"),
                    elapsedMilliseconds: 0,
                    anyObject: tz));
        }

        private static void LogErrors()
        {

        }

        private static void LogFatals()
        {

        }
        private static void LogObjects()
        {

        }

        private static void LogWarn()
        {

        }
    }
}
