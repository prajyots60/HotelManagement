namespace ReservationService.Authorization;

public static class ReservationPolicies
{
    public const string GuestsRead = "GuestsRead";
    public const string GuestsManage = "GuestsManage";
    public const string GuestsDelete = "GuestsDelete";
    public const string ReservationsRead = "ReservationsRead";
    public const string ReservationsManage = "ReservationsManage";
    public const string RoomAvailabilityRead = "RoomAvailabilityRead";
}
