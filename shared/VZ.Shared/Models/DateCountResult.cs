using System;
using System.Collections.Generic;
using System.Text;

namespace VZ.Shared.Models
{
    public class DateCountResult
    {
        public DateTime date { get; set; }
        public int count { get; set; }
        public decimal total { get; set; }

        public DateCountResult(DateTime date, int count, decimal total = 0)
        {
            this.date = date;
            this.count = count;
            this.total = total;
        }
    }
}
