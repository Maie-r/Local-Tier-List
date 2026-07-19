using System;
using System.Collections.Generic;
using System.Text;

namespace Local_Tier_List.Data.App
{
    public enum TLListModalType
    {
        TLEdit,
        FolderEdit,
        TLDelete,
        FolderDelete,
        None
    }

    public enum IOModalType
    {
        Import,
        Export,
        None,
    }
}
