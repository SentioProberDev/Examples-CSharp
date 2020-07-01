namespace SentioSteppingTcpIp
{
    public enum RemoteCommandResult : uint
    {
        /// <summary>
        /// There was no error
        /// </summary>
        NoError = 0,

        /// <summary>
        /// Internal application error
        /// </summary>
        InternalError = 1,

        /// <summary>
        /// Generic error code for execution error
        /// </summary>
        ExecutionError = 2,

        /// <summary>
        /// The command handler was not found
        /// </summary>
        CommandHandlerNotFound = 3,

        InvalidCommand = 4,

        InvalidCommandFormat = 5,

        /// <summary>
        /// One of the parameters was invalid
        /// </summary>
        InvalidParameter = 6,

        /// <summary>
        /// Invalid number of parameters
        /// </summary>
        InvalidNumberOfParameters = 7,

        ArgumentOutOfBounds = 8,

        /// <summary>
        /// File was not found
        /// </summary>
        FileNotFound = 9,

        InvalidFileFormat = 10,

        EndOfRoute = 11,

        InvalidOperation = 12,

        NotSupported = 13,

        SubsiteNotRoutable = 14,

        /// <summary>
        /// This error is used when the underlying functionality of a remote commmand requires a project.
        /// added in Sentio 3.3 (2020-03-12)
        /// </summary>
        ProjectRequired = 15,

        /// <summary>
        /// The Stepping parameter error,
        /// e.g. Probe Card need both OnTheFly Lateral/Vertical
        /// </summary>
        SteppingCompensationParameterFailed = 16,

        PrealignmentFailed = 17,

        Timeout = 19,

        /// <summary>
        /// A required pattern is not trained
        /// </summary>
        PatternNotTrained = 20,

        /// <summary>
        /// The pattern could not be found
        /// </summary>
        PatternNotFound = 21,

        /// <summary>
        /// Too many instances of a pattern were found
        /// </summary>
        TooManyPatternsFound = 22,

        /// <summary>
        /// Returned when the status of an async command is polled with query_command_status and the command is Running
        /// </summary>
        CommandPending = 30,

        /// <summary>
        /// Returned when a async command was aborted prematurely
        /// </summary>
        AsyncCommandAborted = 31,

        /// <summary>
        /// Returned when an async command i queried but SENTIO does not know anything about this command id
        /// </summary>
        UnknownCommandId = 32,

        /// <summary>
        /// A camera required for a vision task is not calibrated
        /// </summary>
        CameraNotCalibrated = 35,

        /// <summary>
        /// A required camera is not installed in the system.
        /// </summary>
        CameraDoesNotExist = 36,

        /// <summary>
        /// Alignment accuracy over 10 µm
        /// </summary>
        AlignAccuracyBad = 37,

        /// <summary>
        /// The front load door is open
        /// </summary>
        FrontDoorOpen = 60,

        /// <summary>
        /// The side door is open
        /// </summary>
        LoaderDoorOpen = 61,

        /// <summary>
        /// Front door lock cannot be engaged
        /// </summary>
        FrontDoorLockFail = 62,

        /// <summary>
        /// Side door lock cannot be engaged
        /// </summary>
        LoaderDoorLockFail = 63,

        /// <summary>
        /// A slot or station that is the target of a wafer transfer is already occupied
        /// </summary>
        SlotOrStationOccupied = 64,

        /// <summary>
        /// A slot or station that is the origin of a wafer transfer does not have a wafer
        /// </summary>
        SlotOrStationEmpty = 65,

        // Loader error code, from 80 ~ 99
        LoaderCassetteDoesNotExist = 80,
        LoaderSlotNumberError = 81,
        LoaderPreAlignerAngleError = 83,
        LoaderNoWaferOnPrealigner = 85,
        LoaderNoWaferOnChuck = 86,
        LoaderNoWaferOnRobot = 88,
        LoaderNoIdReader = 90,
        LoaderReadIdFail = 91,

        // Probe Error Code, from 100 ~ 119
        ProbeNotInitialized = 100,
        ProbeServoOnOffFail = 101,

        // Setting Error Code, from 120 ~ 199
        OvertravelOutOfAxisLimit = 120,
        MissingTopographyTable = 121,


        SiPhMoveHoverFail = 300,
        SiPhMoveSeparationFail = 301,
        SiPhGradientSearchFail = 302,
        SiPhFastAlignFail = 303,
    }
}