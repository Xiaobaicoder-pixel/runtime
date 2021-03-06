// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Threading;

namespace System.Net.Http
{
    internal abstract class HttpContentStream : HttpBaseStream
    {
        protected HttpConnection? _connection;

        // Makes sure we don't call HttpTelemetry events more than once.
        private int _requestStopCalled; // 0==no, 1==yes

        public HttpContentStream(HttpConnection connection)
        {
            _connection = connection;
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            Write(new ReadOnlySpan<byte>(buffer, offset, count));
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_connection != null)
                {
                    _connection.Dispose();
                    _connection = null;
                }
            }

            base.Dispose(disposing);
        }

        protected HttpConnection GetConnectionOrThrow()
        {
            return _connection ??
                // This should only ever happen if the user-code that was handed this instance disposed of
                // it, which is misuse, or held onto it and tried to use it later after we've disposed of it,
                // which is also misuse.
                ThrowObjectDisposedException();
        }

        protected void LogRequestStop()
        {
            if (Interlocked.Exchange(ref _requestStopCalled, 1) == 0)
            {
                HttpTelemetry.Log.RequestStop();
            }
        }

        private HttpConnection ThrowObjectDisposedException() => throw new ObjectDisposedException(GetType().Name);
    }
}
