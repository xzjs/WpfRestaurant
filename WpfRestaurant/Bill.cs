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
    
    public partial class Bill
    {
        public long Id { get; set; }
        public Nullable<long> Food_id { get; set; }
        public Nullable<int> Num { get; set; }
        public Nullable<long> Order_id { get; set; }
    
        public virtual Order Order { get; set; }
        public virtual Food Food { get; set; }
    }
}
