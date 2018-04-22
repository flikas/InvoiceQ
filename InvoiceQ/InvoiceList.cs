using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InvoiceQ
{
    class InvoiceList : ObservableCollection<Invoice>
    {
        public InvoiceList()
        {
        }

        public InvoiceList(List<Invoice> list) : base(list)
        {
        }

        public InvoiceList(IEnumerable<Invoice> collection) : base(collection)
        {
        }
    }
}
