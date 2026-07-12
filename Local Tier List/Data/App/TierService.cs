using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Local_Tier_List.Data.TierLists;
using MudBlazor.Charts;

namespace Local_Tier_List.Data.App
{
    public class TierService
    {
        private static TierLister _tierlister;
        private static TLFolder _rootfolder;

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

        public static TLFolder RootFolder
        {
            get
            {
                if (_rootfolder == null)
                {
                    _rootfolder = LoadTLFolders();
                }
                return _rootfolder;
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

        public static string GetFolderFile()
        {
            var p = GetMainFolder();
            return Path.Join(p, "folderstructure.json");
        }

        // SAVING          ------------------------------------------
        public async static Task SaveAll()
        {
            await SaveAllJson();
        }

        /// <summary>
        /// Returns the tierlists included ON ALL FOLDERS recursively, and adds any tierlists found in folders that are not in the main list to the main list.
        /// </summary>
        /// <returns></returns>
        static List<TierList> GetIncludedTL(TLFolder folder, Dictionary<Guid, TierList>alltierlists)
        {
            List<TierList> res = new();
            foreach (var t in folder.Folders)
            {
                var temp = GetIncludedTL(t, alltierlists);
                if (temp.Count > 0) res.AddRange(temp);
            }
            foreach (var l in folder.Tierlists)
            {
                if (alltierlists.TryGetValue(l.Id, out var found))
                    res.Add(found);
                else
                {
                    Debug.WriteLine("Found a tierlist in a folder that is not in the main list: " + l.name + " ID: " + l.Id);
                    Debug.WriteLine("Adding it in the main list");
                    alltierlists.Add(l.Id, l);
                    res.Add(l);
                }
            }
            return res;
        }

        static async Task SaveAllJson()
        {
            // Validating first, mainly to actually remove tierlists removed by an entire folder deletion
            if (RootFolder != null)
            {
                var missing = new List<TierList>();
                var lookup = TierLists.ToDictionary(t => t.Id);
                var res = GetIncludedTL(RootFolder, lookup);
                if (res.Count > 0) missing = TierLists.Except(res).ToList();

                foreach (var mis in missing)
                {
                    TierLists.Remove(mis);
                }
            }

            var json = await tl.ExportJsonAsync();
            await File.WriteAllTextAsync(GetMainFile(), json);
            await SaveFoldersJson();
        }

        static async Task SaveFoldersJson()
        {
            var content = "";
            if (RootFolder != null)
            {
                var folderDTO = TLFolderDTO.ToDTO(RootFolder);
                content = JsonSerializer.Serialize(folderDTO, options);
            }
            await File.WriteAllTextAsync(GetFolderFile(), content);
        }

        // LOADING         ------------------------------------------
        public static List<TierList> Import(string ImportJson)
        {
            // separate folders from tl data
            return _tierlister.LoadFromJson(ImportJson);
        }

        public static TLFolder LoadTLFolders()
        {
            try
            {
                var temp = File.ReadAllText(GetFolderFile());
                if (!String.IsNullOrEmpty(temp))
                {
                    TLFolderDTO? data = JsonSerializer.Deserialize<TLFolderDTO>(temp, options);
                    if (data != null)
                    {
                        var tls = TierLists;
                        var res = TLFolderDTO.ToObject(data, tls);
                        return res;
                    }
                }
            }
            catch (FileNotFoundException e)
            {

            }
            return new TLFolder() { Name = "Root", Tierlists = new List<TierList>(TierService.TierLists), Color = "#ffffff00" };
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
                Debug.WriteLine("Failed to load data: " + e.Message);
                return new TierLister();
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

        static JsonSerializerOptions options = new()
        {
            //ReferenceHandler = ReferenceHandler.Preserve,
            //DefaultIgnoreCondition = JsonIgnoreCondition.Never,
            IncludeFields = true,
            WriteIndented = true
        };

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
