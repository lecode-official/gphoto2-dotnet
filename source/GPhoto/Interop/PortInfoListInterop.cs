
#region Using Directives

using System;
using System.Runtime.InteropServices;

#endregion

namespace GPhoto.Interop
{
    /// <summary>
    /// Represents a class that contains the interop code for the native functions for managing port info lists. Port info lists are lists that
    /// contain port info objects, which themselves represent information about the port (e.g., USB, serial port, IP, etc.) via which devices (cameras
    /// as well as other devices) are connected to the system.
    /// </summary>
    internal static class PortInfoListInterop
    {
        #region Public Static Methods

        /// <summary>
        /// Creates a new list which can later be filled with port infos.
        /// </summary>
        /// <param name="list">A pointer to the port info list that will be allocated.</param>
        /// <returns>Returns a gPhoto2 error code.</returns>
        [DllImport("gphoto2")]
        public static extern ErrorCode gp_port_info_list_new(out IntPtr list);

        /// <summary>
        /// Searches the system for IO-drivers and appends them to the list. You would normally call this function once after
        /// <see cref="gp_port_info_list_new"/> and then use this list in order to supply <see cref="gp_port_set_info"/> with parameters or to do
        /// autodetection.
        /// </summary>
        /// <param name="list">A port info list that was previously created using <see cref="gp_port_info_list_new"/>.</param>
        /// <returns>Returns a gPhoto2 error code.</returns>
        [DllImport("gphoto2")]
        public static extern ErrorCode gp_port_info_list_load(IntPtr list);

        /// <summary>
        /// Frees a port info list structure and its internal data structures.
        /// </summary>
        /// <param name="list">The port info list that is to be freed.</param>
        /// <returns>Returns a gPhoto2 error code.</returns>
        [DllImport("gphoto2")]
        public static extern ErrorCode gp_port_info_list_free(IntPtr list);

        /// <summary>
        /// Retrieves the number of entries in the passed list.
        /// </summary>
        /// <param name="list">The port info list for which the number of entries is to be retrieved.</param>
        /// <returns>Returns the number of entries or a gPhoto2 error code</returns>
        [DllImport("gphoto2")]
        public static extern int gp_port_info_list_count(IntPtr list);

        /// <summary>
        /// Retrieves the port information of specific entry of the port info list.
        /// </summary>
        /// <param name="list">A port info list from which the specified entry is to be retrieved.</param>
        /// <param name="n">The index of the entry that is to be retrieved.</param>
        /// <param name="info">A pointer to a port info, which will receive the port info of the entry.</param>
        /// <returns>Returns a gPhoto2 error code.</returns>
        [DllImport("gphoto2")]
        public static extern ErrorCode gp_port_info_list_get_info(IntPtr list, int n, out IntPtr info);

        #endregion
    }
}
