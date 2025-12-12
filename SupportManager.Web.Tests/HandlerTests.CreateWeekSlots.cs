using Shouldly;
using Xunit;
using static SupportManager.Web.Areas.Teams.Pages.Report.DailyModel;

namespace SupportManager.Web.Tests
{
    public partial class HandlerTests
    {
        public class CreateWeekSlots
        {
            [Fact]
            public void Creates_Expected_Number_Of_Week_Slots()
            {
                // Act
                var slots = Handler.CreateWeekSlots();

                // Assert
                slots.Count.ShouldBe(12);
            }

            [Fact]
            public void First_Slot_Is_Monday_Kantooruren_At_07_30()
            {
                // Act
                var slots = Handler.CreateWeekSlots();
                var firstSlot = slots[0];

                // Assert
                firstSlot.GroupingKey.ShouldBe("Kantooruren");

                var expectedStart = TimeSpan.FromDays(1).Add(TimeSpan.FromHours(7.5));

                firstSlot.Start.ShouldBe(expectedStart);
            }

            [Fact]
            public void Contains_Doordeweeks_Slot_At_16_30()
            {
                // Act
                var slots = Handler.CreateWeekSlots();

                var doordeweeksSlot = slots.First(s =>
                    s.GroupingKey == "Doordeweeks" &&
                    s.Start.Hours == 16 &&
                    s.Start.Minutes == 30);

                // Assert
                doordeweeksSlot.ShouldNotBeNull();
            }

            [Fact]
            public void Contains_Three_Weekend_Slots()
            {
                // Act
                var slots = Handler.CreateWeekSlots();

                var weekendSlots = slots.Where(s => s.GroupingKey == "Weekend").ToList();

                // Assert
                weekendSlots.Count.ShouldBe(3);
            }

            [Fact]
            public void Weekend_Starts_On_Friday_At_16_30()
            {
                // Act
                var slots = Handler.CreateWeekSlots();

                var weekendStart = slots.First(s => s.GroupingKey == "Weekend");

                var expectedStart =TimeSpan.FromDays(5).Add(TimeSpan.FromHours(16.5));

                // Assert
                weekendStart.Start.ShouldBe(expectedStart);
            }

            [Fact]
            public void Sunday_Weekend_Slot_Is_Normalized_To_Day_7()
            {
                // Act
                var slots = Handler.CreateWeekSlots();

                var sundaySlot = slots.Last(s =>
                    s.GroupingKey == "Weekend" &&
                    s.Start.Hours == 7 &&
                    s.Start.Minutes == 30);

                // Assert
                sundaySlot.Start.Days.ShouldBe(7);
            }
        }
    }
}
