// ------------------------------------------------------------------------------------------------- 
// <copyright file="CommandLineException.cs" company="Bill Hay">
// Copyright (c) William D Hay
// </copyright>
// -------------------------------------------------------------------------------------------------
namespace CommandLine
{
    using System;
    using System.Globalization;
    using System.Runtime.Serialization;

    /// <summary>
    /// Custom CommandLineException class
    /// </summary>
    /// <remarks>
    /// Provides the catcher of the exception with the name, value and type of the failing parameter
    /// as well as a (hopefully) informative Message.
    /// </remarks>
    [Serializable]
    public class CommandLineException : SystemException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CommandLineException"/> class
        /// </summary>
        public CommandLineException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandLineException"/> class with a specified error message.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <remarks>Not used, but included to keep FxCop happy</remarks>
        public CommandLineException(string message)
        {
            throw new NotImplementedException(message);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandLineException"/> class with a specified 
        /// error message and a reference to the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="message">
        /// The message that describes the error.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, 
        /// or a null reference (Nothing in Visual Basic) if no inner exception is specified. 
        /// </param>
        /// <remarks>Not used, but included to keep FxCop happy</remarks>
        public CommandLineException(string message, Exception innerException)
        {
            throw new NotImplementedException(message, innerException);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandLineException"/> class
        /// </summary>
        /// <param name="parameterName">The name of command line parameter</param>
        /// <param name="parameterValue">The value of the command line parameter</param>
        /// <param name="parameterType">The type of the target to which this parameter will be applied</param>
        /// <param name="innerException">The inner exception</param>
        public CommandLineException(string parameterName, string parameterValue, Type parameterType, Exception innerException)
            : base(CommandLineException.MakeMessage(parameterName, parameterValue, parameterType, innerException), innerException)
        {
            this.ParameterName = parameterName;
            this.ParameterValue = parameterValue;
            this.ParameterType = parameterType;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandLineException"/> class
        /// </summary>
        /// <param name="parameterName">The name of command line parameter</param>
        /// <param name="parameterValue">The value of the command line parameter</param>
        /// <param name="reason">The type of the target to which this parameter will be applied</param>
        public CommandLineException(string parameterName, string parameterValue, string reason)
            : base(CommandLineException.MakeMessage(parameterName, parameterValue, reason))
        {
            this.ParameterName = parameterName;
            this.ParameterValue = parameterValue;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandLineException"/> class with serialized data.
        /// </summary>
        /// <param name="info">The object that holds the serialized object data.</param>
        /// <param name="context">The contextual information about the source or destination.</param>
        protected CommandLineException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        /// <summary>
        /// Gets or sets the parameter name
        /// </summary>
        public string ParameterName { get; set; }

        /// <summary>
        /// Gets or sets the parameter value
        /// </summary>
        public string ParameterValue { get; set; }

        /// <summary>
        /// Gets or sets the type of the target of the set
        /// </summary>
        public Type ParameterType { get; set; }

        /// <summary>
        /// When overridden in a derived class, sets the SerializationInfo with information about the exception.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/>that holds the serialized object data about the exception being thrown</param>
        /// <param name="context">The <see cref="StreamingContext"/> that contains contextual information about the source or destination.</param>
        /// <remarks>To keep FxCop happy</remarks>
        public override void GetObjectData(
            SerializationInfo info,
            StreamingContext context)
            {
                base.GetObjectData(info, context);
            }

        /// <summary>
        /// Utility to construct the reason text
        /// </summary>
        /// <param name="name">The parameter name</param>
        /// <param name="value">The parameter value</param>
        /// <param name="parameterType">The type of the target being set</param>
        /// <param name="innerException">The inner exception</param>
        /// <returns>The formatted reason text</returns>
        private static string MakeMessage(string name, string value, Type parameterType, Exception innerException)
        {
            return string.Format(
                CultureInfo.CurrentCulture,
                "Exception parsing comand line parameter: name = {0}, value = {1}, type = {2} exception = {3}",
                name,
                value,
                parameterType.Name,
                innerException?.Message ?? string.Empty);
        }

        /// <summary>
        /// Utility to construct the reason text
        /// </summary>
        /// <param name="name">The parameter name</param>
        /// <param name="value">The parameter value</param>
        /// <param name="reason">The reason for the exception</param>
        /// <returns>The formatted reason text</returns>
        private static string MakeMessage(string name, string value, string reason)
        {
            return string.Format(
                CultureInfo.CurrentCulture,
                "Exception parsing comand line parameter: name = {0}, value = {1}, reason = {2}",
                name,
                value,
                reason);
        }
    }
}
