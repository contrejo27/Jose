/* 
*   NatMic
*   Copyright (c) 2018 Yusuf Olokoba
*/

namespace NatMicU.Utilities {

    using UnityEngine;
    using System;
    using System.Runtime.CompilerServices;

    public sealed class CircularBuffer {

        #region --Op vars--
        private int totalWriteCount, totalReadCount;
        private int writeIndex, readIndex;
        private int writeRemaining { get { return buffer.Length - writeIndex; }}
        private int readRemaining { get { return buffer.Length - readIndex; }}
        private readonly float[] buffer;
        #endregion

        
        #region --Client API--

        public CircularBuffer (int size = 1 << 16) {
            buffer = new float[size];
        }

        [MethodImpl(MethodImplOptions.Synchronized)] // yikes
        public void Write (float[] sampleBuffer) {
            var writeCount = Mathf.Min(sampleBuffer.Length, writeRemaining);
            Array.Copy(sampleBuffer, 0, buffer, writeIndex, writeCount);
            writeIndex += writeCount;
            var residualCount = sampleBuffer.Length - writeCount;
            if (residualCount > 0) {
                Array.Copy(sampleBuffer, writeCount, buffer, 0, residualCount);
                writeIndex = residualCount;
            }
            totalWriteCount += sampleBuffer.Length;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void Read (ref float[] dst, int count) {
            if (dst != null && dst.Length != count) dst = null;
            dst = dst ?? new float[count];
            var readCount = Mathf.Min(dst.Length, readRemaining);
            Array.Copy(buffer, readIndex, dst, 0, readCount);
            readIndex += readCount;
            var residualCount = dst.Length - readCount;
            if (residualCount > 0) {
                Array.Copy(buffer, 0, dst, readCount, residualCount);
                readIndex = residualCount;
            }
            totalReadCount += count;
        }

        public int AvailableSamples {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get {
                return totalWriteCount - totalReadCount;
            }
        }
        #endregion
    }
}