using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LocalTierListLib
{
    public class TierLister
    {
        public List<TierList> TierLists;
        public TierLister()
        {
            LoadStuff();
        }

        public TierLister(string link)
        {
            LoadStuff(link);
        }

        public static TierList GetSpecificList(string link, string name)
        {
            if (File.Exists(link))
            {
                string[] tierlists = File.ReadAllText(link).Split(';');
                foreach (string tierlist in tierlists) // every tier list
                {
                    if (tierlist.Split("/-/")[0].Contains(name))
                        return ReadList(tierlist);
                }
            }
            /*StreamReader sw = new StreamReader(link);
            string line;
            for (line = sw.ReadLine(); !line.Contains(name) && line != null; line = sw.ReadLine());
            if (line != null) // if the list does not exist
            {
                string list = "\r\n" + line;
                while (!line.StartsWith(';') && line != null)
                {
                    Debug.WriteLine(line);
                    list += line;
                    line = sw.ReadLine();
                }
                list += "\r\n";
                return ReadList(list);
            }*/
            Debug.WriteLine($"Couldn't find list {name} at {link} !!");
            return null;
        }

        static List<TierList> LoadFrom(string link)
        {
            List<TierList> TierLists = new List<TierList>();
            if (!File.Exists(link))
            {
                return null;
            }
            string[] tierlists = File.ReadAllText($"link").Split(';');
            foreach (string tierlist in tierlists) // every tier list
            {
                TierList temp = ReadList(tierlist);
                TierLists.Add(temp);
            }
            return TierLists;
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
            Debug.WriteLine(tierlists[0]);
            foreach (string tierlist in tierlists) // every tier list
            {
                TierList temp = ReadList(tierlist);
                TierLists.Add(temp);
            }
        }

        void LoadStuff(string link)
        {
            TierLists = new List<TierList>();
            if (!File.Exists(link))
            {
                File.WriteAllText(link, "");
            }
            string[] tierlists = File.ReadAllText(link).Split(';');
            foreach (string tierlist in tierlists) // every tier list
            {
                TierList temp = ReadList(tierlist);
                TierLists.Add(temp);
            }
        }

        static TierList ReadList(string tierlist)
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
            return temp;
        }

        /// <summary>
        /// It will only update by replacing a tierlist with the same name as the one in the parameter
        /// </summary>
        public static void SaveSpecific(string link, TierList newtl)
        {
            List<TierList> TierLists = LoadFrom(link);
            for (int j = 0; j < TierLists.Count; j++)
            {
                if (TierLists[j].name == newtl.name)
                {
                    TierLists[j] = newtl;
                    break;
                }
            }
            string result = "";
            int i = 0;
            foreach (var list in TierLists) // each tier list
            {
                i++;
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
            File.WriteAllText(link, result);
        }

        public void SaveAll()
        {
            string folder = GetFolder();
            string result = "";
            int i = 0;
            foreach (var list in TierLists) // each tier list
            {
                i++;
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

        public TierList(string name, bool templatefill)
        {
            this.name = name;
            list = new Dictionary<string, Tier>();
            if (templatefill)
                TemplateTiers();
        }

        void TemplateTiers()
        {
            list.Add("S", new Tier("S", "#6603fcff"));
            list.Add("A", new Tier("A", "#d31cd9ff"));
            list.Add("B", new Tier("B", "#d92b59ff"));
            list.Add("C", new Tier("C", "#de6626ff"));
            list.Add("D", new Tier("D", "#e8b426ff"));
            list.Add("F", new Tier("F", "#6f6e78ff"));
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
