using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;

namespace Paradox.Common.Parsers.pck
{
    public sealed class FileReaderEnumerable : TextReaderEnumerable
    {
        protected override bool CanCreateReader => true;

        private readonly string _filename;
        public FileReaderEnumerable(string filename)
        {
            if (null == filename) throw new ArgumentNullException(nameof(filename));
            if (0 == filename.Length) throw new ArgumentException("The filename must not be empty.", nameof(filename));
            this._filename = filename;
        }
        protected override TextReader CreateTextReader()
        {
            return File.OpenText(this._filename);
        }
    }
    public sealed class ConsoleReaderEnumerable : TextReaderEnumerable
    {
        protected override bool CanCreateReader => false;
        public ConsoleReaderEnumerable()
        {
        }
        protected override TextReader CreateTextReader()
        {
            return Console.In;
        }
    }
    public sealed class UrlReaderEnumerable : TextReaderEnumerable
    {
        protected override bool CanCreateReader => true;

        private readonly string _url;
        public UrlReaderEnumerable(string url)
        {
            if (null == url) throw new ArgumentNullException(nameof(url));
            if (0 == url.Length) throw new ArgumentException("The url must not be empty.", nameof(url));
            this._url = url;
        }
        protected override TextReader CreateTextReader()
        {
            var wq = WebRequest.Create(this._url);
            var wr = wq.GetResponse();
            return new StreamReader(wr.GetResponseStream());
        }
    }
    public abstract class TextReaderEnumerable : IEnumerable<char>
    {
        #region _OnceReaderEnumerable

        private sealed class OnceTextReaderEnumerable : TextReaderEnumerable
        {
            private TextReader _reader;
            internal OnceTextReaderEnumerable(TextReader reader)
            {
                this._reader = reader;
            }
            protected override TextReader CreateTextReader()
            {
                if (null == this._reader)
                    throw new NotSupportedException("This method can only be called once.");
                var r = this._reader;
                this._reader = null;
                return r;
            }
            protected override bool CanCreateReader => false;
        }
        #endregion
        public static TextReaderEnumerable FromReader(TextReader reader)
        {
            if (null == reader)
                throw new ArgumentNullException(nameof(reader));
            return new OnceTextReaderEnumerable(reader);
        }
        public IEnumerator<char> GetEnumerator()
        {
            return new TextReaderEnumerator(this);
        }
        protected abstract bool CanCreateReader { get; }
        protected abstract TextReader CreateTextReader();

        IEnumerator IEnumerable.GetEnumerator()
            => this.GetEnumerator();

        private sealed class TextReaderEnumerator : IEnumerator<char>
        {
            private readonly TextReaderEnumerable _outer;

            private TextReader _reader;

            private int _state;

            private char _current;
            internal TextReaderEnumerator(TextReaderEnumerable outer)
            {
                this._outer = outer;
                this._reader = null;
                if (this._outer.CanCreateReader)
                    this.Reset();
                else
                {
                    this._state = -1;
                    this._reader = this._outer.CreateTextReader(); // doesn't really recreate it
                }
            }

            public char Current
            {
                get
                {
                    switch (this._state)
                    {
                        case -3:
                            throw new ObjectDisposedException(this.GetType().Name);
                        case -2:
                            throw new InvalidOperationException("The cursor is past the end of input.");
                        case -1:
                            throw new InvalidOperationException("The cursor is before the start of input.");
                    }
                    return this._current;
                }
            }
            object IEnumerator.Current => this.Current;

            public void Dispose()
            {
                // Dispose of unmanaged resources.
                this._Dispose(true);
                // Suppress finalization.
                GC.SuppressFinalize(this);
            }
            ~TextReaderEnumerator()
            {
                this._Dispose(false);
            }
            // Protected implementation of Dispose pattern.
            private void _Dispose(bool disposing)
            {
                if (null == this._reader)
                    return;

                if (!disposing) return;
                this._reader.Close();
                this._reader = null;
                this._state = -3;

            }

            public bool MoveNext()
            {
                switch (this._state)
                {
                    case -3:
                        throw new ObjectDisposedException(this.GetType().Name);
                    case -2:
                        return false;
                }
                var i = this._reader.Read();
                if (-1 == this._state &&
                    ((BitConverter.IsLittleEndian && '\uFEFF' == i) ||
                        (!BitConverter.IsLittleEndian && '\uFFFE' == i))) // skip the byte order mark
                    i = this._reader.Read();
                this._state = 0;
                if (-1 == i)
                {
                    this._state = -2;
                    return false;
                }
                this._current = unchecked((char)i);
                return true;
            }

            public void Reset()
            {
                // don't bother if we haven't moved.
                if (-1 == this._state) return;
                try
                {

                    // optimization for streamreader.
                    if (this._reader is StreamReader sr && sr.BaseStream.CanSeek && 0L == sr.BaseStream.Seek(0, SeekOrigin.Begin))
                    {
                        this._state = -1;
                        return;
                    }
                }
                catch (IOException) { }
                if (!this._outer.CanCreateReader)
                    throw new NotSupportedException();
                this._Dispose(true);
                this._reader = this._outer.CreateTextReader();
                this._state = -1;
            }
        }
    }
}
