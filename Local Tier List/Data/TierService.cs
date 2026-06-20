using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Diagnostics;
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
                    _tierlister = LoadTierLister(); 
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

        // DEFAULT FILE LOCATIONS  ------------------------------------------
        public static string GetMainFolder()
        {
            var p = Path.Join(FileSystem.AppDataDirectory, "TierList");
            if (!Directory.Exists(p))
            {
                Debug.WriteLine("Folder not found, creating...");
                Directory.CreateDirectory(p);
            }
            return p;
        }

        public static string GetMainFile()
        {
            var p = GetMainFolder();
            return Path.Join(p, "lists.json");
        }

        // SAVING          ------------------------------------------
        public async static Task SaveAll()
        {
            try
            {
                await SaveAllJson();
            }
            catch (Exception e)
            {
                throw new Exception("Couldn't save the tierlist data: " + e);
            }
        }

        static async Task SaveAllJson()
        {
            var json = await tl.ExportJsonAsync();
            await File.WriteAllTextAsync(GetMainFile(), json);
        }

        // LOADING         ------------------------------------------
        public static List<TierList> Import(string ImportJson)
        {
            return _tierlister.LoadFromJson(ImportJson);
        }
    
        public static TierLister LoadTierLister()
        {
            try
            {
                var temp = File.ReadAllText(GetMainFile());
                return new TierLister(temp);
            }
            catch (FileNotFoundException e)
            {
                Debug.WriteLine("Attempting loading with legacy options...");
                // Legacy fallback
                try
                {
                    var TierLists = new List<TierList>();
                    //Debug.WriteLine("loading from " + link);
                    var link = Path.Join(GetMainFolder(), "lists.txt");

                    if (!File.Exists(link))
                    {
                        File.WriteAllText(link, string.Empty);
                    }
                    string content = File.ReadAllText(link);
                    //Debug.WriteLine(content);
                    string[] tierlists = content.Split(';');
                    foreach (string tierlist in tierlists) // every tier list
                    {
                        //Debug.WriteLine("Ahoy!");
                        TierList temp = ReadList(tierlist);
                        TierLists.Add(temp);
                    }
                    Debug.WriteLine("Loaded legacy data and parsed it into current structure successfully!");
                    return new TierLister() { TierLists = TierLists };
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Failed to load tierlists: " + ex.Message);
                    return new TierLister();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Failed to load data: " + ex.Message);
                return new TierLister();
            }
        }

        // DEPRECATED      ------------------------------------------
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
                    temp.tiers.Add(temp2.name, temp2);
                }
                catch
                {
                    temp.tiers.Add(temp2.name + "(1)", temp2);
                }
                //Debug.WriteLine(sep2.Length);
                for (int j = 1; j < sep2.Length; j++) // every item
                {
                    if (sep2[j] == "") { break; }
                    temp.tiers[temp2.name].items.Add(new TierItem(sep2[j], temp2));
                }

            }
            return temp;
        }
    }
}
