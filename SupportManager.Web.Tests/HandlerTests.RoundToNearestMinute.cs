using Shouldly;
using Xunit;
using static SupportManager.Web.Areas.Teams.Pages.Report.DailyModel;

namespace SupportManager.Web.Tests
{
    public partial class HandlerTests
    {
        public class RoundToNearestMinute
        {
            [Fact]
            public void Rounds_Down_When_Less_Than_30_Seconds()
            {
                // Arrange
                var input = TimeSpan.FromSeconds(29);

                // Act
                var result = Handler.RoundToNearestMinute(input);

                // Assert
                result.ShouldBe(TimeSpan.Zero);
            }

            [Fact]
            public void Rounds_Up_At_30_Seconds()
            {
                // Arrange
                var input = TimeSpan.FromSeconds(30);

                // Act
                var result = Handler.RoundToNearestMinute(input);

                // Assert
                result.ShouldBe(TimeSpan.FromMinutes(1));
            }

            [Fact]
            public void Rounds_Up_When_More_Than_30_Seconds()
            {
                // Arrange
                var input = TimeSpan.FromSeconds(91); // 1:31

                // Act
                var result = Handler.RoundToNearestMinute(input);

                // Assert
                result.ShouldBe(TimeSpan.FromMinutes(2));
            }

            [Fact]
            public void Exact_Minutes_Are_Not_Changed()
            {
                // Arrange
                var input = TimeSpan.FromMinutes(5);

                // Act
                var result = Handler.RoundToNearestMinute(input);

                // Assert
                result.ShouldBe(TimeSpan.FromMinutes(5));
            }
        }
        
    }
}
