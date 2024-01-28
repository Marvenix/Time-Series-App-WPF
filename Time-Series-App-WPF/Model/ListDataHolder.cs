using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Time_Series_App_WPF.Model
{
    public class ListDataHolder<T>
    {
        public ICollection<T>? Data { get; set; }
    }
}
