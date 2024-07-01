using System;
using System.Collections.Generic;
using System.Text;

namespace VZ.Reviews.Services.Models.Request
{
    public class ReviewVoteRequest
    {
        /// <summary>
        /// 1 = up, -1 = down, 0 remove vote
        /// </summary>
        public short vote { get; set; }
    }
}
