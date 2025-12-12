using Shouldly;
using Xunit;
using NodaTime;
using SupportManager.DAL;
using static SupportManager.Web.Areas.Teams.Pages.Report.DailyModel;

namespace SupportManager.Web.Tests
{
    public partial class HandlerTests
    {
        public class GetWeeks
        {
            private static ForwardingState CreateState(DateTimeOffset when, string userName)
            {
                return new ForwardingState
                {
                    When = when,
                    DetectedPhoneNumber = new UserPhoneNumber { User = new User { DisplayName = userName }}
                };
            }

            [Fact]
            public void Miguel_Appears_On_Monday_and_Tuesday()
            {
                // Arrange
                var weekSlots = Handler.CreateWeekSlots();
                var resultStart = new DateTime(2025, 3, 3, 7, 30, 0); // Monday
                var resultEnd = new DateTime(2025, 3, 6, 7, 30, 0);

                var forwardingStates = new List<ForwardingState>
                {
                    CreateState(new DateTimeOffset(2025, 3, 3, 16, 30, 0, TimeSpan.Zero), "Miguel"),
                    CreateState(new DateTimeOffset(2025, 3, 4, 12, 10, 0, TimeSpan.Zero), "Miguel"),
                };

                var lastRealState = forwardingStates.Last();

                // Act
                var weeks = Handler.GetWeeks(weekSlots, resultStart, resultEnd, forwardingStates, lastRealState);

                var monday = weeks[0].Days.First(d => d.Date == LocalDate.FromDateTime(resultStart));
                var tuesday = weeks[0].Days.First(d => d.Date == LocalDate.FromDateTime(resultStart.AddDays(1)));

                // Assert
                monday.Slots.SelectMany(s => s.Participations).Any(p => p.UserName == "Miguel").ShouldBeTrue();
                tuesday.Slots.SelectMany(s => s.Participations).Any(p => p.UserName == "Miguel").ShouldBeTrue();
            }

            [Fact]
            public void Carlos_Appears_Only_On_Tuesday()
            {
                // Arrange
                var weekSlots = Handler.CreateWeekSlots();
                var resultStart = new DateTime(2025, 3, 3, 7, 30, 0);
                var resultEnd = new DateTime(2025, 3, 6, 7, 30, 0);

                var forwardingStates = new List<ForwardingState>
                {
                    CreateState(new DateTimeOffset(2025, 3, 4, 12, 11, 0, TimeSpan.Zero), "Carlos"),
                    CreateState(new DateTimeOffset(2025, 3, 5, 7, 30, 0, TimeSpan.Zero), "Carlos"),
                };

                var lastRealState = forwardingStates.Last();

                // Act
                var weeks = Handler.GetWeeks(weekSlots, resultStart, resultEnd, forwardingStates, lastRealState);

                var monday = weeks[0].Days.First(d => d.Date == LocalDate.FromDateTime(resultStart));
                var tuesday = weeks[0].Days.First(d => d.Date == LocalDate.FromDateTime(resultStart.AddDays(1)));

                // Assert
                monday.Slots.SelectMany(s => s.Participations).Any(p => p.UserName == "Carlos").ShouldBeFalse();
                tuesday.Slots.SelectMany(s => s.Participations).Any(p => p.UserName == "Carlos").ShouldBeTrue();
            }

            [Fact]
            public void No_Participations_When_States_Start_After_Day_Boundary()
            {
                // Arrange
                var weekSlots = Handler.CreateWeekSlots();
                var resultStart = new DateTime(2025, 3, 3, 7, 30, 0);
                var resultEnd = new DateTime(2025, 3, 4, 7, 30, 0);

                var forwardingStates = new List<ForwardingState>
                {
                    CreateState(new DateTimeOffset(2025, 3, 4, 8, 0, 0, TimeSpan.Zero), "Miguel"),
                };

                var lastRealState = forwardingStates.Last();

                // Act
                var weeks = Handler.GetWeeks(weekSlots, resultStart, resultEnd, forwardingStates, lastRealState);

                var monday = weeks[0].Days.First(d => d.Date == LocalDate.FromDateTime(resultStart));

                // Assert
                monday.Slots.SelectMany(s => s.Participations).ShouldBeEmpty();
            }
        }
    }
}
