
#region Using Directives

using System;
using System.Runtime.InteropServices;

#endregion

namespace GPhoto.Interop
{
    /// <summary>
    /// Represent information about the port (e.g., USB, serial port, IP, etc.) via which devices (cameras as well as other devices) are connected to
    /// the system.
    /// </summary>
    internal static class PortInfoInterop
    {
        #region Public Static Methods

        /// <summary>
        /// Retrieves the name of the passed-in port info.
        /// </summary>
        /// <param name="info">A pointer to the port info for which the name is to be retrieved.</param>
        /// <param name="name">A pointer to a character array, which will receive the name.</param>
        /// <returns>Returns a gPhoto2 error code.</returns>
        [DllImport("gphoto2")]
        public static extern ErrorCode gp_port_info_get_name(IntPtr info, out IntPtr name);

        #endregion
    }
}
