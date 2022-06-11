using Sidekick.Model;
using System.Collections.Generic;
using System.Reflection;

namespace Sidekick.Admin.Extensions
{
    public static class PopulateStatistics
    {
        public static GroupLastMonthYear PopulateLastMonthStats<T>(this KeyValuePair<T, int> item, GroupLastMonthYear model)
        {
            var getType = item.Key.GetType();
            int weekNumber = 0;
            IList<PropertyInfo> props = new List<PropertyInfo>(getType.GetProperties());
            foreach (var prop in props)
            {
                object property = prop.Name;
                object value = prop.GetValue(item.Key, null);
                if (property.ToString() == "Month")
                    model.LastMonth = int.Parse(value.ToString());
                if (property.ToString() == "Year")
                    model.Year = int.Parse(value.ToString());
                if (property.ToString() == "WeekNumber")
                    weekNumber = int.Parse(value.ToString());
                    model.WeekNumber = weekNumber == 5 ? 1 : weekNumber;
            }

            model.ObjectCount = item.Value;

            return model;
        }

        public static GroupYear PopulateThisYearStats<T>(this KeyValuePair<T, int> item, GroupYear model)
        {
            var getType = item.Key.GetType();
            IList<PropertyInfo> props = new List<PropertyInfo>(getType.GetProperties());
            foreach (var prop in props)
            {
                object property = prop.Name;
                object value = prop.GetValue(item.Key, null);
                if (property.ToString() == "Month")
                    model.Month = int.Parse(value.ToString());
                if (property.ToString() == "Year")
                    model.Year = int.Parse(value.ToString());
            }

            model.ObjectCount = item.Value;

            return model;
        }


        public static GroupYear PopulatePaymentYearStats<T>(this KeyValuePair<T, decimal> item, GroupYear model)
        {
            var getType = item.Key.GetType();
            IList<PropertyInfo> props = new List<PropertyInfo>(getType.GetProperties());
            foreach (var prop in props)
            {
                object property = prop.Name;
                object value = prop.GetValue(item.Key, null);
                if (property.ToString() == "Month")
                    model.Month = int.Parse(value.ToString());
                if (property.ToString() == "Year")
                    model.Year = int.Parse(value.ToString());
               

            }

            model.TotalAmount = item.Value;

            return model;
        }

        public static GroupLastMonthYear PopulatePaymentLastMonthStats<T>(this KeyValuePair<T, decimal> item, GroupLastMonthYear model)
        {
            var getType = item.Key.GetType();
            int weekNumber = 0;
            IList<PropertyInfo> props = new List<PropertyInfo>(getType.GetProperties());
            foreach (var prop in props)
            {
                object property = prop.Name;
                object value = prop.GetValue(item.Key, null);
                if (property.ToString() == "Month")
                    model.LastMonth = int.Parse(value.ToString());
                if (property.ToString() == "Year")
                    model.Year = int.Parse(value.ToString());
                if (property.ToString() == "WeekNumber")
                    weekNumber = int.Parse(value.ToString());
                model.WeekNumber = weekNumber == 5 ? 1 : weekNumber;
            }

            model.TotalAmount = item.Value;

            return model;
        }
    }
}
