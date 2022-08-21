using System;

namespace XrmPath.Helpers.Utilities
{
    public static class DateUtility
    {
        public static string GetDateTimeRange(DateTime startDate, DateTime? endDate = null, bool allDay = false)
        {
            var dateRange = string.Empty;

            try
            {
                var dateFormat = "MMMM d, yyyy";

                if (endDate == null || endDate == DateTime.MinValue || endDate == DateTime.MaxValue ||
                    endDate <= startDate)
                {
                    if (!allDay)
                    {
                        dateFormat = "MMMM d, yyyy h:mm tt";
                    }
                    dateRange = startDate.ToString(dateFormat);
                }
                else
                {
                    //has start and end date
                    var dtEndDate = Convert.ToDateTime(endDate);
                    if (startDate.Year == dtEndDate.Year && startDate.Month == dtEndDate.Month && startDate.Day == dtEndDate.Day)
                    {
                        //same day
                        if (!allDay)
                        {
                            dateRange = $"{startDate.ToString(dateFormat)} {startDate:h:mm tt} - {dtEndDate:h:mm tt}";
                        }
                        else
                        {
                            dateRange = $"{startDate.ToString(dateFormat)}";
                        }
                    }
                    else
                    {
                        //different day
                        if (!allDay)
                        {
                            dateFormat = "MMMM d, yyyy h:mm tt";
                        }
                        dateRange = $"{startDate.ToString(dateFormat)} - {dtEndDate.ToString(dateFormat)}";
                    }
                }
            }
            catch (Exception ex)
            {
                Serilog.Log.Error(ex, "XrmPath caught error on DateHelper.GetDateTimeRange()");
                //LogHelper.Error<string>("XrmPath caught error on DateHelper.GetDateTimeRange()", ex);
            }
            return dateRange;
        }

        public static string GetDateRange(DateTime startDate, DateTime? endDate = null, bool allDay = false)
        {
            var dateRange = string.Empty;

            try
            {
                var dateFormat = "MMMM d, yyyy";

                if (endDate == null || endDate == DateTime.MinValue || endDate == DateTime.MaxValue ||
                    endDate <= startDate)
                {
                    //dateFormat = "MMMM d, yyyy h:mm tt";
                    dateRange = startDate.ToString(dateFormat);
                }
                else
                {
                    //has start and end date
                    var dtEndDate = Convert.ToDateTime(endDate);
                    if (startDate.Year == dtEndDate.Year && startDate.Month == dtEndDate.Month &&
                        startDate.Day == dtEndDate.Day)
                    {
                        //same day
                        //dateRange = $"{startDate.ToString(dateFormat)} {startDate:h:mm tt} - {dtEndDate:h:mm tt}";
                        dateRange = startDate.ToString(dateFormat);
                    }
                    else
                    {
                        //different day
                        //dateFormat = "MMMM d, yyyy h:mm tt";
                        dateRange = $"{startDate.ToString(dateFormat)} - {dtEndDate.ToString(dateFormat)}";
                    }
                }
            }
            catch (Exception ex)
            {
                Serilog.Log.Error(ex, "XrmPath caught error on DateHelper.GetDateRange()");
                //LogHelper.Error<string>("XrmPath caught error on DateHelper.GetDateRange()", ex);
            }

            return dateRange;
        }

        public static string GetTimeRange(DateTime startDate, DateTime? endDate = null, bool allDay = false, bool showTimeOnly = false)
        {
            var timeRange = string.Empty;
            try
            {
                if (allDay)
                {
                    timeRange = "All Day";
                }
                else
                {
                    string dateFormat;
                    if (endDate == null || endDate == DateTime.MinValue || endDate == DateTime.MaxValue || endDate <= startDate)
                    {
                        dateFormat = "h:mm tt";
                        timeRange = startDate.ToString(dateFormat);
                    }
                    else
                    {
                        //has start and end date
                        var dtEndDate = Convert.ToDateTime(endDate);
                        var startTime = $"{startDate:h:mm tt}";
                        var endTime = $"{dtEndDate:h:mm tt}";
                        if ((startDate.Year == dtEndDate.Year && startDate.Month == dtEndDate.Month && startDate.Day == dtEndDate.Day) || showTimeOnly)
                        {
                            //same day
                            if (startTime != endTime || startTime != "12:00 AM")
                            {
                                //if both start time and end time set to 12:00 AM, means time not set.
                                timeRange = $"{startTime} - {endTime}";
                            }
                        }
                        else
                        {
                            //different day
                            dateFormat = "MMMM d, yyyy h:mm tt";
                            timeRange = $"{startDate.ToString(dateFormat)} - {dtEndDate.ToString(dateFormat)}";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Serilog.Log.Error(ex, "XrmPath caught error on DateHelper.GetTimeRange()");
                //LogHelper.Error<DateTime>("XrmPath caught error on DateHelper.GetTimeRange()", ex);
            }
            return timeRange;
        }

        public static DateTime StartOfWeek(this DateTime dt, DayOfWeek startOfWeek)
        {
            int diff = (7 + (dt.DayOfWeek - startOfWeek)) % 7;
            return dt.AddDays(-1 * diff).Date;
        }
        public static DateTime StartOfMonth(this DateTime dt)
        {
            var startMonth = new DateTime(dt.Year, dt.Month, 1);
            return startMonth;
        }

        public static DateTime StartOfMonthBusinessDay(this DateTime dt)
        {
            //get the first business day of the current month
            var startMonth = new DateTime(dt.Year, dt.Month, 1);
            var isBusinessDay = false;
            while (!isBusinessDay)
            {
                if (startMonth.DayOfWeek == DayOfWeek.Monday || startMonth.DayOfWeek == DayOfWeek.Tuesday || startMonth.DayOfWeek == DayOfWeek.Wednesday
                    || startMonth.DayOfWeek == DayOfWeek.Thursday || startMonth.DayOfWeek == DayOfWeek.Friday)
                {
                    isBusinessDay = true;
                }
                else
                {
                    startMonth = startMonth.AddDays(1);
                }
            }
            return startMonth;
        }
    }
}