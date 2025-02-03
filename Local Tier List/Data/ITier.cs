using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Local_Tier_List.Data
{
    public interface ITier
    {
        Task<TierLister>? Start();
        Task Reload();
    }
}
