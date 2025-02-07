using System;

namespace Domain.Entities
{
    public abstract class BaseEntity<T>:IBaseEntity<T>
    {
        public T Id { get; set; }
        public DateTime DateCreated { get; set; } = DateTime.Now;
        public DateTime? DateModified { get; set; }
        public bool IsDeleted { get; set; }

    }
}
