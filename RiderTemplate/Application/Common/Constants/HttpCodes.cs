namespace Application.Common.Constants;

public class HttpCodes
{
    public enum Forbidden
    {
        UserIsBlockedOrNotActive = 4031
    }

    public enum Conflict
    {
        ContractIsNotValid = 4091
    }
}
