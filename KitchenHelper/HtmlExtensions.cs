using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace KitchenHelper
{
    public static class HtmlExtensions
    {
        public static MvcHtmlString FriendlyDate(this HtmlHelper html, DateTime? input)
        {
            if (input.HasValue)
            {
                DateTime date = input.Value;
                StringBuilder dateString = new StringBuilder();

                TimeSpan span = DateTime.Now - date;

                if (span.Days >= 365)
                {
                    dateString.Append("Over ");

                    int numYears = span.Days / 365;
                    dateString.Append(numYears);
                    if (numYears == 1)
                    {
                        dateString.Append(" year ago");
                    }
                    else
                    {
                        dateString.Append(" years ago");
                    }
                }
                else if (span.Days >= 56)
                {
                    dateString.Append("Over ");

                    int numMonths = span.Days / 30;
                    dateString.Append(numMonths);
                    dateString.Append(" months ago");
                }
                else if (span.Days >= 7)
                {
                    int numWeeks = span.Days / 7;
                    dateString.Append(numWeeks);
                    if (numWeeks == 1)
                    {
                        dateString.Append(" week ago");
                    }
                    else
                    {
                        dateString.Append(" weeks ago");
                    }
                }
                else if (span.Days >= 1)
                {
                    int numDays = span.Days;
                    dateString.Append(numDays);
                    if (numDays == 1)
                    {
                        dateString.Append(" day ago");
                    }
                    else
                    {
                        dateString.Append(" days ago");
                    }
                }
                else
                {
                    if (span.Hours >= 1)
                    {
                        dateString.Append(span.Hours);
                        if (span.Hours == 1)
                        {
                            dateString.Append(" hour ago");
                        }
                        else
                        {
                            dateString.Append(" hours ago");
                        }
                    }
                    else
                    {
                        if (span.Minutes > 1)
                        {
                            dateString.Append(span.Minutes);
                            dateString.Append(" minutes ago");
                        }
                        else
                        {
                            dateString.Append("Now");
                        }
                    }
                }

                return MvcHtmlString.Create(dateString.ToString());
            }
            else
            {
                return MvcHtmlString.Create("-");
            }
        }
    }
}