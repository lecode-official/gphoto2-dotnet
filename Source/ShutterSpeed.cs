
#region Using Directives

using System;
using System.Globalization;

#endregion

namespace System.Devices
{
    /// <summary>
    /// Represents a shutter speed of the camera.
    /// </summary>
    public class ShutterSpeed
    {
        #region Constructors
        
        /// <summary>
        /// Initializes a new <see cref="ShutterSpeed" /> instance.
        /// </summary>
        /// <param name="textualRepresentation">The textual representation of the shutter speed retrieved from the camera.</param>
        public ShutterSpeed(string textualRepresentation)
        {
            this.TextualRepresentation = textualRepresentation;
            this.Speed = ShutterSpeed.ParseTextualRepresentation(this.TextualRepresentation);
        }
        
        #endregion
        
        #region Public Properties
        
        /// <summary>
        /// Gets the textual representation of the shutter speed retrieved from the camera.
        /// </summary>
        public string TextualRepresentation { get; private set; }
        
        /// <summary>
        /// Gets the shutter speed. A shutter speed of <c>TimeSpan.MaxValue</c> means Bulb mode, where the camera exposes the image for as long as the release is pressed.
        /// </summary>
        public TimeSpan Speed { get; private set; }
        
        #endregion
        
        #region Private Static Methods
        
        /// <summary>
        /// Parses the textual representation of the shutter speed and turns it into a <see cref="TimeSpan" />.
        /// </summary>
        /// <param name="textualRepresentation">The textual representation of the shutter speed, which is to be parsed.</param>
        /// <returns>Returns the shutter speed as a <see cref="TimeSpan" />.</returns>
        private static TimeSpan ParseTextualRepresentation(string textualRepresentation)
        {
            if (textualRepresentation.ToUpperInvariant() == "BULB")
            {
                return TimeSpan.MaxValue;
            }
            else if (textualRepresentation.Contains("/"))
            {
                string[] fractionElements = textualRepresentation.Split('/');
                if (fractionElements.Length < 2)
                    throw new CameraException("The shutter speed could not be properly retrieved for an unknown reason.");
                double numerator, denominator;
                if (!double.TryParse(fractionElements[0], NumberStyles.Number, CultureInfo.InvariantCulture, out numerator))
                    throw new CameraException(string.Format(CultureInfo.InvariantCulture, "The shutter speed {0} is not supported.", textualRepresentation));
                if (!double.TryParse(fractionElements[1], NumberStyles.Number, CultureInfo.InvariantCulture, out denominator))
                    throw new CameraException(string.Format(CultureInfo.InvariantCulture, "The shutter speed {0} is not supported.", textualRepresentation));
                return TimeSpan.FromTicks(Convert.ToInt64(numerator / denominator * 1000000L));
            }
            else
            {
                double seconds;
                if (!double.TryParse(textualRepresentation, NumberStyles.Number, CultureInfo.InvariantCulture, out seconds))
                    throw new CameraException(string.Format(CultureInfo.InvariantCulture, "The shutter speed {0} is not supported.", textualRepresentation));
                return TimeSpan.FromSeconds(seconds);
            }
        }
        
        #endregion
    }
}