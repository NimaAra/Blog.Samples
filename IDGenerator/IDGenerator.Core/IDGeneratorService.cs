namespace IDGenerator.Core
{
    using System;
    using System.Threading;

    internal static class IDGeneratorService
    {
        private const string Encode_32_Chars = "0123456789ABCDEFGHIJKLMNOPQRSTUV";
        private static long _lastId = DateTime.UtcNow.Ticks;

        private static readonly char[] _globalCharBuffer = new char[13];
        
        private static readonly ThreadLocal<char[]> _charBufferThreadLocal = 
            new ThreadLocal<char[]>(() => new char[13]);

        [ThreadStatic]
        private static char[] _charBufferThreadStatic;

        public static string GetNextIDUnsafe() => GenerateUnsafe(Interlocked.Increment(ref _lastId));
        public static string GetNextIDWithLocking() => GenerateWithLocking(Interlocked.Increment(ref _lastId));
        public static string GetNextIDThreadLocal() => GenerateThreadLocal(Interlocked.Increment(ref _lastId));
        public static string GetNextIDThreadStatic() => GenerateThreadStatic(Interlocked.Increment(ref _lastId));
        public static string GetNextIDNewBuffer() => GenerateNewBuffer(Interlocked.Increment(ref _lastId));
        public static string GetNextIDSpinLock() => GenerateSpinLock(Interlocked.Increment(ref _lastId));

        private static unsafe string GenerateUnsafe(long id)
        {
            char* buffer = stackalloc char[13];

            buffer[0] = Encode_32_Chars[(int)(id >> 60) & 31];
            buffer[1] = Encode_32_Chars[(int)(id >> 55) & 31];
            buffer[2] = Encode_32_Chars[(int)(id >> 50) & 31];
            buffer[3] = Encode_32_Chars[(int)(id >> 45) & 31];
            buffer[4] = Encode_32_Chars[(int)(id >> 40) & 31];
            buffer[5] = Encode_32_Chars[(int)(id >> 35) & 31];
            buffer[6] = Encode_32_Chars[(int)(id >> 30) & 31];
            buffer[7] = Encode_32_Chars[(int)(id >> 25) & 31];
            buffer[8] = Encode_32_Chars[(int)(id >> 20) & 31];
            buffer[9] = Encode_32_Chars[(int)(id >> 15) & 31];
            buffer[10] = Encode_32_Chars[(int)(id >> 10) & 31];
            buffer[11] = Encode_32_Chars[(int)(id >> 5) & 31];
            buffer[12] = Encode_32_Chars[(int)id & 31];

            return new string(buffer, 0, 13);
        }

        private static string GenerateWithLocking(long id)
        {
            lock (_globalCharBuffer)
            {
                var buffer = _globalCharBuffer;

                buffer[0] = Encode_32_Chars[(int)(id >> 60) & 31];
                buffer[1] = Encode_32_Chars[(int)(id >> 55) & 31];
                buffer[2] = Encode_32_Chars[(int)(id >> 50) & 31];
                buffer[3] = Encode_32_Chars[(int)(id >> 45) & 31];
                buffer[4] = Encode_32_Chars[(int)(id >> 40) & 31];
                buffer[5] = Encode_32_Chars[(int)(id >> 35) & 31];
                buffer[6] = Encode_32_Chars[(int)(id >> 30) & 31];
                buffer[7] = Encode_32_Chars[(int)(id >> 25) & 31];
                buffer[8] = Encode_32_Chars[(int)(id >> 20) & 31];
                buffer[9] = Encode_32_Chars[(int)(id >> 15) & 31];
                buffer[10] = Encode_32_Chars[(int)(id >> 10) & 31];
                buffer[11] = Encode_32_Chars[(int)(id >> 5) & 31];
                buffer[12] = Encode_32_Chars[(int)id & 31];

                return new string(buffer, 0, buffer.Length);
            }
        }

        private static string GenerateThreadLocal(long id)
        {
            var buffer = _charBufferThreadLocal.Value;

            buffer[0] = Encode_32_Chars[(int)(id >> 60) & 31];
            buffer[1] = Encode_32_Chars[(int)(id >> 55) & 31];
            buffer[2] = Encode_32_Chars[(int)(id >> 50) & 31];
            buffer[3] = Encode_32_Chars[(int)(id >> 45) & 31];
            buffer[4] = Encode_32_Chars[(int)(id >> 40) & 31];
            buffer[5] = Encode_32_Chars[(int)(id >> 35) & 31];
            buffer[6] = Encode_32_Chars[(int)(id >> 30) & 31];
            buffer[7] = Encode_32_Chars[(int)(id >> 25) & 31];
            buffer[8] = Encode_32_Chars[(int)(id >> 20) & 31];
            buffer[9] = Encode_32_Chars[(int)(id >> 15) & 31];
            buffer[10] = Encode_32_Chars[(int)(id >> 10) & 31];
            buffer[11] = Encode_32_Chars[(int)(id >> 5) & 31];
            buffer[12] = Encode_32_Chars[(int)id & 31];

            return new string(buffer, 0, buffer.Length);
        }

        private static string GenerateThreadStatic(long id)
        {
            if (_charBufferThreadStatic is null)
            {
                _charBufferThreadStatic = new char[13];
            }

            var buffer = _charBufferThreadStatic;

            buffer[0] = Encode_32_Chars[(int)(id >> 60) & 31];
            buffer[1] = Encode_32_Chars[(int)(id >> 55) & 31];
            buffer[2] = Encode_32_Chars[(int)(id >> 50) & 31];
            buffer[3] = Encode_32_Chars[(int)(id >> 45) & 31];
            buffer[4] = Encode_32_Chars[(int)(id >> 40) & 31];
            buffer[5] = Encode_32_Chars[(int)(id >> 35) & 31];
            buffer[6] = Encode_32_Chars[(int)(id >> 30) & 31];
            buffer[7] = Encode_32_Chars[(int)(id >> 25) & 31];
            buffer[8] = Encode_32_Chars[(int)(id >> 20) & 31];
            buffer[9] = Encode_32_Chars[(int)(id >> 15) & 31];
            buffer[10] = Encode_32_Chars[(int)(id >> 10) & 31];
            buffer[11] = Encode_32_Chars[(int)(id >> 5) & 31];
            buffer[12] = Encode_32_Chars[(int)id & 31];

            return new string(buffer, 0, buffer.Length);
        }

        private static string GenerateNewBuffer(long id)
        {
            var buffer = new char[13];

            buffer[0] = Encode_32_Chars[(int)(id >> 60) & 31];
            buffer[1] = Encode_32_Chars[(int)(id >> 55) & 31];
            buffer[2] = Encode_32_Chars[(int)(id >> 50) & 31];
            buffer[3] = Encode_32_Chars[(int)(id >> 45) & 31];
            buffer[4] = Encode_32_Chars[(int)(id >> 40) & 31];
            buffer[5] = Encode_32_Chars[(int)(id >> 35) & 31];
            buffer[6] = Encode_32_Chars[(int)(id >> 30) & 31];
            buffer[7] = Encode_32_Chars[(int)(id >> 25) & 31];
            buffer[8] = Encode_32_Chars[(int)(id >> 20) & 31];
            buffer[9] = Encode_32_Chars[(int)(id >> 15) & 31];
            buffer[10] = Encode_32_Chars[(int)(id >> 10) & 31];
            buffer[11] = Encode_32_Chars[(int)(id >> 5) & 31];
            buffer[12] = Encode_32_Chars[(int)id & 31];

            return new string(buffer, 0, buffer.Length);
        }

        private static readonly char[] _globalCharBuffer2 = new char[13];
        private static SpinLock _spinner = new SpinLock(false);
        private static string GenerateSpinLock(long id)
        {
            bool lockTaken = false;
            try
            {
                _spinner.Enter(ref lockTaken);

                var buffer = _globalCharBuffer2;

                buffer[0] = Encode_32_Chars[(int)(id >> 60) & 31];
                buffer[1] = Encode_32_Chars[(int)(id >> 55) & 31];
                buffer[2] = Encode_32_Chars[(int)(id >> 50) & 31];
                buffer[3] = Encode_32_Chars[(int)(id >> 45) & 31];
                buffer[4] = Encode_32_Chars[(int)(id >> 40) & 31];
                buffer[5] = Encode_32_Chars[(int)(id >> 35) & 31];
                buffer[6] = Encode_32_Chars[(int)(id >> 30) & 31];
                buffer[7] = Encode_32_Chars[(int)(id >> 25) & 31];
                buffer[8] = Encode_32_Chars[(int)(id >> 20) & 31];
                buffer[9] = Encode_32_Chars[(int)(id >> 15) & 31];
                buffer[10] = Encode_32_Chars[(int)(id >> 10) & 31];
                buffer[11] = Encode_32_Chars[(int)(id >> 5) & 31];
                buffer[12] = Encode_32_Chars[(int)id & 31];

                return new string(buffer, 0, buffer.Length);
            } finally
            {
                if (lockTaken)
                {
                    _spinner.Exit();
                }
            }
        }
    }
}