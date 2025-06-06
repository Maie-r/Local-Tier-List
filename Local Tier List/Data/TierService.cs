using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MaieBlazorLib.LocalTierLister;

namespace Local_Tier_List.Data
{
    public class TierService
    {
        private static TierLister _tierlister = new TierLister();

        public static TierLister tl { get { return _tierlister; } }

        public static List<TierList> TierLists 
        { get { return _tierlister.TierLists; } }

        public static void SaveAll()
        {
            _tierlister.SaveAll();
        }
    }
}
