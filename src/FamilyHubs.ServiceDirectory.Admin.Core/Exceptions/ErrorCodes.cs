namespace FamilyHubs.ServiceDirectory.Admin.Core.Exceptions;

public enum ErrorCodes
{
    UnhandledException,
    GenericAuthorizationException,
    AlreadyExistsException
}

public static class ErrorCodesParser
{
    public static ErrorCodes Parse(string code)
    {
        switch (code)
        {
            case "FH0001":
                return ErrorCodes.UnhandledException;

            case "FH0002":
                return ErrorCodes.GenericAuthorizationException;

            case "FH0003":
                return ErrorCodes.AlreadyExistsException;
        }

        throw new Exception("ErrorCode does not exist");
    }
}

