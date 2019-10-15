namespace TuringMachine.Core.Fuzzers
{
    public enum EFuzzerMessageType : byte
    {
        /// <summary>
        /// Get Available configurations
        /// </summary>
        GetAvailableConfigs = 0x01,

        /// <summary>
        /// Get Available configurations
        /// </summary>
        AvailableConfigs = 0x02,

        /// <summary>
        /// Get available inputs
        /// </summary>
        GetAvailableInputs = 0x03,

        /// <summary>
        /// Get available inputs
        /// </summary>
        AvailableInputs = 0x04,

        /// <summary>
        /// Push public name
        /// </summary>
        PushPublicName = 0x10,

        /// <summary>
        /// Log
        /// </summary>
        PushLog = 0x20,

        /// <summary>
        /// Log with error
        /// </summary>
        PushLogWithError = 0x21,

        /// <summary>
        /// GoodBye
        /// </summary>
        GoodByte = 0xFF
    }
}
