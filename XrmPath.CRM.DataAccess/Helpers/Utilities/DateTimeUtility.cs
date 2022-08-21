using System;
using System.Linq;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using MVCCrm.CrmCore;
using XrmPath.CRM.DataAccess;

namespace Sample.Web.Crm.Helpers.Utilities
{
    public static class DateTimeUtility
    {

        public static DateTime ToUtcDateFromLocal(this DateTime dt)
        {
            return TimeZoneInfo.ConvertTimeToUtc(new DateTime(dt.Year, dt.Month, dt.Day, 0, 0, 0, DateTimeKind.Local), TimeZoneInfo.Local);
        }

        public static DateTime ToUtcDateTimeFromLocal(this DateTime dt)
        {
            return TimeZoneInfo.ConvertTimeToUtc(new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second, DateTimeKind.Local), TimeZoneInfo.Local);
        }

        public static DateTime ToLocalFromUtcDate(this DateTime dt)
        {
            return TimeZoneInfo.ConvertTimeFromUtc(new DateTime(dt.Year, dt.Month, dt.Day, 0, 0, 0, DateTimeKind.Unspecified), TimeZoneInfo.Local);
        }

        public static DateTime ToLocalFromUtcDateTime(this DateTime dt)
        {
            return TimeZoneInfo.ConvertTimeFromUtc(new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second, DateTimeKind.Unspecified), TimeZoneInfo.Local);
        }
        public static DateTime ToMountainDateTime(this DateTime dt)
        {
            return TimeZoneInfo.ConvertTimeBySystemTimeZoneId(dt, "Mountain Standard Time");
        }
        public static DateTime? ToMountainDateTime(this DateTime? dt)
        {
            if (dt == null)
            {
                return null;
            }
            return TimeZoneInfo.ConvertTimeBySystemTimeZoneId(dt.Value, "Mountain Standard Time");
        }


        public static DateTime? ToLocalFromUtcDate(this DateTime? dt)
        {
            if (dt == null)
            {
                return null;
            }
            return TimeZoneInfo.ConvertTimeFromUtc(new DateTime(dt.Value.Year, dt.Value.Month, dt.Value.Day, 0, 0, 0, DateTimeKind.Unspecified), TimeZoneInfo.Local);
        }

        public static DateTime? ToLocalFromUtcDateTime(this DateTime? dt)
        {
            if (dt == null)
            {
                return null;
            }
            return TimeZoneInfo.ConvertTimeFromUtc(new DateTime(dt.Value.Year, dt.Value.Month, dt.Value.Day, dt.Value.Hour, dt.Value.Minute, dt.Value.Second, DateTimeKind.Unspecified), TimeZoneInfo.Local);
        }

        public static DateTime? ConvertFromCrmDateTime(this DateTime dt, TimeZoneInfo tzi)
        {
            if (tzi == null)
            {
                throw new ArgumentNullException(nameof(tzi));
            }
            return TimeZoneInfo.ConvertTimeBySystemTimeZoneId(dt, tzi.Id);
        }

        public static DateTime ConvertFromCrmDateTime(this DateTime dt, IOrganizationService service = null)
        {
            var websiteDateTime = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(dt, CrmTimeZoneInfo.Id);
            return websiteDateTime;
        }

        #region Cache Datetime Info

        //cache. these values will never change and we don't want to hit the database each time to get this information
        //load it the first time and pull from cache.

        private static IOrganizationService _CrmOrganizationService;
        public static IOrganizationService CrmOrganizationService
        {
            get
            {

                //if cacheeventlist is null or is older than x hours
                if (_CrmOrganizationService == null)
                {
                    var crmManager = new CrmConnectionManager();
                    _CrmOrganizationService = crmManager.GetOrganizationService();
                }

                return _CrmOrganizationService;
            }
            set
            {
                _CrmOrganizationService = value;
            }
        }

        private static UserSettings _CrmUserSettings;
        public static UserSettings CrmUserSettings
        {
            get
            {
                
                //if cacheeventlist is null or is older than x hours
                if (_CrmUserSettings == null)
                {
                    _CrmUserSettings = RetrieveCurrentUsersSettings(CrmOrganizationService);
                }

                return _CrmUserSettings;
            }
            set
            {
                _CrmUserSettings = value;
            }
        }

        private static TimeZoneInfo _CrmTimeZoneInfo;
        public static TimeZoneInfo CrmTimeZoneInfo
        {
            get
            {

                //if cacheeventlist is null or is older than x hours
                if (_CrmTimeZoneInfo == null)
                {
                    _CrmTimeZoneInfo = GetUserTimeZone(CrmOrganizationService, CrmUserSettings);
                }

                return _CrmTimeZoneInfo;
            }
            set
            {
                _CrmTimeZoneInfo = value;
            }
        }

        #endregion

        #region Timezone info


        /// <summary>
        /// Retrieves the current users timezone code and locale id
        /// </summary>
        private static UserSettings RetrieveCurrentUsersSettings(IOrganizationService service)
        {
            var currentUserSettings = service.RetrieveMultiple(
                new QueryExpression(UserSettings.EntityLogicalName)
                {
                    ColumnSet = new ColumnSet("localeid", "timezonecode"),
                    Criteria = new FilterExpression
                    {
                        Conditions =
                        {
                            new ConditionExpression("systemuserid", ConditionOperator.EqualUserId)
                        }
                    }
                }).Entities[0].ToEntity<UserSettings>();

            //_localeId = currentUserSettings.LocaleId;
            //_timeZoneCode = currentUserSettings.TimeZoneCode;
            return currentUserSettings;
        }

        public static TimeZoneInfo GetUserTimeZone(IOrganizationService service, UserSettings userSettings)
        {
            var timeZoneCode = 35; //default timezone to eastern just incase one doesnt exists for user
            
            if (userSettings?.TimeZoneCode != null)
            {
                timeZoneCode = userSettings.TimeZoneCode.Value;
            }

            return GetTimeZone(service, timeZoneCode);
        }

        public static TimeZoneInfo GetTimeZone(IOrganizationService service, int crmTimeZoneCode)
        {
            var qe = new QueryExpression(TimeZoneDefinition.EntityLogicalName)
            {
                ColumnSet = new ColumnSet("standardname")
            };
            qe.Criteria.AddCondition("timezonecode", ConditionOperator.Equal, crmTimeZoneCode);
            return TimeZoneInfo.FindSystemTimeZoneById(service.RetrieveMultiple(qe).Entities.First().ToEntity<TimeZoneDefinition>().StandardName);
        }

        #endregion

        public static bool MonthRangeCheck(DateTime checkDate, DateTime? startDate, DateTime? endDate)
        {
            var isInRange = false;

            if (startDate == null)
            {
                startDate = DateTime.MinValue;
            }
            if (endDate == null || endDate == DateTime.MinValue)
            {
                endDate = DateTime.MaxValue;
            }

            if (startDate >= checkDate && startDate < checkDate.AddMonths(1))
            {
                isInRange = true;
            }

            if (endDate >= checkDate && endDate < checkDate.AddMonths(1))
            {
                isInRange = true;
            }

            if (startDate <= checkDate && endDate >= checkDate)
            {
                isInRange = true;
            }

            return isInRange;
        }

        public static string DateRange(DateTime? startDate, DateTime? endDate, string dateFormat = "MMMM d, yyyy h:mm tt")
        {
            var dateRange = "";
            if (startDate != null && endDate != null)
            {
                if (startDate.Value.Date == endDate.Value.Date)
                {
                    if (startDate.Value == endDate.Value)
                    {
                        dateRange = startDate.Value.ToString(dateFormat);
                    }
                    else
                    {
                        dateRange = $"{startDate.Value:MMMM d, yyyy} {startDate.Value:h:mm tt} - {endDate.Value:h:mm tt}";
                    }
                }
                else
                {
                    dateRange = $"{startDate.Value:MMMM d, yyyy} - {endDate.Value:MMMM d, yyyy}";
                }
            }
            else if (startDate != null)
            {
                dateRange = startDate.Value.ToString(dateFormat);
            }
            else if(endDate != null)
            {
                dateRange = endDate.Value.ToString(dateFormat);
            }
            return dateRange;
        }

        public static string DateName(DateTime? startDate, DateTime? endDate, string dateFormat = "MMMM d, yyyy")
        {
            var dateName = "";
            if (startDate != null && endDate != null)
            {
                dateName = startDate.Value.Date == endDate.Value.Date ? startDate.Value.ToString(dateFormat) : $"{startDate.Value:MMMM d, yyyy} - {endDate.Value:MMMM d, yyyy}";
            }
            else if (startDate != null)
            {
                dateName = startDate.Value.ToString(dateFormat);
            }
            else if (endDate != null)
            {
                dateName = endDate.Value.ToString(dateFormat);
            }
            return dateName;
        }

        public static string TimeRange(DateTime? startDate, DateTime? endDate)
        {
            var timeFormat = "h:mm tt";
            var dateRange = "";
            if (startDate != null && endDate != null)
            {
                if (startDate.Value.Date == endDate.Value.Date)
                {
                    dateRange = startDate.Value == endDate.Value ? startDate.Value.ToString(timeFormat) : $"{startDate.Value:h:mm tt} - {endDate.Value:h:mm tt}";
                }
                else
                {
                    dateRange = $"{startDate.Value:MMMM d, yyyy} - {endDate.Value:MMMM d, yyyy}";
                }
            }
            else if (startDate != null)
            {
                dateRange = startDate.Value.ToString(timeFormat);
            }
            else if (endDate != null)
            {
                dateRange = endDate.Value.ToString(timeFormat);
            }
            return dateRange;
        }
    }
}
