
#region Using Directives

using System;
using System.Runtime.Serialization;

#endregion

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

	#region Public Properties

	/// <summary>
	/// Gets or sets exception details.
	/// </summary>
	public string Details { get; set; }

	#endregion
}