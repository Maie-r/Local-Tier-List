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
                    Debug.WriteLine(sep2.Length);
                    for (int j = 1; j < sep2.Length; j++) // every item
                    {
                        if (sep2[j] == "") { break; }
                        if (!temp.list.ContainsKey(sep2[0]))
                        {
                            temp.list.Add(sep2[0], new List<TierItem>());
                        }
                        temp.list[sep2[0]].Add(new TierItem(sep2[j]));
                    }
                    if (!temp.list.ContainsKey(sep2[0]))
                    {
                        temp.list.Add(sep2[0], new List<TierItem>());
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
                foreach (var kv in list.list)
                {
                    result += "/-/" + kv.Key + "\r\n";
                    foreach (var item in kv.Value)
                    {
                        Debug.WriteLine($"{item.name}, {item.img}");
                        result += $"{item.name}, {item.img}" + "\r\n";
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
        public Dictionary<string, List<TierItem>> list;
        public string name;

        public TierList(string name)
        {
            this.name = name;
            list = new Dictionary<string, List<TierItem>>();
        }
    }

    public class TierItem
    {
        public string name;
        public string img;

        public TierItem(string both)
        {
            string[] eh = both.Split(',');
            Debug.WriteLine(both);
            name = eh[0];
            img = eh[1];
        }
    }
}
