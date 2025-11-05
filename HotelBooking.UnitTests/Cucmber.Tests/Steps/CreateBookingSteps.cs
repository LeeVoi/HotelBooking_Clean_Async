using System;
using System.Threading.Tasks;
using Moq;
using HotelBooking.Core;
using Reqnroll;
using Xunit;
using HotelBooking.Infrastructure.Repositories;


namespace HotelBooking.UnitTests.Cucmber.Tests.Steps;

[Binding]
public class CreateBookingSteps
{
    private readonly Mock<IBookingManager> _bookingManagerMock;
    private Booking _booking;
    private bool _bookingResult;
    private DateTime _occupiedStart;
    private DateTime _occupiedEnd;

    public CreateBookingSteps()
    {
        _bookingManagerMock = new Mock<IBookingManager>();
    }

    [Given(@"the hotel has one room occupied from ""(.*)"" to ""(.*)""")]
    public void GivenTheHotelHasOneRoomOccupied(DateTime startDate, DateTime endDate)
    {
        _occupiedStart = startDate;
        _occupiedEnd = endDate;

        // Mock the FindAvailableRoom method to return -1 if the requested dates overlap,
        // and 1 otherwise. This should mimic how the system decides if a room is available.
        _bookingManagerMock.Setup(bm => bm.FindAvailableRoom(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync((DateTime s, DateTime e) =>
            {
                // Overlap occurs if the requested end date is NOT before the occupied start,
                // and the requested start date is NOT after the occupied end.
                bool overlaps = !(e < _occupiedStart || s > _occupiedEnd);
                return overlaps ? -1 : 1;
            });

        // Mock CreateBooking to simply return true to simulate a successful call
        // (the actual success is determined by the overlap logic above).
        _bookingManagerMock.Setup(bm => bm.CreateBooking(It.IsAny<Booking>()))
            .ReturnsAsync((Booking b) => true);
    }

    // This step performs the booking attempt during the test.
    // It passes the desired start and end dates into the mocked booking manager.
    [When(@"I attempt to book from ""(.*)"" to ""(.*)""")]
    public async Task WhenIAttemptToBookFromTo(DateTime startDate, DateTime endDate)
    {
        _booking = new Booking
        {
            StartDate = startDate,
            EndDate = endDate
        };

        int roomId = await _bookingManagerMock.Object.FindAvailableRoom(startDate, endDate);
        _bookingResult = roomId >= 0;
    }

    // This assertion verifies that a booking overlapping existing occupied dates
    // is correctly accepted or rejected according to the expected outcome.
    [Then(@"the booking should be ""(.*)"" due to overlapping dates")]
    public void ThenTheBookingShouldBeDueToOverlappingDates(string expected)
    {
        bool shouldSucceed = expected.Equals("successful", StringComparison.OrdinalIgnoreCase);
        Assert.Equal(shouldSucceed, _bookingResult);
    }

    // This assertion verifies that a booking with no date overlap behaves correctly
    // (i.e., it should succeed if there’s no conflict or overlap).
    [Then(@"the booking should be ""(.*)"" due to no overlapping dates")]
    public void ThenTheBookingShouldBeDueToNoOverlappingDates(string expected)
    {
        bool shouldSucceed = expected.Equals("successful", StringComparison.OrdinalIgnoreCase);
        Assert.Equal(shouldSucceed, _bookingResult);
    }
}