using System;

namespace FileIntake.Exceptions;

public class FileProcessingException : Exception
{
    public FileProcessingException()
    {
    }

    public FileProcessingException(string message) : base(message)
    {
    }

    public FileProcessingException(string message, Exception innerException) : base(message, innerException)
    {
    }
}