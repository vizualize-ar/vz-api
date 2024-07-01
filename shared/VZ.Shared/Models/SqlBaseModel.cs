using System;

namespace VZ.Shared.Models
{
    public class SqlBaseModel
    {
        public SqlBaseModel()
        {
            this.createdOn = DateTime.UtcNow;
        }

        public long id { get; set; }
        public DateTime createdOn { get; set; }
        public DateTime? updatedOn { get; set; }
    }
}
