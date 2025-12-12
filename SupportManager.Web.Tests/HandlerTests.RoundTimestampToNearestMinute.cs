using Shouldly;
using Xunit;
using static SupportManager.Web.Areas.Teams.Pages.Report.DailyModel;

namespace SupportManager.Web.Tests
{
    public partial class HandlerTests
    {
        public class RoundTimestampToNearestMinute
        {
            [Fact]
            public void Rounds_Down_When_Seconds_Less_Than_30()
            {
                // Arrange
                var input = new DateTimeOffset(2025, 1, 1, 10, 15, 29, TimeSpan.FromHours(1));

                // Act
                var result = Handler.RoundTimestampToNearestMinute(input);

                // Assert
                result.ShouldBe(new DateTimeOffset(2025, 1, 1, 10, 15, 0, TimeSpan.FromHours(1)));
            }

            [Fact]
            public void Rounds_Up_When_Seconds_Are_30_Or_More()
            {
                // Arrange
                var input = new DateTimeOffset(2025, 1, 1, 10, 15, 30, TimeSpan.FromHours(1));

                // Act
                var result = Handler.RoundTimestampToNearestMinute(input);

                // Assert
                result.ShouldBe(new DateTimeOffset(2025, 1, 1, 10, 16, 0, TimeSpan.FromHours(1)));
            }

            [Fact]
            public void Rounds_Up_Across_Hour_Boundary()
            {
                // Arrange
                var input = new DateTimeOffset(2025, 1, 1, 10, 59, 45, TimeSpan.Zero);

                // Act
                var result = Handler.RoundTimestampToNearestMinute(input);

                // Assert
                result.ShouldBe(new DateTimeOffset(2025, 1, 1, 11, 0, 0, TimeSpan.Zero));
            }
        }
    }
}

