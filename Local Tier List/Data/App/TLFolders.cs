using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using Local_Tier_List.Data.TierLists;

namespace Local_Tier_List.Data.App
{
    public class TLFolder
    {
        public string Name { get; set; } = "New Folder";
        public string Color { get; set; } = "#a050dc";
        public List<TierList> Tierlists { get; set; } = new List<TierList>();
        public List<TLFolder> Folders { get; set; } = new List<TLFolder>();

        public static List<TLFolder> GetAllSubFolders(TLFolder folder)
        {
            var res = new List<TLFolder>();
            foreach (var f in folder.Folders)
            {
                res.Add(f);
                res.AddRange(GetAllSubFolders(f));
            }
            return res;
        }
        public static List<TierList> GetAllTierlists(TLFolder folder)
        {
            var res = new List<TierList>();
            res.AddRange(folder.Tierlists);
            foreach (var f in folder.Folders)
            {
                res.AddRange(GetAllTierlists(f));
            }
            return res;
        }
    }

    public class TLFolderDTO
    {
        public string Name { get; set; } = "New Folder";
        public string Color { get; set; } = "#a050dc";
        public string[] Tierlists { get; set; } = new string[0];
        public List<TLFolderDTO> Folders { get; set; } = new List<TLFolderDTO>();

        public static TLFolderDTO ToDTO(TLFolder folder)
        {
            var tiDTO = new TLFolderDTO();
            tiDTO.Name = folder.Name;
            tiDTO.Color = folder.Color;
            tiDTO.Tierlists = folder.Tierlists.ConvertAll(tl => tl.Id.ToString("N")).ToArray();
            tiDTO.Folders = folder.Folders.ConvertAll(f => ToDTO(f));
            return tiDTO;
        }

        public static TLFolder ToObject(TLFolderDTO folderDTO, List<TierList> tierlists)
        {
            var folder = new TLFolder();

            folder.Name = folderDTO.Name;
            folder.Color = folderDTO.Color;

            foreach (var tlid in folderDTO.Tierlists)
            {
                var tl = tierlists.Find(t => t.Id.ToString("N") == tlid);
                if (tl != null)
                {
                    folder.Tierlists.Add(tl);
                }
                else Debug.WriteLine("Couldn't find TierList with ID: " + tlid + " for organizing in folders, it will reset to the root");
            }
            folder.Folders = folderDTO.Folders.ConvertAll(f => ToObject(f, tierlists));
            return folder;
        }
    }
}
