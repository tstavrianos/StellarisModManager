﻿namespace Stellaris.Data.Parsers.pck
{
    public enum ErrorLevel
    {
        Message = 0,
        Warning = 1,
        Error = 2
    }
    public interface IMessage
    {
        ErrorLevel ErrorLevel { get; }
        string Message { get; }
        int ErrorCode { get; }
        int Line { get; }
        int Column { get; }
        long Position { get; }
        string Filename { get; }
    }
}
