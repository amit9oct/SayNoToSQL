using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SNTSBackend.Semantics
{
    class DataTableCustomComparator : IEqualityComparer<DataRow>
    {
        const int FORCE_EQUALITY_COMPARISON = 5;
        public bool Equals(DataRow x, DataRow y)
        {
            return x.ItemArray.SequenceEqual(y.ItemArray);
        }

        public int GetHashCode(DataRow obj)
        {
            return FORCE_EQUALITY_COMPARISON;
        }
        
    }
}
