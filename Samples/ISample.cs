
#region Using Directives

using System.Devices;
using System.Threading.Tasks;

#endregion

namespace SamplesApplication
{
    /// <summary>
    /// Represents the interface that all samples have to implement in order to the recognized as samples.
    /// </summary>
    public interface ISample
    {
        #region Properties
        
        /// <summary>
        /// Gets the title of the sample.
        /// </summary>
        string Title { get; }
        
        #endregion
        
        #region Methods
        
        /// <summary>
        /// Executes the sample.
        /// </summary>
        /// <param name="camera">The camera with which the sample is to be executed.</param>
        Task ExecuteAsync(Camera camera);
        
        #endregion
    }
}