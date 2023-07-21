namespace FamilyHubs.ServiceDirectory.Admin.Core
{
    /// <summary>
    /// Outcome object for when Result data is only needed for either success or failure
    /// </summary>
    /// <typeparam name="TData">Object to be returned</typeparam>
    public class Outcome<TData> : Outcome<TData,TData>
    {
        public Outcome(bool isSuccess) 
        {
            IsSuccess = isSuccess;
        }
        
        public Outcome(TData data, bool isSuccess)
        {
            
            IsSuccess = isSuccess;
            if (IsSuccess)
            {
                SuccessResult = data;
            }
            else
            {
                FailureResult = data;
            }
        }
    }

    /// <summary>
    /// Outcome object for when the result is different depending on if the method has succeeded or failed
    /// </summary>
    /// <typeparam name="TSuccess">Object to be returned on success</typeparam>
    /// <typeparam name="TFailure">Object to be returned of failure</typeparam>
    public class Outcome<TSuccess, TFailure>
    {
        public TSuccess? SuccessResult { get; set; }
        public TFailure? FailureResult { get; set; }
        public bool IsSuccess { get; set; }

        public Outcome()
        {

        }

        public Outcome(TSuccess success)
        {
            SuccessResult = success;
            IsSuccess = true;
        }

        public Outcome(TFailure failure)
        {
            FailureResult = failure;
            IsSuccess = false;
        }
    }
}
