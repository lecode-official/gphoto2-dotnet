
#region Using Directives

using System;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;

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
	/// Gets or sets the error code of the gPhoto2 error.
	/// </summary>
	public string ErrorCode { get; set; }

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
	/// <returns>Returns a <see cref="CameraException" /> if an error was detected and <c>null</c> otherwise.</returns>
	public static void DetectCameraErrors(string output)
	{
		// Creates a new regular expression, which detects an error message in the output of gPhoto2
		Regex errorRegex = new Regex(string.Concat("^", Regex.Escape("*** Error ("),
			"(?<ErrorCode>(([0-9]|-))*): '(?<ErrorMessage>(.*))'", Regex.Escape(") ***"), "$"));
		
		// Tries to match the regular expression with the output of gPhoto2
		Match match = errorRegex.Match(output);
		
		// Checks if the regular expression was a match, if so then the error message from the gPhoto2 output is retrieved and a camera
		// exception is thrown
		if (match.Success)
		{
			throw new CameraException("An error occurred during the processing of the command send to the camera.")
			{
				ErrorCode = match.Groups["ErrorCode"].Value,
				Details = match.Groups["ErrorMessage"].Value
			};
		}
	}
	
	#endregion
}