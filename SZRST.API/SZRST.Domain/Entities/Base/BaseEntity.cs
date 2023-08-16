using System;

namespace Domain.Entities
{
    public abstract class BaseEntity<T>:IBaseEntity<T>
    {
        public T Id { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime DateModified { get; set; }
    }
}
