
#region Using Directives

using System;
using System.Runtime.Serialization;

#endregion

namespace System.Devices
{
    /// <summary>
    /// Represents an exception, which is thrown when a camera setting is not supported by the camera.
    /// </summary>
    [Serializable]
    public class CameraSettingException : Exception
    {
        #region Constructors

        /// <summary>
        /// Initializes a new <see cref="CameraSettingException"/> instance.
        /// </summary>
        public CameraSettingException()
        {
        }

        /// <summary>
        /// Initializes a new <see cref="CameraSettingException"/> instance.
        /// </summary>
        /// <param name="message">The message that describes what went wrong.</param>
        public CameraSettingException(string message)
            : base(message) 
        {
        }

        /// <summary>
        /// Initializes a new <see cref="CameraSettingException"/> instance.
        /// </summary>
        /// <param name="message">The message that describes what went wrong.</param>
        /// <param name="innerException">The original exception, which caused this exception to be thrown.</param>
        public CameraSettingException(string message, Exception innerException)
            : base (message, innerException)
        {
        }

        /// <summary>
        /// Initializes a new <see cref="CameraSettingException"/> instance.
        /// </summary>
        /// <param name="info">The serialization information, which is needed to serialize the exception.</param>
        /// <param name="context">The serialization context, which is needed to serialize the exception.</param>
        protected CameraSettingException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        #endregion
    }
}