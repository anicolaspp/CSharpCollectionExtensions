using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Security.Cryptography;

namespace Nikos.Extensions.Streams
{
    delegate void Execute<T>(IEnumerable<Action<T, int>> actions);
    public delegate void ProgressReportEventHandler(object sender, TFileReport args);

    public class TFileReport
    {
        public long Copied { get; internal set; }
        public long Length { get; internal set; }
    }

    public enum CheckSumType
    {
        Adler32, CRC32
    }


    public static class Files
    {
        ///<summary>
        /// The numbers of bytes into a Mega Byte
        ///</summary>
        public const int MB_SIZE = 1024 * 1024;

        /// <summary>
        /// The numbers of bytes into a Kb
        /// </summary>
        public const int KB_SIZE = 1024;

        private static void InvokeProgressReport(ProgressReportEventHandler report, object sender, TFileReport args)
        {
            if (report != null)
                report(sender, args);
        }

        ///<summary>
        /// Compare two Stream based in MD5 hash algorimth
        ///</summary>
        ///<param name="stream"></param>
        ///<param name="other"></param>
        ///<returns></returns>
        public static bool CompareHash(this Stream stream, Stream other)
        {
            MD5Cng cng = new MD5Cng();

            var data_1 = cng.ComputeHash(stream);
            var data_2 = cng.ComputeHash(other);

            if (data_1.Length != data_2.Length)
                return false;

            for (int i = 0; i < data_1.Length; i++)
                if (data_1[i] != data_2[i])
                    return false;

            return true;
        }

        public static bool CompareTo(this Stream stream, Stream other)
        {
            if (stream.Length != other.Length)
                return false;

            stream.Position = 0;
            other.Position = 0;

            while (stream.CanRead)
            {
                var x = stream.ReadByte();
                var y = other.ReadByte();

                if (x != y)
                    return false;
            }

            return true;
        }

        public static IAsyncResult BeginExecute(this Stream stream, Action<byte[], int> action, int bufferLength = 1024, AsyncCallback callback = null)
        {
            return stream.BeginExecute(new[] { action }, bufferLength, callback);
        }

        /// <summary>
        /// Execute each function on each segment of the stream
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="actions">Actions to applies</param>
        /// <param name="bufferLength">Size of buffer to get a segment of stream</param>
        /// <param name="callback">Call back funtion</param>
        /// <returns>Handler to async operation</returns>
        public static IAsyncResult BeginExecute(this Stream stream, IEnumerable<Action<byte[], int>> actions, int bufferLength = 1024, AsyncCallback callback = null)
        {
            Execute<byte[]> execute = x =>
                                          {
                                              var buff = new byte[bufferLength];
                                              while (stream.Position < stream.Length)
                                              {
                                                  int readers = stream.Read(buff, (int)stream.Position, bufferLength);
                                                  foreach (var action in x)
                                                      action(buff, readers);
                                                              
                                              }
                                          };

            return execute.BeginInvoke(actions, callback, execute);
        }

        ///<summary>
        /// Stop execution of the process on stream
        ///</summary>
        ///<param name="stream"></param>
        ///<param name="result"></param>
        public static void EndExecute(this Stream stream, IAsyncResult result)
        {
            if (!result.IsCompleted)
            {
                var execute = (Execute<byte[]>)result.AsyncState;
                execute.EndInvoke(result);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="source"></param>
        /// <param name="count">Cantidad en bytes</param>
        /// <param name="report"></param>
        public static void CopyFrom(this Stream stream, Stream source, long count, ProgressReportEventHandler report)
        {
            int bufferSize = MB_SIZE;
            byte[] buffer = new byte[bufferSize];
            int counts = 0, length = ((int)count / bufferSize) + 1;
            int readers = 1;
            long llevo = 0;

            while (llevo < count && readers > 0)
            {
                readers = source.Read(buffer, 0, bufferSize);
                if (llevo + readers <= count)
                {
                    stream.Write(buffer, 0, readers);
                    llevo += readers;
                    counts++;

                    InvokeProgressReport(report, stream, new TFileReport { Copied = counts, Length = length });
                }
                else
                {
                    var aux = new List<byte>();
                    foreach (var item in buffer)
                    {
                        if (llevo + aux.Count > count)
                            break;

                        aux.Add(item);
                    }
                    llevo += aux.Count;

                    stream.Write(aux.ToArray(), 0, aux.Count);
                }
            }

        }

        /// <summary>
        /// Compress stream using defleate algorithm
        /// </summary>
        /// <param name="stream">Stream to compress</param>
        /// <param name="output">Output streamm</param>
        /// <param name="count">Count of bytes to compress</param>
        /// <param name="offset">Offset on the output stream</param>
        /// <param name="report">Progress report handler</param>
        public static void CompressTo(this Stream stream, Stream output, long count, long offset = 0, ProgressReportEventHandler report = null)
        {
            if (output == null)
                throw new ArgumentNullException("output");

            output.Position = offset;

            using (DeflateStream deflateStream = new DeflateStream(output, CompressionMode.Compress, true))
            {
                deflateStream.CopyFrom(stream, count, report);
            }
        }

        /// <summary>
        /// Decompress stream using defleate algorithm
        /// </summary>
        /// <param name="stream">Stream to decompress</param>
        /// <param name="output">Output stream</param>
        /// <param name="count">Count of bytes to decompress</param>
        /// <param name="offset">Offset on the output stream</param>
        /// <param name="report">Progress report handler</param>
        public static void DeCompressTo(this Stream stream, Stream output, long count, long offset = 0, ProgressReportEventHandler report = null)
        {
            if (output == null)
                throw new ArgumentNullException("output");

            output.Position = offset;

            using (DeflateStream deflateStream = new DeflateStream(stream, CompressionMode.Decompress, true))
            {
                deflateStream.CopyTo(output);
            }

            InvokeProgressReport(report, stream, new TFileReport { Copied = count / 4096, Length = count / 4096 });
        }

        ///<summary>
        /// Checksum algorimth based on Adler32 and CRC32 algorimths
        ///</summary>
        ///<param name="stream">Stream to apply the algorimth</param>
        ///<param name="checkSumeType">Checksum type, Adler32 or CRC32</param>
        ///<returns>Checksum numeber</returns>
        ///<exception cref="InvalidOperationException"></exception>
        ///<exception cref="ArgumentOutOfRangeException"></exception>
        public static long CheckSum(this Stream stream, CheckSumType checkSumeType)
        {
            if (stream == null) 
                throw new ArgumentNullException("stream");
            if (!stream.CanRead)
                throw new InvalidOperationException("The stream not suported readings");

            ICheckSum checkSum;
            switch (checkSumeType)
            {
                case CheckSumType.Adler32: checkSum = new Adler32();
                    break;
                case CheckSumType.CRC32: checkSum = new Crc32();
                    break;
                default:
                    throw new ArgumentOutOfRangeException("checkSumeType");
            }

            lock (stream)
            {
                long tmp = stream.Position;
                stream.Position = 0;

                int readers;
                var buffer = new byte[MB_SIZE];

                while ((readers = stream.Read(buffer, 0, MB_SIZE)) > 0)
                {
                    checkSum.Update(buffer, 0, readers);
                }

                stream.Position = tmp;
            }

            return checkSum.Value;
        }
    }
}