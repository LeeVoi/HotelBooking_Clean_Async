using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using Xunit;
using HotelBooking.Core;

namespace HotelBooking.UnitTests
{
    public class FindAvailableRoomTest
    {

        [Fact]
        public async Task FindAvailableRoom_RoomAvailable_ReturnsRoomId()
        {
            // Arrange
            var start = DateTime.Today.AddDays(2);
            var end = DateTime.Today.AddDays(3);

            // Mock repositories used by BookingManager
            var bookingRepo = new Mock<IRepository<Booking>>(MockBehavior.Strict);
            var roomRepo = new Mock<IRepository<Room>>(MockBehavior.Strict);

            // No conflicting bookings
            bookingRepo.Setup(r => r.GetAllAsync())
                    .ReturnsAsync(new List<Booking>());

            // Provide rooms (at least 1)
            roomRepo.Setup(r => r.GetAllAsync())
                    .ReturnsAsync(new List<Room> { new Room { Id = 1 } });

            var sut = new BookingManager(bookingRepo.Object, roomRepo.Object);

            // Act
            var result = await sut.FindAvailableRoom(start, end);

            // Assert
            Assert.True(result >= 0);

            // Explicitly verify expected calls:
            bookingRepo.Verify(r => r.GetAllAsync(), Times.AtLeastOnce());
            roomRepo.Verify(r => r.GetAllAsync(), Times.AtLeastOnce());
        }

        [Fact]
        public async Task FindAvailableRoom_NoRoomAvailable_ReturnsMinusOne()
        {
            // Arrange
            var start = DateTime.Today.AddDays(2);
            var end = DateTime.Today.AddDays(3);

            // Mock repositories used by BookingManager
            var bookingRepo = new Mock<IRepository<Booking>>(MockBehavior.Strict);
            var roomRepo = new Mock<IRepository<Room>>(MockBehavior.Strict);

            // Provide a conflicting booking for the only room
            bookingRepo.Setup(r => r.GetAllAsync())
                    .ReturnsAsync(new List<Booking> {
                        new Booking {
                            RoomId = 1,
                            StartDate = start,
                            EndDate = end,
                            IsActive = true
                        }
                    });

            // Provide rooms (at least 1)
            roomRepo.Setup(r => r.GetAllAsync())
                    .ReturnsAsync(new List<Room> { new Room { Id = 1 } });

            var sut = new BookingManager(bookingRepo.Object, roomRepo.Object);

            // Act
            var result = await sut.FindAvailableRoom(start, end);

            // Assert
            Assert.Equal(-1, result);

            // Explicitly verify expected calls:
            bookingRepo.Verify(r => r.GetAllAsync(), Times.AtLeastOnce());
            roomRepo.Verify(r => r.GetAllAsync(), Times.AtLeastOnce());
        }
        [Fact]
        public async Task FindAvailableRoom_StartDateInPast_ThrowsArgumentException()
        {
            // Arrange
            var start = DateTime.Today.AddDays(-1);
            var end = DateTime.Today.AddDays(3);

            // Mock repositories used by BookingManager
            var bookingRepo = new Mock<IRepository<Booking>>(MockBehavior.Strict);
            var roomRepo = new Mock<IRepository<Room>>(MockBehavior.Strict);

            var sut = new BookingManager(bookingRepo.Object, roomRepo.Object);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(async () =>
                await sut.FindAvailableRoom(start, end)
            );

            // No calls should have been made to the repositories
            bookingRepo.Verify(r => r.GetAllAsync(), Times.Never());
            roomRepo.Verify(r => r.GetAllAsync(), Times.Never());
        }
        [Fact]
        public async Task FindAvailableRoom_StartDateAfterEndDate_ThrowsArgumentException()
        {
            // Arrange
            var start = DateTime.Today.AddDays(4);
            var end = DateTime.Today.AddDays(3);

            // Mock repositories used by BookingManager
            var bookingRepo = new Mock<IRepository<Booking>>(MockBehavior.Strict);
            var roomRepo = new Mock<IRepository<Room>>(MockBehavior.Strict);

            var sut = new BookingManager(bookingRepo.Object, roomRepo.Object);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(async () =>
                await sut.FindAvailableRoom(start, end)
            );

            // No calls should have been made to the repositories
            bookingRepo.Verify(r => r.GetAllAsync(), Times.Never());
            roomRepo.Verify(r => r.GetAllAsync(), Times.Never());
        }
    }
}

