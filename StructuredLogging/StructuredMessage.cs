// --------------------------------------------------------------------------------------------------------------------
// <copyright file="StructuredMessage.cs" company="CraigNicholson">
//   MIT
// </copyright>
// <summary>
//   Defines the StructuredMessage type.
//       https://www.owasp.org/index.php/Logging_Cheat_Sheet
//       https://logging.apache.org/log4net/log4net-1.2.13/release/sdk/log4net.Layout.PatternLayout.html
//   In your code define the Structured Message.
//           private static readonly StructuredMessage LogMessage = new StructuredMessage();
//           private static readonly log4net.ILog Logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
//           Logger.Info(LogMessage.GetMessage(transactionId, "Connect", "Consumer started and listening for outageEvents"));
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace StructuredLogging
{
    using System;
    using System.Net;

    using Newtonsoft.Json;

    /// <summary>
    /// The message for log4net.
    /// </summary>
    [Serializable]
    public class StructuredMessage
    {
        #region Properties

        /// <summary>
        /// Gets or sets the environment variables.
        /// </summary>
        [JsonProperty(PropertyName = "environmentVariables")]
        private EnvironmentVariables environmentVariables = new EnvironmentVariables();

        /// <summary>
        /// Gets or sets the app name.
        /// </summary>
        [JsonProperty(PropertyName = "appName")]
        private string appName = System.AppDomain.CurrentDomain.FriendlyName;

        /// <summary>
        /// The app version.
        /// </summary>
        private string appVersion = string.Empty;

        #endregion

        /// <summary>
        /// Gets or sets the correlation id.  Used to help keep transactions for multiple lines together.
        /// If you need to keep log lines as a transaction while cleaning, scrubbing and transforming data.
        /// </summary>
        [JsonProperty(PropertyName = "correlationId")]
        private string CorrelationId { get; set; }

        /// <summary>
        /// Gets or sets the method name set by the user since using %method is too expensive in log4net.
        /// Can also be used as a grouper to group similar parts of a transaction together by name.
        /// </summary>
        [JsonProperty(PropertyName = "methodName")]
        private string MethodName { get; set; }

        /// <summary>
        /// Gets or sets the message sent in by the user.
        /// </summary>
        [JsonProperty(PropertyName = "message")]
        private string Message { get; set; }

        /// <summary>
        /// Gets or sets the error string sent in by the user.  Typically the exception.Message.
        /// </summary>
        [JsonProperty(PropertyName = "error")]
        private string Error { get; set; }

        /// <summary>
        /// Gets or sets the stackTrace if user feels the need to pass this into the logger.
        /// </summary>
        [JsonProperty(PropertyName = "stackTrace")]
        private string StackTrace { get; set; }

        /// <summary>
        /// Gets or sets the elapsed milliseconds for a method.
        /// </summary>
        [JsonProperty(PropertyName = "elapsedMilliseconds")]
        private long ElapsedMilliseconds { get; set; }

        /// <summary>
        /// Gets or sets the local date time for the message, the logger uses UTC time.
        /// </summary>
        [JsonProperty(PropertyName = "localDateTime")]
        private DateTime LocalDateTime { get; set; }

        /// <summary>
        /// Gets or sets the exception received. Can skip a stack trace with this.
        /// </summary>
        [JsonProperty(PropertyName = "exceptionReceived")]
        private Exception ExceptionReceived { get; set; }

        /// <summary>
        /// Gets or sets the generic object.  Maybe to dump other types of data I can't imagine.
        /// How can I change the property name to represent the actual getType
        /// </summary>
        [JsonProperty(PropertyName = "object")]
        private object GenericObject { get; set; }

        /// <summary>
        /// Gets or sets the object type.
        /// </summary>
        [JsonProperty(PropertyName = "objectType")]
        private object ObjectType { get; set; }

        /// <summary>
        /// Garbage Collector get allocated bytes helper.
        /// Trying another way to demonstrate use case.
        /// </summary>
        public class GcAllocatedBytesHelper
        {
            /// <summary>
            /// The to string.
            /// </summary>
            /// <returns>
            /// The <see cref="string"/>.
            /// </returns>
            public override string ToString()
            {
                return GC.GetTotalMemory(true).ToString();
            }
        }

        /// <summary>
        /// The get message.
        /// </summary>
        /// <param name="correlationId">
        /// The correlation id.
        /// </param>
        /// <param name="methodName">
        /// The method name.
        /// </param>
        /// <param name="message">
        /// The message.
        /// </param>
        /// <param name="error">
        /// The error.
        /// </param>
        /// <param name="stackTrace">
        /// The stack trace.
        /// </param>
        /// <param name="exception">
        /// The e.
        /// </param>
        /// <param name="anyObject">
        /// The object.
        /// </param>
        /// <param name="elapsedMilliseconds">
        /// The elapsed milliseconds.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public string GetMessage(
            string correlationId,
            string methodName,
            string message = "",
            string error = null,
            string stackTrace = null,
            Exception exception = null,
            object anyObject = null,
            long elapsedMilliseconds = 0)
        {
            StructuredMessage msg = null;
            if (anyObject != null)
            {
                msg = new StructuredMessage
                {
                    CorrelationId = correlationId,
                    MethodName = methodName,
                    Message = message,
                    Error = error,
                    StackTrace = stackTrace,
                    ExceptionReceived = exception,
                    ElapsedMilliseconds = elapsedMilliseconds,
                    LocalDateTime = DateTime.Now,
                    ObjectType = anyObject.GetType(),
                    GenericObject = anyObject
                };
            }
            else
            {
                msg = new StructuredMessage
                {
                    CorrelationId = correlationId,
                    MethodName = methodName,
                    Message = message,
                    Error = error,
                    StackTrace = stackTrace,
                    ExceptionReceived = exception,
                    ElapsedMilliseconds = elapsedMilliseconds,
                    LocalDateTime = DateTime.Now,
                };
            }
            var response = JsonConvert.SerializeObject(msg);
            return response;
        }

        /// <summary>
        /// The environment variables
        /// </summary>
        [Serializable]
        private class EnvironmentVariables
        {
            /// <summary>
            /// Gets or sets the server.
            /// </summary>
            [JsonProperty(PropertyName = "machineName")]
            private string machineName = Environment.MachineName;

            /// <summary>
            /// The IP address.
            /// </summary>
            [JsonProperty(PropertyName = "ipAddress")]
            private string ipAddress = Dns.GetHostAddresses(Environment.MachineName)[0].ToString();

            /// <summary>
            /// Gets or sets the operating system.
            /// </summary>
            [JsonProperty(PropertyName = "operatingSystem")]
            private string operatingSystem = Environment.OSVersion.VersionString;

            /// <summary>
            /// The user.
            /// </summary>
            [JsonProperty(PropertyName = "userName")]
            private string userName = Environment.UserName;

            /// <summary>
            /// The user domain name.
            /// </summary>
            [JsonProperty(PropertyName = "userDomainName")]
            private string userDomainName = Environment.UserDomainName;


            /// <summary>
            /// Gets or sets the memory. Is this an expensive task?
            /// </summary>
            [JsonProperty(PropertyName = "totalMemory")]
            private long totalMemory = GC.GetTotalMemory(true);

            /// <summary>
            /// The working set.
            /// </summary>
            [JsonProperty(PropertyName = "workingSet")]
            private long workingSet = Environment.WorkingSet;
        }
    }
}