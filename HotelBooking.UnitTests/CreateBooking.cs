using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using Xunit;
using HotelBooking.Core;

namespace HotelBooking.UnitTests
{
    public class CreateBooking
    {

        [Fact]
        public async Task CreateBooking_RoomAvailable_SetsRoom_IsActive_PersistsAndReturnsTrue()
        {
            // Arrange
            var start = DateTime.Today.AddDays(2);
            var end   = DateTime.Today.AddDays(3);

            var booking = new Booking
            {
                StartDate = start,
                EndDate   = end
            };

            // Mock repositories used by BookingManager
            var bookingRepo = new Mock<IRepository<Booking>>(MockBehavior.Strict);
            var roomRepo    = new Mock<IRepository<Room>>(MockBehavior.Strict);

            // No conflicting bookings
            bookingRepo.Setup(r => r.GetAllAsync())
                    .ReturnsAsync(new List<Booking>());

            // Provide rooms (at least 1)
            roomRepo.Setup(r => r.GetAllAsync())
                    .ReturnsAsync(new List<Room> { new Room { Id = 1 } });

            // Expect AddAsync called with the *mutated* instance
            bookingRepo.Setup(r => r.AddAsync(It.Is<Booking>(b =>
                    ReferenceEquals(b, booking) &&
                    b.IsActive == true &&
                    b.RoomId   >= 0 &&
                    b.StartDate == start &&
                    b.EndDate   == end
                )))
                .Returns(Task.CompletedTask)
                .Verifiable();

            var sut = new BookingManager(bookingRepo.Object, roomRepo.Object);

            // Act
            var result = await sut.CreateBooking(booking);

            // Assert
            Assert.True(result);
            Assert.True(booking.IsActive);
            Assert.True(booking.RoomId >= 0);

            // Explicitly verify expected calls:
            bookingRepo.Verify(r => r.GetAllAsync(), Times.AtLeastOnce());
            bookingRepo.Verify(r => r.AddAsync(It.IsAny<Booking>()), Times.Once);
            roomRepo.Verify(r => r.GetAllAsync(), Times.AtLeastOnce());
        }

        [Fact]
        public async Task CreateBooking_NoRoomAvailable_DoesNotPersistAndReturnsFalse()
        {
            // Arrange
            var start = DateTime.Today.AddDays(2);
            var end   = DateTime.Today.AddDays(3);

            var booking = new Booking { StartDate = start, EndDate = end };

            var bookingRepo = new Mock<IRepository<Booking>>(MockBehavior.Strict);
            var roomRepo    = new Mock<IRepository<Room>>(MockBehavior.Strict);

            // Provide rooms that are all "taken" by a conflicting booking,
            // or simply provide zero rooms. Here’s the minimal approach:
            roomRepo.Setup(r => r.GetAllAsync())
                    .ReturnsAsync(new List<Room>()); // no rooms available

            // Service may check existing bookings:
            bookingRepo.Setup(r => r.GetAllAsync())
                    .ReturnsAsync(new List<Booking>());

            // Expect: AddAsync should NOT be called
            // (no setup for AddAsync; we'll verify Times.Never)

            var sut = new BookingManager(bookingRepo.Object, roomRepo.Object);

            // Act
            var result = await sut.CreateBooking(booking);

            // Assert
            Assert.False(result);
            Assert.False(booking.IsActive);    // should not be toggled true
            Assert.True(booking.RoomId == 0 || booking.RoomId < 0); // unchanged/default

            bookingRepo.Verify(r => r.GetAllAsync(), Times.AtLeastOnce());
            bookingRepo.Verify(r => r.AddAsync(It.IsAny<Booking>()), Times.Never);
            roomRepo.Verify(r => r.GetAllAsync(), Times.AtLeastOnce());
        }
    }
}

