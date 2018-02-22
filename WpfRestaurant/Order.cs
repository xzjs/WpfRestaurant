//------------------------------------------------------------------------------
// <auto-generated>
//     此代码已从模板生成。
//
//     手动更改此文件可能导致应用程序出现意外的行为。
//     如果重新生成代码，将覆盖对此文件的手动更改。
// </auto-generated>
//------------------------------------------------------------------------------

namespace WpfRestaurant
{
    using System;
    using System.Collections.Generic;
    
    public partial class Order
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Order()
        {
            this.Bill = new HashSet<Bill>();
        }
    
        public long Id { get; set; }
        public string Name { get; set; }
        public Nullable<long> Phone { get; set; }
        public Nullable<int> Counts { get; set; }
        public string No { get; set; }
        public string Remark { get; set; }
        public long Table_id { get; set; }
        public Nullable<System.DateTime> Time { get; set; }
        public Nullable<int> Type { get; set; }
        public Nullable<decimal> Cost { get; set; }
        public Nullable<int> Finish { get; set; }
        public long Server_id { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Bill> Bill { get; set; }
        public virtual Table Table { get; set; }
    }
}
