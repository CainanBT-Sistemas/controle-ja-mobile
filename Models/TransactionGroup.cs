using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace controle_ja_mobile.Models
{
    public class TransactionGroup : List<Transaction>
    {
        public string DateHeader { get; private set; }

        public TransactionGroup(string dateHeader, List<Transaction> transactions) : base(transactions)
        {
            DateHeader = dateHeader;
        }
    }
}
