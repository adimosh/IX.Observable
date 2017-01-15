using System;

namespace IX.Observable
{
    /// <summary>
    /// Event arguments for 
    /// </summary>
    public class ExceptionOccurredEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ExceptionOccurredEventArgs"/> class.
        /// </summary>
        /// <param name="exception">The exception that has occurred.</param>
        public ExceptionOccurredEventArgs(Exception exception)
        {
            this.Exception = exception;
        }

        /// <summary>
        /// Gets or sets the exception that has occurred.
        /// </summary>
        public Exception Exception { get; private set; }
    }
}