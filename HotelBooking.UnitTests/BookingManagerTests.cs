using System;
using System.Collections.Generic;
using HotelBooking.Core;
using HotelBooking.UnitTests.Fakes;
using Xunit;
using System.Linq;
using System.Threading.Tasks;
using Moq;


namespace HotelBooking.UnitTests
{
    public class BookingManagerTests
    {
        private IBookingManager bookingManager;
        IRepository<Booking> bookingRepository;

        public BookingManagerTests(){
            DateTime start = DateTime.Today.AddDays(10);
            DateTime end = DateTime.Today.AddDays(20);
            bookingRepository = new FakeBookingRepository(start, end);
            IRepository<Room> roomRepository = new FakeRoomRepository();
            bookingManager = new BookingManager(bookingRepository, roomRepository);
        }

        [Fact]
        public async Task FindAvailableRoom_StartDateNotInTheFuture_ThrowsArgumentException()
        {
            // Arrange
            DateTime date = DateTime.Today;

            // Act
            Task result() => bookingManager.FindAvailableRoom(date, date);

            // Assert
            await Assert.ThrowsAsync<ArgumentException>(result);
        }

        [Fact]
        public async Task FindAvailableRoom_RoomAvailable_RoomIdNotMinusOne()
        {
            // Arrange
            DateTime date = DateTime.Today.AddDays(1);
            // Act
            int roomId = await bookingManager.FindAvailableRoom(date, date);
            // Assert
            Assert.NotEqual(-1, roomId);
        }

        [Fact]
        public async Task FindAvailableRoom_RoomAvailable_ReturnsAvailableRoom()
        {
            // This test was added to satisfy the following test design
            // principle: "Tests should have strong assertions".

            // Arrange
            DateTime date = DateTime.Today.AddDays(1);
            
            // Act
            int roomId = await bookingManager.FindAvailableRoom(date, date);

            var bookingForReturnedRoomId = (await bookingRepository.GetAllAsync()).
                Where(b => b.RoomId == roomId
                           && b.StartDate <= date
                           && b.EndDate >= date
                           && b.IsActive);
            
            // Assert
            Assert.Empty(bookingForReturnedRoomId);
        }
        
        [Fact]
        public async Task GetFullyOccupiedDates_NoBookings_ReturnsEmptyList()
        {
            // Arrange
            DateTime start = DateTime.Today;
            DateTime end = DateTime.Today.AddDays(1);
            
            var bookingRepo = new Mock<IRepository<Booking>>();
            bookingRepo.Setup(r => r.GetAllAsync()).Returns(Task.FromResult<IEnumerable<Booking>>(new List<Booking>()));
            
            var roomRepo = new Mock<IRepository<Room>>();
            var manager = new BookingManager(bookingRepo.Object, roomRepo.Object);
            
            // Act
            var result = await manager.GetFullyOccupiedDates(start, end);
            
            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetFullyOccupiedDates_StartDateAfterEndDate_ThrowsArgumentException()
        {
            // Arrange
            DateTime start = DateTime.Today.AddDays(6);
            DateTime end = DateTime.Today;
            
            var bookingRepo = new Mock<IRepository<Booking>>();
            var roomRepo = new Mock<IRepository<Room>>();
            var manager = new BookingManager(bookingRepo.Object, roomRepo.Object);
            
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => manager.GetFullyOccupiedDates(start, end));
        }

        [Fact]
        public async Task GetFullyOccupiedDates_AllRoomsBookedForPeriod_ReturnsAllDatesInPeriod()
        {
            DateTime start = DateTime.Today;
            DateTime end = DateTime.Today.AddDays(5);
            
            IEnumerable<Booking> bookings = new List<Booking>
            {
                new Booking { Id=1, StartDate=start, EndDate=end, IsActive=true, CustomerId=1, RoomId=1 },
                new Booking { Id=1, StartDate=start, EndDate=end.AddDays(1), IsActive=true, CustomerId=2, RoomId=1 },
                new Booking { Id=2, StartDate=start, EndDate=end.AddDays(3), IsActive=true, CustomerId=3, RoomId=3 },
            };
            
            IEnumerable<Room> rooms = new List<Room>
            {
                new Room { Id=1, Description="A" },
                new Room { Id=2, Description="B" },
                new Room { Id=3, Description="C" },
            };
            
            var bookingRepo = new Mock<IRepository<Booking>>();
            bookingRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(bookings);
            var roomRepo = new Mock<IRepository<Room>>();
            roomRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(rooms);
            
            var manager = new BookingManager(bookingRepo.Object, roomRepo.Object);
            
            // Act
            var result = await manager.GetFullyOccupiedDates(start, end);
            
            // Assert
            Assert.Equal(6, result.Count);
        }
        
        [Fact]
        public async Task GetFullyOccupiedDates_OneRoomAvailableInPeriod_ReturnsNoFullyBookedDates()
        {
            DateTime start = DateTime.Today;
            DateTime end = DateTime.Today.AddDays(5);
            
            IEnumerable<Booking> bookings = new List<Booking>
            {
                new Booking { Id=1, StartDate=start, EndDate=end, IsActive=true, CustomerId=1, RoomId=1 },
                new Booking { Id=1, StartDate=start.AddDays(10), EndDate=end.AddDays(10), IsActive=true, CustomerId=2, RoomId=1 },
                new Booking { Id=2, StartDate=start, EndDate=end.AddDays(3), IsActive=true, CustomerId=3, RoomId=3 },
            };
            
            IEnumerable<Room> rooms = new List<Room>
            {
                new Room { Id=1, Description="A" },
                new Room { Id=2, Description="B" },
                new Room { Id=3, Description="C" },
            };
            
            var bookingRepo = new Mock<IRepository<Booking>>();
            bookingRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(bookings);
            var roomRepo = new Mock<IRepository<Room>>();
            roomRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(rooms);
            
            var manager = new BookingManager(bookingRepo.Object, roomRepo.Object);
            
            // Act
            var result = await manager.GetFullyOccupiedDates(start, end);
            
            // Assert
            Assert.Empty(result);
        }
        
        [Fact]
        public async Task GetFullyOccupiedDates_SomeDaysRoomAvailable_ReturnsAllDatesInPeriod()
        {
            DateTime start = DateTime.Today;
            DateTime end = DateTime.Today.AddDays(5);
            
            IEnumerable<Booking> bookings = new List<Booking>
            {
                new Booking { Id=1, StartDate=start, EndDate=end, IsActive=true, CustomerId=1, RoomId=1 },
                new Booking { Id=2, StartDate=start, EndDate=end.AddDays(-1), IsActive=true, CustomerId=2, RoomId=1 },
                new Booking { Id=3, StartDate=start, EndDate=end.AddDays(-3), IsActive=true, CustomerId=3, RoomId=3 },
            };
            
            IEnumerable<Room> rooms = new List<Room>
            {
                new Room { Id=1, Description="A" },
                new Room { Id=2, Description="B" },
                new Room { Id=3, Description="C" },
            };
            
            var bookingRepo = new Mock<IRepository<Booking>>();
            bookingRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(bookings);
            
            var roomRepo = new Mock<IRepository<Room>>();
            roomRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(rooms);
            
            var manager = new BookingManager(bookingRepo.Object, roomRepo.Object);
            
            // Act
            var result = await manager.GetFullyOccupiedDates(start, end);
            
            // Assert
            Assert.Equal(3, result.Count);
            Assert.Contains(end.AddDays(-3), result);
            Assert.Contains(end.AddDays(-4), result);
            Assert.Contains(end.AddDays(-5), result);

        }

    }
}
