using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nikos.Extensions.Streams
{
    interface ICheckSum
    {
        void Reset();
        void Update(int value);
        void Update(byte[] buffer);
        void Update(byte[] buffer, int offset, int count);

        long Value { get; }
    }
}
