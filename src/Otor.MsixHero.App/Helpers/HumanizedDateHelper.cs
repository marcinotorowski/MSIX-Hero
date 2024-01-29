// MSIX Hero
// Copyright (C) 2024 Marcin Otorowski
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// Full notice:
// https://github.com/marcinotorowski/msix-hero/blob/develop/LICENSE.md

using System;

namespace Otor.MsixHero.App.Helpers
{
    public static class HumanizedDateHelper
    {
        private static DateTime 
            _lastRefreshDate,
            _today,
            _yesterday,
            _thisWeek,
            _lastWeek,
            _thisMonth,
            _lastMonth,
            _last6Months,
            _thisYear,
            _lastYear;

        public static DateTime Today
        {
            get
            {
                EnsureDate();
                return _today;
            }
        }

        public static DateTime Yesterday
        {
            get
            {
                EnsureDate();
                return _yesterday;
            }
        }

        public static DateTime ThisWeek
        {
            get
            {
                EnsureDate();
                return _thisWeek;
            }
        }

        public static DateTime LastWeek
        {
            get
            {
                EnsureDate();
                return _lastWeek;
            }
        }

        public static DateTime ThisMonth
        {
            get
            {
                EnsureDate();
                return _thisMonth;
            }
        }

        public static DateTime LastMonth
        {
            get
            {
                EnsureDate();
                return _lastMonth;
            }
        }

        public static DateTime Last6Months
        {
            get
            {
                EnsureDate();
                return _last6Months;
            }
        }

        public static DateTime ThisYear
        {
            get
            {
                EnsureDate();
                return _thisYear;
            }
        }

        public static DateTime LastYear
        {
            get
            {
                EnsureDate();
                return _lastYear;
            }
        }

        public enum HumanizedDate
        {
            Today,
            Yesterday,
            ThisWeek,
            LastWeek,
            ThisMonth,
            LastMonth,
            Last6Months,
            ThisYear,
            LastYear,
            Older
        }
        
        public static HumanizedDate GetHumanizedDate(DateTime date)
        {
            EnsureDate();
            var now = date.Date;

            if (now >= _today)
            {
                return HumanizedDate.Today;
            }

            if (now >= _yesterday)
            {
                return HumanizedDate.Yesterday;
            }

            if (now >= _thisWeek)
            {
                return HumanizedDate.ThisWeek;
            }

            if (now >= _lastWeek)
            {
                return HumanizedDate.LastWeek;
            }

            if (now >= _thisMonth)
            {
                return HumanizedDate.ThisMonth;
            }

            if (now >= _lastMonth)
            {
                return HumanizedDate.LastMonth;
            }

            if (now >= _last6Months)
            {
                return HumanizedDate.Last6Months;
            }

            if (now >= _thisYear)
            {
                return HumanizedDate.ThisYear;
            }

            if (now >= _lastYear)
            {
                return HumanizedDate.LastYear;
            }

            return HumanizedDate.Older;
        }

        private static void EnsureDate()
        {
            if (_lastRefreshDate < DateTime.Now.Date)
            {
                Refresh();
            }
        }

        private static void Refresh()
        {
            _today = DateTime.Now.Date;
            _yesterday = _today.Subtract(TimeSpan.FromHours(24));
            _thisWeek = _today.Subtract(TimeSpan.FromDays(((int)_today.DayOfWeek + 6) % 7));
            _lastWeek = _thisWeek.Subtract(TimeSpan.FromDays(7));
            _thisMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            _lastMonth = _today.AddMonths(-1);
            _last6Months = _today.AddMonths(-6);
            _thisYear = new DateTime(DateTime.Now.Year, 1, 1);
            _lastYear = new DateTime(DateTime.Now.Year - 1, 1, 1);
            _lastRefreshDate = DateTime.Now.Date;
        }
    }
}