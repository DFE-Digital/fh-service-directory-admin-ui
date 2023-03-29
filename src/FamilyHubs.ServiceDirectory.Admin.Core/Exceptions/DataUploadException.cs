namespace FamilyHubs.ServiceDirectory.Admin.Core.Exceptions;

public class DataUploadException : Exception
{
    public DataUploadException(string message) : base(message)
    {

    }

    public DataUploadException(string message, Exception innerException) : base(message, innerException)
    {

    }
}