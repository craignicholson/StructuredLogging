// --------------------------------------------------------------------------------------------------------------------
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
    using System.Collections.Generic;
    using System.Configuration;
    using System.Text;
    using System.Threading.Tasks;

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
            LogSamples();
            Console.ReadLine();
        }

        /// <summary>
        /// The log samples.
        /// </summary>
        private static void LogSamples()
        {
            // Demonstrate Property
            // long4net.config 
            log4net.GlobalContext.Properties["EntityName"] = EntityName;
            Logger.Info(MyLogMessage.GetMessage(Guid.NewGuid().ToString(), "LogSamples"));
        }
    }
}
