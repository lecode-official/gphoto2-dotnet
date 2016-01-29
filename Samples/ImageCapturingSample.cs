
#region Using Directives

using System;
using System.Devices;
using System.Globalization;
using System.Threading.Tasks;

#endregion

namespace SamplesApplication
{
    /// <summary>
    /// Represents a gPhoto2.NET sample, which gets the first camera attached to the system and captures an image.
    /// </summar>
    public class ImageCapturingSample : ISample
    {
        #region ISample Implementation
        
        /// <summary>
        /// Gets the title of the sample.
        /// </summary>
        public string Title
        {
            get
            {
                return "Capture images";
            }
        }
        
        /// <summary>
        /// Executes the sample.
        /// </summary>
        /// <param name="camera">The camera with which the sample is to be executed.</param>
        public async Task ExecuteAsync(Camera camera)
        {
            // Captures an image and stores it on the camera
            string fileName = await camera.CaptureImageAsync();
            Console.WriteLine(string.Format(CultureInfo.InvariantCulture, "Image captured and stored on the camera: {0}", fileName));
        }
        
        #endregion
    }
}