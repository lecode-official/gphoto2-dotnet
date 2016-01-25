
namespace System.Devices
{
    /// <summary>
    /// Represents a container for identifiers of well-known camera configurations.
    /// </summary>
    public static class CameraConfigurations
    {
        #region Public Constants
        
        /// <summary>
        /// Contains the name of the camera configuration for the date and time to which the camera is set.
        /// </summary>
        public const string DateTime = "/main/settings/datetime";
        
        /// <summary>
        /// Contains the name of the camera configuration for the name of the camera owner.
        /// </summary>
        public const string OwnerName = "/main/settings/ownername";
        
        /// <summary>
        /// Contains the name of the camera configuration for the copyright notice, which is added to the meta data of each
        /// photo that is taken by the camera.
        /// </summary>
        public const string Copyright = "/main/settings/copyright";
        
        /// <summary>
        /// Contains the name of the camera configuration for the serial number of the camera.
        /// </summary>
        public const string SerialNumber = "/main/status/serialnumber";
        
        /// <summary>
        /// Contains the name of the camera configuration for the manufacturer of the camera.
        /// </summary>
        public const string Manufacturer = "/main/status/manufacturer";
        
        /// <summary>
        /// Contains the name of the camera configuration for the camera model.
        /// </summary>
        public const string CameraModel = "/main/status/cameramodel";
        
        /// <summary>
        /// Contains the name of the camera configuration for the device version of the camera.
        /// </summary>
        public const string DeviceVersion = "/main/status/deviceversion";
        
        /// <summary>
        /// Contains the name of the camera configuration for the textual representation of the battery level of the camera.
        /// </summary>
        public const string BatteryLevel = "/main/status/batterylevel";
        
        /// <summary>
        /// Contains the name of the camera configuration for the name of the lens that is currently attached to the camera.
        /// </summary>
        public const string LensName = "/main/status/lensname";
        
        /// <summary>
        /// Contains the name of the camera configuration for the image format in which the camera stores pictures.
        /// </summary>
        public const string ImageFormat = "/main/status/imageformat";
        
        /// <summary>
        /// Contains the name of the camera configuration for the ISO speed of the camera.
        /// </summary>
        public const string IsoSpeed = "/main/imgsettings/iso";
        
        /// <summary>
        /// Contains the name of the camera configuration for the white balance of the camera.
        /// </summary>
        public const string WhiteBalance = "/main/imgsettings/whitebalance";
        
        /// <summary>
        /// Contains the name of the camera configuration for the color space of the camera.
        /// </summary>
        public const string ColorSpace = "/main/imgsettings/colorspace";
        
        /// <summary>
        /// Contains the name of the camera configuration for the exposure compensation of the camera.
        /// </summary>
        public const string ExposureCompensation = "/main/imgsettings/exposurecompensation";
        
        /// <summary>
        /// Contains the name of the camera configuration for the shutter speed of the camera.
        /// </summary>
        public const string ShutterSpeed = "/main/capturesettings/shutterspeed";
        
        /// <summary>
        /// Contains the name of the camera configuration for the aperture of the camera.
        /// </summary>
        public const string Aperture = "/main/capturesettings/aperture";
        
        /// <summary>
        /// Contains the name of the camera configuration for the focus mode of the camera.
        /// </summary>
        public const string FocusMode = "/main/capturesettings/focusmode";
        
        /// <summary>
        /// Contains the name of the camera configuration for the drive mode of the camera.
        /// </summary>
        public const string DriveMode = "/main/capturesettings/drivemode";
        
        /// <summary>
        /// Contains the name of the camera configuration for the picture style of the camera.
        /// </summary>
        public const string PictureStyle = "/main/capturesettings/picturestyle";
        
        /// <summary>
        /// Contains the name of the camera configuration for the metering mode of the camera.
        /// </summary>
        public const string MeteringMode = "/main/capturesettings/meteringmode";
        
        /// <summary>
        /// Contains the name of the camera configuration for the bracket mode of the camera.
        /// </summary>
        public const string BracketMode = "/main/capturesettings/bracketmode";
        
        /// <summary>
        /// Contains the name of the camera configuration for the auto exposure bracketing of the camera.
        /// </summary>
        public const string AutoExposureBracketing = "/main/capturesettings/aeb";
        
        #endregion
    }
}