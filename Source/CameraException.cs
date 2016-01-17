
#region Using Directives

using System;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;

#endregion

namespace System.Devices
{
    /// <summary>
    /// Represents an exception, which is thrown when an error occurs during using the camera.
    /// </summary>
    [Serializable]
    public class CameraException : Exception
    {
        #region Constructors

        /// <summary>
        /// Initializes a new <see cref="CameraException"/> instance.
        /// </summary>
        public CameraException()
        {
        }

        /// <summary>
        /// Initializes a new <see cref="CameraException"/> instance.
        /// </summary>
        /// <param name="message">The message that describes what went wrong.</param>
        public CameraException(string message)
            : base(message) 
        {
        }

        /// <summary>
        /// Initializes a new <see cref="CameraException"/> instance.
        /// </summary>
        /// <param name="message">The message that describes what went wrong.</param>
        /// <param name="innerException">The original exception, which caused this exception to be thrown.</param>
        public CameraException(string message, Exception innerException)
            : base (message, innerException)
        {
        }

        /// <summary>
        /// Initializes a new <see cref="CameraException"/> instance.
        /// </summary>
        /// <param name="info">The serialization information, which is needed to serialize the exception.</param>
        /// <param name="context">The serialization context, which is needed to serialize the exception.</param>
        protected CameraException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        #endregion
        
        #region Private Static Fields
        
        /// <summary>
        /// Contains the regular expression for the first type of error messages. This error message comes in this format: "*** Error
        /// ({ErrorCode}: '{ErrorMessage}') ***".
        /// </summary>
        private static Regex firstTypeErrorRegex = new Regex(@"^\*\*\* Error \(((-|[0-9])*): '(?<ErrorMessage>(.*))'\) \*\*\*(.*)$",
            RegexOptions.Multiline);
        
        /// <summary>
        /// Contains the regular expression for the first type of error messages. This error message comes in this format: "*** Error ***
        /// <NewLine>{ErrorMessage}".
        /// </summary>
        private static Regex secondTypeErrorRegex = new Regex(@"^\*\*\* Error \*\*\*(.*)$\n^(?<ErrorMessage>(.*))$",
            RegexOptions.Multiline);
        
        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets details of the gPhoto2 error.
        /// </summary>
        public string Details { get; set; }

        #endregion
        
        #region Public Static Methods
        
        /// <summary>
        /// Checks the output of gPhoto2 for any error messages and returns a <see cref="CameraException" />
        /// exception if an error was detected in the output.
        /// <summary>
        /// <param name="output">The output of gPhoto2, which is to be searched for DetectCameraErrors.</param>
        public static void DetectCameraErrors(string output)
        {
            // Tries to match the regular expressions with the output of gPhoto2
            string errorMessage = string.Empty;
            foreach (Match match in CameraException.firstTypeErrorRegex.Matches(output))
                errorMessage = string.Concat(errorMessage, Environment.NewLine, match.Groups["ErrorMessage"].Value);
            foreach (Match match in CameraException.secondTypeErrorRegex.Matches(output))
                errorMessage = string.Concat(errorMessage, Environment.NewLine, match.Groups["ErrorMessage"].Value);
            
            // Checks if the regular expressions were a match, if so then the error message from the gPhoto2 output is retrieved and a camera
            // exception is thrown
            if (!string.IsNullOrWhiteSpace(errorMessage))
            {
                throw new CameraException("An error occurred during the processing of the command send to the camera.")
                {
                    Details = errorMessage
                };
            }
        }
        
        #endregion
    }
}