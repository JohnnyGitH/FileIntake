using System;

namespace FileIntake.Exceptions;
/// <summary>
/// Represents errors that occur during file processing operations,
/// such as PDF parsing failures, unreadable file content,
/// or downstream storage issues.
/// </summary>
public class FileProcessingException : Exception
{
    /// <summary>
    /// Creates a new <see cref="FileProcessingException"/> with a descriptive message.
    /// </summary>
    public FileProcessingException(string message) : base(message)
    {
    }

    /// <summary>
    /// Creates a new <see cref="FileProcessingException"/> with a message and inner exception.
    /// </summary>
    public FileProcessingException(string message, Exception? inner = null) : base(message, inner)
    {
    }
} 