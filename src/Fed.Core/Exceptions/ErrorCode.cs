namespace Fed.Core.Exceptions
{
    public enum ErrorCode
    {
        QuantityCannotBeZero = 2000,

        DateOutOfRange = 3000,
        IvalidDateRange = 3010,
        InvalidHour = 3020,
        InvalidTimeRange = 3030,

        InvalidTotalCapacity = 4000,
        InvalidQuantty = 4010,
        InvalidWeeklyRecurrence = 4020,
        NullOrEmptyName = 4030,
        PastCutOff = 4040,
        InvalidOperationForOneOffOrders = 4050,

        DuplicateCompanyName = 5000,
        DuplicateEmailAddress = 5001,
        OrderHasNoItems = 6000,

        OrderNotFound = 7000,
        OrderDoesNotContainProduct = 7001,
        ProductAlreadyShortened = 7002,
        ShortenedQuantityMustBeLessThanOrderedQuantity = 7003,
        MissingReasonForShortage = 7004,
        ProductSubstituteAlreadyAdded = 7005,
        DeliveryShortageDoesNotExist = 7010,
        DeliveryAdditionDoesNotExist = 7011,

        InvalidOperationForDeletedAccount = 8000,

        ProductDoesNotExist = 9000,
        ProductHasBeenDiscontinued = 9001
    }
}