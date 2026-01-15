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
        private static TierLister _tierlister;

        public static TierLister tl 
        { 
            get 
            { 
                if (_tierlister == null) 
                { 
                    _tierlister = new TierLister(); 
                }
                return _tierlister; 
            } 
        }

        public static List<TierList> TierLists 
        { 
            get 
            {
                return tl.TierLists; 
            } 
        }

        public static void SaveAll()
        {
            _tierlister.SaveAll();
        }

        public static List<TierList> Import(string ImportJson)
        {
            return _tierlister.LoadFromJson(ImportJson);
        }
    }
}
