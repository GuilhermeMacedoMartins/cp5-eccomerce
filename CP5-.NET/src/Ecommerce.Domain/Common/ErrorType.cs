namespace Ecommerce.Domain.Common;

public enum ErrorType
{
    Validation = 0,
    NotFound = 1,
    Conflict = 2,
    Unauthorized = 3,
    External = 4,
    Unexpected = 5
}
