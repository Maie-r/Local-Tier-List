using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Local_Tier_List.Data
{
    public class TierLister
    {
        public List<TierList> TierLists;
        public TierLister()
        {
            LoadStuff();
        }

        void LoadStuff()
        {
            TierLists = new List<TierList>();
            string folder = GetFolder();
            Directory.CreateDirectory(folder);
            if (!File.Exists($"{folder}lists.txt"))
            {
                File.WriteAllText($"{folder}lists.txt", "");
            }
            string[] tierlists = File.ReadAllText($"{folder}lists.txt").Split(';');
            foreach (string tierlist in tierlists) // every tier list
            {
                string[] sep1 = tierlist.Split("/-/");
                TierList temp = new TierList(sep1[0]);
                for (int i = 1; i < sep1.Length; i++) // every tier
                {
                    string[] sep2 = sep1[i].Split("\r\n");
                    Tier temp2 = new Tier(sep2[0].Split(','));
                    try
                    {
                        temp.list.Add(temp2.name, temp2);
                    }
                    catch
                    {
                        temp.list.Add(temp2.name + "(1)", temp2);
                    }
                    //Debug.WriteLine(sep2.Length);
                    for (int j = 1; j < sep2.Length; j++) // every item
                    {
                        if (sep2[j] == "") { break; }
                        temp.list[temp2.name].items.Add(new TierItem(sep2[j], temp2));
                    }

                }
                TierLists.Add(temp);
            }
        }

        public void SaveAll()
        {
            string folder = GetFolder();
            string result = "";
            int i = 0;
            foreach (var list in TierLists) // each tier list
            {
                i++;
                int j = 0;
                result += list.name;
                foreach (Tier tier in list.list.Values)
                {
                    result += $"/-/{tier.name},{tier.color}\r\n";
                    foreach (var item in tier.items)
                    {
                        //Debug.WriteLine($"{item.name}, {item.img}");
                        result += $"{item.name},{item.img}" + "\r\n";
                    }
                }
                if (i < TierLists.Count)
                {
                    result += ";";
                }

            }
            File.WriteAllText(folder + "lists.txt", result);
        }

        static string GetFolder()
        {
            return AppDomain.CurrentDomain.BaseDirectory + @"TierList\";
        }
    }

    public class TierList
    {
        public Dictionary<string, Tier> list;
        public string name;

        public TierList(string name)
        {
            this.name = name;
            list = new Dictionary<string, Tier>();
        }
    }

    public class Tier
    {
        public string name;
        public string ogname;
        public string color;
        public List<TierItem> items;
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
        }
    }

    public class TierItem
    {
        public string name;
        public string img;
        public Tier parent;

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
