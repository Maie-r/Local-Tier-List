using Microsoft.Maui.Storage;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using static MudBlazor.CategoryTypes;
using Local_Tier_List.Components.TierListerComp;

namespace Local_Tier_List.Data.TierLists
{
    public class TierLister
    {
        public List<TierList> TierLists;
        public TierLister()
        {
            TierLists = new();
        }
        public TierLister(string DataJson)
        {
            try
            {
                var tl = LoadFromJson(DataJson);
                this.TierLists = tl;
                Debug.WriteLine(TierLists.Count + " Tierlists loaded!");
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                TierLists = new();
            }
        }
        
        // LOADING ----------------------
        public List<TierList> LoadFromJson(string json)
        {
            return TierListSaveData.ImportSaveData(json);
        }

        // SAVING -----------------------
        public static string ExportJson(TierLister tl)
        {
            return TierListSaveData.ExportTierList(tl.TierLists);
        }
        public static string ExportJson(TierList tl)
        {
            return TierListSaveData.ExportTierList(tl);
        }
        public static async Task<string> ExportJsonAsync(TierLister tl)
        {
            return await TierListSaveData.ExportJsonAsync(new TierListSaveData(tl));
        }
        
        public string ExportJson()
        {
            return ExportJson(this);
        }
        public async Task<string> ExportJsonAsync()
        {
            return await ExportJsonAsync(this);
        }
    }

    public class TierList
    {
        public Dictionary<string, Tier> tiers;
        public string name;
        public string color { get; set; } = "#a050dc";
        public DateTime lastModified { get; set; } = DateTime.UtcNow;

        public TierList(string name)
        {
            this.name = name;
            tiers = new Dictionary<string, Tier>();
        }

        public TierList(string name, bool templatefill)
        {
            this.name = name;
            tiers = new Dictionary<string, Tier>();
            if (templatefill)
                TemplateTiers();
        }

        public void ModifiedNow(TierItem item)
        {
            item.lastModified = DateTime.UtcNow;

            var tier = item.parent;
            if (tier != null)
            {
                tier.lastModified = item.lastModified;
                return;
            }
                
            this.lastModified = item.lastModified;
        }

        public void ModifiedNow(Tier tier)
        {
            tier.lastModified = DateTime.UtcNow;

            this.lastModified = tier.lastModified;
        }

        public void ModifiedNow()
        {
            this.lastModified = DateTime.UtcNow;
        }

        void TemplateTiers()
        {
            tiers.Add("S", new Tier("S", "#6603fcff"));
            tiers.Add("A", new Tier("A", "#d31cd9ff"));
            tiers.Add("B", new Tier("B", "#d92b59ff"));
            tiers.Add("C", new Tier("C", "#de6626ff"));
            tiers.Add("D", new Tier("D", "#e8b426ff"));
            tiers.Add("F", new Tier("F", "#6f6e78ff"));
        }

        public List<TierItem> GetAllItems()
        {
            List<TierItem> items = new List<TierItem>();
            foreach (Tier tier in tiers.Values)
            {
                items.AddRange(tier.items);
            }
            return items;
        }

        public string ExportJson()
        {
            return TierListSaveData.ExportTierList(this);
        }
    }

    public class Tier
    {
        public string name;
        public string ogname;
        public string color;
        public List<TierItem> items;
        public DateTime lastModified { get; set; } = DateTime.UtcNow;
        public Tier(string name, string color)
        {
            this.name = name;
            this.ogname = name;
            this.color = color;
            items = new List<TierItem>();
        }

        public Tier(string[] data)
        {
            //Debug.WriteLine(data[0]);
            this.name = data[0];
            this.ogname = data[0];
            this.color = data[1];
            items = new List<TierItem>();
        }

        public void Add(TierItem item)
        {
            items.Add(item);
            if (item.parent != this)
            {
                item.parent = this;
            }
        }

        public string AsString()
        {
            string res = "";
            res += $"Tier {name} (ogname {ogname}), color {color} with {items.Count} items";
            foreach (var i in items)
            {
                res += "\n" + i.AsString();
            }
            return res;
        }
    }

    public class TierItem
    {
        public string name { get; set; }
        public string img { get; set; }
        public string imgLocal { get; set; } //local address
        public byte[] imgBytes { get; set; }     
        public string imgMime { get; set; }
        public string[] tags { get; set; }
        public string notes { get; set; }
        public DateTime lastModified { get; set; } = DateTime.UtcNow;
        public Tier? parent { get; set; }
        public TierItemComp? ComponentInstance { get; set; }
        public int Renderversion { get; set; } = 1;

        public TierItem() { }

        public TierItem(string name) 
        {
            this.name = name;
            this.img = "";
        }

        public TierItem(string name, string img)
        {
            this.name = name;
            this.img = img;
        }

        public string AsString()
        {
            return $"TierItem {name}, img link: {img}. {tags.Length} tags" + (parent == null ? "No parent" : $"parent {parent!.name}");
        }

        //DEPRECATED
        public TierItem(string both, Tier parent)
        {
            string[] eh = both.Split(',');
            //Debug.WriteLine(both);
            name = eh[0];
            img = eh[1];
            this.parent = parent;
        }
    }
}
