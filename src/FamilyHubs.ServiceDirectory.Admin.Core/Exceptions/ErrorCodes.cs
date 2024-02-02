namespace FamilyHubs.ServiceDirectory.Admin.Core.Exceptions;

public enum ErrorCodes
{
    UnhandledException,
    GenericAuthorizationException,
    AlreadyExistsException
}

public static class ErrorCodesExtensions
{
    private const string UnhandledException = "FH0001";
    private const string GenericAuthorizationException = "FH0002";
    private const string AlreadyExistsException = "FH0003";

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

        //todo: not the best exception to throw here
        throw new InvalidOperationException("ErrorCode does not exist");
    }

    public static string ParseToCodeString(this ErrorCodes code)
    {
        return code switch
        {
            ErrorCodes.UnhandledException => UnhandledException,
            ErrorCodes.GenericAuthorizationException => GenericAuthorizationException,
            ErrorCodes.AlreadyExistsException => AlreadyExistsException,
            _ => throw new InvalidOperationException("ErrorCode does not exist")
        };
    }
}

