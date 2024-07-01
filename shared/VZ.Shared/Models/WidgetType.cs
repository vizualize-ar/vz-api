namespace VZ.Shared.Models
{
    public enum WidgetType
    {
        None = 0,

        /// <summary>
        /// Endless scroll feed of reviews, display in comment-like fashion (like Disqus)
        /// </summary>
        Feed = 1,

        /// <summary>
        /// 3 visible reviews to the left of a summary panel
        /// </summary>
        Carousel = 2,

        /// <summary>
        /// 3 visible reviews with left/right arrows
        /// </summary>
        Slider = 3,

        /// <summary>
        /// Shows TR logo, average rating, total reviews
        /// </summary>
        Light = 4,

        /// <summary>
        /// TR logo, no data displayed
        /// </summary>
        Collector = 5,

        /// <summary>
        /// Count and business slug to view all reviews
        /// </summary>
        Count = 6,

        /// <summary>
        /// Shows logo, average rating, total reviews
        /// </summary>
        Combo = 7,

        /// <summary>
        /// Shows logo and average business rating
        /// </summary>
        Star = 8,

        /// <summary>
        /// Shows logo, average rating, total reviews, 
        /// </summary>
        Flex = 9
    }
}
