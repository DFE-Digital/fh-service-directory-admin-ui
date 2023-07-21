namespace FamilyHubs.ServiceDirectory.Admin.Core.Exceptions;

public enum ErrorCodes
{
    UnhandledException,
    GenericAuthorizationException,
    AlreadyExistsException
}

public static class ErrorCodesExtensions
{
    public const string UnhandledException = "FH0001";
    public const string GenericAuthorizationException = "FH0002";
    public const string AlreadyExistsException = "FH0003";

    public static ErrorCodes ParseToErrorCode(this string code)
    {
        switch (code)
        {
            case UnhandledException:
                return ErrorCodes.UnhandledException;

            case GenericAuthorizationException:
                return ErrorCodes.GenericAuthorizationException;

            case AlreadyExistsException:
                return ErrorCodes.AlreadyExistsException;
        }

        throw new Exception("ErrorCode does not exist");
    }

    public static string ParseToCodeString(this ErrorCodes code)
    {
        switch (code)
        {
            case ErrorCodes.UnhandledException:
                return UnhandledException;

            case ErrorCodes.GenericAuthorizationException:
                return GenericAuthorizationException;

            case ErrorCodes.AlreadyExistsException:
                return AlreadyExistsException;
        }

        throw new Exception("ErrorCode does not exist");
    }
}

