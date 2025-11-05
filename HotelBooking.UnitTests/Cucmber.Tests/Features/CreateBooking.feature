Feature: Create Booking
    In order to prevent overlapping room reservations
    As a hotel booking system
    I want to only allow bookings that do not overlap with occupied dates

    Background:
        Given the hotel has one room occupied from "2025-06-10" to "2025-06-12"

    #
    # These scenarios come directly from the black-box test techniques or matrix (B, A, O zones)
    #

    Scenario: Booking fully within an occupied period (O-O)
        When I attempt to book from "2025-06-10" to "2025-06-12"
        Then the booking should be "rejected" due to overlapping dates

    Scenario: Booking fully before occupied period (B-B)
        When I attempt to book from "2025-06-07" to "2025-06-09"
        Then the booking should be "successful" due to no overlapping dates

    Scenario: Booking fully after occupied period (A-A)
        When I attempt to book from "2025-06-13" to "2025-06-15"
        Then the booking should be "successful" due to no overlapping dates

    Scenario: Booking overlaps start of occupied period (B-O)
        When I attempt to book from "2025-06-09" to "2025-06-10"
        Then the booking should be "rejected" due to overlapping dates

    Scenario: Booking overlaps end of occupied period (O-A)
        When I attempt to book from "2025-06-11" to "2025-06-13"
        Then the booking should be "rejected" due to overlapping dates

    Scenario: Booking surrounds the occupied period (B-A)
        When I attempt to book from "2025-06-08" to "2025-06-14"
        Then the booking should be "rejected" due to overlapping dates