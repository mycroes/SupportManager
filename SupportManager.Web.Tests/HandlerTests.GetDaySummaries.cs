using Shouldly;
using Xunit;
using static SupportManager.Web.Areas.Teams.Pages.Report.DailyModel;

namespace SupportManager.Web.Tests
{
    public partial class HandlerTests
    {
        public class GetDaySummaries
        {
            [Fact]
            public void Returns_One_Summary_Per_GroupingKey()
            {
                // Arrange
                var startTime = DateTimeOffset.Now;

                var day = new Result.Day
                {
                    Slots =
                    [
                        new() { StartTime = startTime, EndTime = startTime.AddHours(1), GroupingKey = "A", Participations = [] },
                        new() { StartTime = startTime.AddHours(2), EndTime = startTime.AddHours(3), GroupingKey = "A", Participations = [] },
                        new() { StartTime = startTime.AddHours(4), EndTime = startTime.AddHours(5), GroupingKey = "B", Participations = [] },
                    ]
                };

                // Act
                var summaries = Handler.GetDaySummaries(day);

                // Assert
                summaries.Count.ShouldBe(2);
                summaries.ShouldContain(s => s.GroupingKey == "A");
                summaries.ShouldContain(s => s.GroupingKey == "B");
            }

            [Fact]
            public void Sets_Duration_As_Sum_Of_Slot_Durations()
            {
                // Arrange
                var startTime = DateTimeOffset.Now;

                var day = new Result.Day
                {
                    Slots =
                    [
                        new() { StartTime = startTime, EndTime = startTime.AddHours(2), GroupingKey = "A", Participations = [] },
                        new() { StartTime = startTime.AddHours(3), EndTime = startTime.AddHours(4), GroupingKey = "A", Participations = [] },
                    ]
                };

                // Act
                var summaries = Handler.GetDaySummaries(day);

                // Assert
                var summary = summaries.First(s => s.GroupingKey == "A");
                summary.Duration.ShouldBe(TimeSpan.FromHours(3));
            }

            [Fact]
            public void Aggregates_Participations_Per_User()
            {
                // Arrange
                var startTime = DateTimeOffset.Now;

                var day = new Result.Day
                {
                    Slots =
                    [
                        new()
                        {
                            StartTime = startTime,
                            EndTime = startTime.AddHours(1),
                            GroupingKey = "A",
                            Participations =
                            [
                                new Result.Participation { UserName = "Miguel", Duration = TimeSpan.FromMinutes(10), FirstStart = startTime }
                            ]
                        },
                        new()
                        {
                            StartTime = startTime.AddHours(2),
                            EndTime = startTime.AddHours(3),
                            GroupingKey = "A",
                            Participations =
                            [
                                new Result.Participation { UserName = "Miguel", Duration = TimeSpan.FromMinutes(20), FirstStart = startTime.AddHours(2) }
                            ]
                        }
                    ]
                };

                // Act
                var summaries = Handler.GetDaySummaries(day);

                // Assert
                var summary = summaries.First(s => s.GroupingKey == "A");
                summary.Participations.Count.ShouldBe(1);

                var p = summary.Participations.First();
                p.UserName.ShouldBe("Miguel");
                p.Duration.ShouldBe(TimeSpan.FromMinutes(30));
            }

            [Fact]
            public void Uses_Earliest_FirstStart_Per_User()
            {
                // Arrange
                var startTime = DateTimeOffset.Now;
                var later = startTime.AddHours(4);
                var earlier = startTime.AddHours(1);

                var day = new Result.Day
                {
                    Slots =
                    [
                        new()
                        {
                            StartTime = startTime,
                            EndTime = startTime.AddHours(1),
                            GroupingKey = "A",
                            Participations =
                            [
                                new Result.Participation { UserName = "Miguel", Duration = TimeSpan.FromMinutes(10), FirstStart = later }
                            ]
                        },
                        new()
                        {
                            StartTime = startTime.AddHours(2),
                            EndTime = startTime.AddHours(3),
                            GroupingKey = "A",
                            Participations =
                            [
                                new Result.Participation { UserName = "Miguel", Duration = TimeSpan.FromMinutes(20), FirstStart = earlier }
                            ]
                        }
                    ]
                };

                // Act
                var summaries = Handler.GetDaySummaries(day);

                // Assert
                var summary = summaries.First(s => s.GroupingKey == "A");
                var p = summary.Participations.First();
                p.FirstStart.ShouldBe(earlier);
            }
        }
    }
}
