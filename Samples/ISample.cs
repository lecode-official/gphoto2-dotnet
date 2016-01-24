
#region Using Directives

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
        Task ExecuteAsync();
        
        #endregion
    }
}