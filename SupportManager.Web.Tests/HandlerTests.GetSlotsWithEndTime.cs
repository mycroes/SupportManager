using Shouldly;
using Xunit;
using static SupportManager.Web.Areas.Teams.Pages.Report.DailyModel;

namespace SupportManager.Web.Tests
{
    public partial class HandlerTests
    {
        public class GetSlotsWithEndTime
        {
            [Fact]
            public void Returns_One_Less_Slot_Than_Start_Times()
            {
                // Arrange
                var week = new Result.Week();

                var startTimes = new List<(Result.Week week, DateTime start, string groupingKey)>
                {
                    (week, new DateTime(2025, 1, 1, 7, 30, 0), "A"),
                    (week, new DateTime(2025, 1, 1, 16, 30, 0), "B"),
                    (week, new DateTime(2025, 1, 2, 7, 30, 0), "C"),
                };

                // Act
                var result = Handler.GetSlotsWithEndTime(startTimes).ToList();

                // Assert
                result.Count.ShouldBe(2);
            }

            [Fact]
            public void Uses_Next_Start_Time_As_End_Time()
            {
                // Arrange
                var week = new Result.Week();

                var firstStart = new DateTime(2025, 1, 1, 7, 30, 0);

                var secondStart = new DateTime(2025, 1, 1, 16, 30, 0);

                var startTimes = new List<(Result.Week week, DateTime start, string groupingKey)>
                {
                    (week, firstStart, "A"),
                    (week, secondStart, "B"),
                };

                // Act
                var result = Handler.GetSlotsWithEndTime(startTimes).ToList();

                var slot = result[0];

                // Assert
                slot.Item2.ShouldBe(firstStart);
                slot.Item3.ShouldBe(secondStart);
            }

            [Fact]
            public void Preserves_Grouping_Key_Of_Start_Slot()
            {
                // Arrange
                var week = new Result.Week();

                var startTimes = new List<(Result.Week week, DateTime start, string groupingKey)>
                {
                    (week, new DateTime(2025, 1, 1, 7, 30, 0), "Kantooruren"),
                    (week, new DateTime(2025, 1, 1, 16, 30, 0), "Doordeweeks"),
                };

                // Act
                var result = Handler.GetSlotsWithEndTime(startTimes).ToList();

                var slot = result[0];

                // Assert
                slot.Item4.ShouldBe("Kantooruren");
            }
        }
    }
}
