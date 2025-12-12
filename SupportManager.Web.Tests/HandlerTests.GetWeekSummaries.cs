using Shouldly;
using Xunit;
using static SupportManager.Web.Areas.Teams.Pages.Report.DailyModel;

namespace SupportManager.Web.Tests
{
    public partial class HandlerTests
    {
        public class GetWeekSummaries
        {
            [Fact]
            public void Returns_Summaries_For_Every_Slot()
            {
                // Arrange
                var startTime = DateTimeOffset.Now;

                var week = new Result.Week
                {
                    Slots =
                    [
                        new() { StartTime = startTime, EndTime = startTime.AddHours(12), GroupingKey = "A", Participations = [] },
                        new() { StartTime = startTime.AddHours(36), EndTime = startTime.AddHours(48), GroupingKey = "B", Participations = [] },
                        new() { StartTime = startTime.AddHours(50), EndTime = startTime.AddHours(60), GroupingKey = "C", Participations = [] },
                    ]
                };

                // Act
                var summaries = Handler.GetWeekSummaries(week);

                // Assert
                summaries.Count.ShouldBe(3);
            }

            [Fact]
            public void Returns_One_Summary_Per_GroupingKey()
            {
                // Arrange
                var startTime = DateTimeOffset.Now;

                var week = new Result.Week
                {
                    Slots =
                    [
                        new() { StartTime = startTime, EndTime = startTime.AddHours(1), GroupingKey = "A", Participations = [] },
                        new() { StartTime = startTime.AddHours(2), EndTime = startTime.AddHours(3), GroupingKey = "A", Participations = [] },
                        new() { StartTime = startTime.AddHours(4), EndTime = startTime.AddHours(5), GroupingKey = "B", Participations = [] },
                    ]
                };

                // Act
                var summaries = Handler.GetWeekSummaries(week);

                // Assert
                summaries.Count.ShouldBe(2);
                summaries.ShouldContain(s => s.GroupingKey == "A");
                summaries.ShouldContain(s => s.GroupingKey == "B");
            }

            [Fact]
            public void Sets_Duration_As_Sum_Of_Slot_Durations_Per_Group()
            {
                // Arrange
                var startTime = DateTimeOffset.Now;

                var week = new Result.Week
                {
                    Slots =
                    [
                        new() { StartTime = startTime, EndTime = startTime.AddHours(2), GroupingKey = "A", Participations = [] },
                        new() { StartTime = startTime.AddHours(3), EndTime = startTime.AddHours(4), GroupingKey = "A", Participations = [] },
                    ]
                };

                // Act
                var summaries = Handler.GetWeekSummaries(week);

                // Assert
                summaries.Count.ShouldBe(1);
                var summary = summaries.First(s => s.GroupingKey == "A");
                var expected = TimeSpan.FromHours(3);
                summary.Duration.ShouldBe(expected);
            }

            [Fact]
            public void Aggregates_Participations_Per_User()
            {
                // Arrange
                var startTime = DateTimeOffset.Now;

                var week = new Result.Week
                {
                    Slots =
                    [
                        new()
                        {
                            StartTime = startTime,
                            EndTime = startTime.AddHours(1),
                            GroupingKey = "A",
                            Participations = [ new Result.Participation { UserName = "Miguel", Duration = TimeSpan.FromMinutes(10), FirstStart = startTime }]
                        },
                        new()
                        {
                            StartTime = startTime.AddHours(2),
                            EndTime = startTime.AddHours(3),
                            GroupingKey = "A",
                            Participations = [ new Result.Participation { UserName = "Miguel", Duration = TimeSpan.FromMinutes(20), FirstStart = startTime.AddHours(2) }]
                        }
                    ]
                };

                // Act
                var summaries = Handler.GetWeekSummaries(week);

                // Assert
                var summary = summaries.First(s => s.GroupingKey == "A");
                summary.Participations.Count.ShouldBe(1);
                var p = summary.Participations.First();
                p.UserName.ShouldBe("Miguel");
                var expectedDuration = TimeSpan.FromMinutes(30);
                p.Duration.ShouldBe(expectedDuration);
            }

            [Fact]
            public void Uses_Earliest_FirstStart_Per_User()
            {
                // Arrange
                var startTime = DateTimeOffset.Now;
                var first = startTime.AddHours(5);
                var earlier = startTime.AddHours(1);
                var week = new Result.Week
                {
                    Slots =
                    [
                        new()
                        {
                            StartTime = startTime,
                            EndTime = startTime.AddHours(1),
                            GroupingKey = "A",
                            Participations = [ new Result.Participation { UserName = "Miguel", Duration = TimeSpan.FromMinutes(10), FirstStart = first }]
                        },
                        new()
                        {
                            StartTime = startTime.AddHours(2),
                            EndTime = startTime.AddHours(3),
                            GroupingKey = "A",
                            Participations = [ new Result.Participation { UserName = "Miguel", Duration = TimeSpan.FromMinutes(20), FirstStart = earlier } ]
                        }
                    ]
                };

                // Act
                var summaries = Handler.GetWeekSummaries(week);

                // Assert
                var summary = summaries.First(s => s.GroupingKey == "A");
                var p = summary.Participations.First(x => x.UserName == "Miguel");
                p.FirstStart.ShouldBe(earlier);
            }
        }
    }
}
