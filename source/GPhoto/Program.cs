
#region Using Directives

using System;
using System.Runtime.InteropServices;
using GPhoto.Interop;

#endregion

namespace GPhoto
{
    /// <summary>
    /// Represents a sample application to show-case the GPhoto2.NET library.
    /// </summary>
    public static class Program
    {
        #region Public Static Methods

        /// <summary>
        /// Represents the entrypoint to the sample application.
        /// </summary>
        public static void Main()
        {
            // Creates a new port info list
            ErrorCode result;
            result = PortInfoListInterop.gp_port_info_list_new(out IntPtr portInfoList);
            if (result != ErrorCode.Okay)
                Console.WriteLine("The port info list could not be created.");

            // Populates the port info list with the information about all ports in the system
            result = PortInfoListInterop.gp_port_info_list_load(portInfoList);
            if (result != ErrorCode.Okay)
                Console.WriteLine("The ports could not be enumerated.");

            // Cycles through all ports and prints out their names
            int numberOfPortInfos = PortInfoListInterop.gp_port_info_list_count(portInfoList);
            for (int portInfoIndex = 0; portInfoIndex < numberOfPortInfos; portInfoIndex++)
            {
                result = PortInfoListInterop.gp_port_info_list_get_info(portInfoList, portInfoIndex, out IntPtr portInfo);
                if (result != ErrorCode.Okay)
                    Console.WriteLine("The port info could not be retrieved.");

                result = PortInfoInterop.gp_port_info_get_name(portInfo, out IntPtr name);
                if (result != ErrorCode.Okay)
                    Console.WriteLine("The name of the port could not be retrieved.");
                string portName = Marshal.PtrToStringAnsi(name);
                Console.WriteLine($"{portInfoIndex + 1}. {portName}");
            }

            // Frees the memory that was allocated for the port info list
            result = PortInfoListInterop.gp_port_info_list_free(portInfoList);
            if (result != ErrorCode.Okay)
                Console.WriteLine("The port info list could not be freed.");
        }

        #endregion
    }
}
