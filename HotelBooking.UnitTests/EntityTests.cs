using System;
using HotelBooking.Core;
using Xunit;

namespace HotelBooking.UnitTests;

public class EntityTests
{
    [Fact]
    public void CanSetAndGetRoomProperties()
    {
        var room = new Room { Id = 1, Description = "Suite" };
        Assert.Equal(1, room.Id);
        Assert.Equal("Suite", room.Description);
    }
    
    [Fact]
    public void CanSetAndGetCustomerProperties()
    {
        var customer = new Customer
        {
            Id = 42,
            Name = "Alice Smith",
            Email = "alice@example.com"
        };

        Assert.Equal(42, customer.Id);
        Assert.Equal("Alice Smith", customer.Name);
        Assert.Equal("alice@example.com", customer.Email);
    }
    
    [Fact]
    public void CanSetAndGetProperties()
    {
        var booking = new Booking
        {
            Id = 10,
            StartDate = new DateTime(2024, 7, 1),
            EndDate = new DateTime(2024, 7, 5),
            IsActive = true,
            CustomerId = 42,
            RoomId = 7,
            Customer = new Customer { Id = 42, Name = "Alice" },
            Room = new Room { Id = 7, Description = "Suite" }
        };

        Assert.Equal(10, booking.Id);
        Assert.Equal(new DateTime(2024, 7, 1), booking.StartDate);
        Assert.Equal(new DateTime(2024, 7, 5), booking.EndDate);
        Assert.True(booking.IsActive);
        Assert.Equal(42, booking.CustomerId);
        Assert.Equal(7, booking.RoomId);
        Assert.Equal(42, booking.Customer.Id);
        Assert.Equal("Alice", booking.Customer.Name);
        Assert.Equal(7, booking.Room.Id);
        Assert.Equal("Suite", booking.Room.Description);
    }
}