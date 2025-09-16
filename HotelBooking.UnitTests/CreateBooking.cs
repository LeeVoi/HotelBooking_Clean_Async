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

            // Here whenever the BookingManager akes for all bookings, we return an empty list
            // No conflicting bookings
            bookingRepo.Setup(r => r.GetAllAsync())
                    .ReturnsAsync(new List<Booking>());

            // Provide at least one available room (roomId = 1)
            roomRepo.Setup(r => r.GetAllAsync())
                    .ReturnsAsync(new List<Room> { new Room { Id = 1 } });

            // Expect AddAsync called with the *mutated* instance of the booking
            bookingRepo.Setup(r => r.AddAsync(It.Is<Booking>(b =>
                    ReferenceEquals(b, booking) &&
                    b.IsActive == true &&
                    b.RoomId   == 1 &&
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
            roomRepo.Verify(r => r.GetAllAsync(), Times.Once());
            bookingRepo.Verify(r => r.GetAllAsync(), Times.Once());
            bookingRepo.Verify(r => r.AddAsync(It.IsAny<Booking>()), Times.Once);
            

            // Assert no other calls should have been made
            bookingRepo.VerifyNoOtherCalls();
            roomRepo.VerifyNoOtherCalls();
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


            // Provide no available rooms
            roomRepo.Setup(r => r.GetAllAsync())
                    .ReturnsAsync(new List<Room>()); // no rooms available


            // Service may check existing bookings:
            bookingRepo.Setup(r => r.GetAllAsync())
                    .ReturnsAsync(new List<Booking>());

            // Expect: AddAsync should NOT be called
            // (no setup for AddAsync; we verify Times.Never later)

            var sut = new BookingManager(bookingRepo.Object, roomRepo.Object);

            // Act
            var result = await sut.CreateBooking(booking);

            // Assert
            Assert.False(result);
            Assert.False(booking.IsActive);    // should not be toggled true
            Assert.True(booking.RoomId == 0); // unchanged/default

            roomRepo.Verify(r => r.GetAllAsync(), Times.Once());
            bookingRepo.Verify(r => r.GetAllAsync(), Times.Once());
            bookingRepo.Verify(r => r.AddAsync(It.IsAny<Booking>()), Times.Never); // Verify AddAsync was never called

            // Assert no other calls should have been made
            bookingRepo.VerifyNoOtherCalls();
            roomRepo.VerifyNoOtherCalls();
        }
    }
}

