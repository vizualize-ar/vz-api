using System;
using System.Collections.Generic;
using System.Text;

namespace VZ.Reviews.Services.Models.Request
{
    public class NewReviewRequestImage
    {
        public string productId { get; set; }
        public string[] images { get; set; }
    }
}
