using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Local_Tier_List.Data
{
    public class TierService : ITier
    {
        private TierLister _tierlister;

        public TierService()
        {
            _tierlister = new TierLister();
        }

        public Task<TierLister> Start()
        {
            return Task.FromResult(_tierlister);
        }

        public async Task Reload()
        {
            _tierlister = null;
            await Task.Run(() => _tierlister = new TierLister());
        }
    }
}
