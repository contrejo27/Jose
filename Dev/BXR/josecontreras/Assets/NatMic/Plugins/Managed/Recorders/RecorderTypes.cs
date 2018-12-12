/* 
*   NatMic
*   Copyright (c) 2018 Yusuf Olokoba
*/

namespace NatMicU.Core.Recorders {

    using Docs;

    #region --Delegates--
    /// <summary>
    /// Delegate used to provide path to a recorded audio file
    /// </summary>
    [Doc(@"RecordingCallback")]
    public delegate void RecordingCallback (string recordingPath);
    #endregion
}