using System;
using System.Collections.Generic;
using System.Text;
using Autodesk.AutoCAD.DatabaseServices;

namespace SpeckleAutoCAD.Helpers
{
    public static class StringExtensions
    {
        public static ResultBuffer ToResultBuffer(this string s)
        {
            const int maxChunkSize = 50000;
            int chunkSize;

            var resultBuffer = new ResultBuffer();
            int startIndex = 0;
            while (startIndex < s.Length)
            {
                chunkSize = s.Length - startIndex;
                if (chunkSize > maxChunkSize)
                {
                    chunkSize = maxChunkSize;
                }

                resultBuffer.Add(new TypedValue((int)DxfCode.Text, s.Substring(startIndex, chunkSize)));
                startIndex += chunkSize;
            }
            return resultBuffer;
        }
    }
}
