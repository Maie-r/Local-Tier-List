using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using Local_Tier_List.Data.TierLists;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Storage;
using MudBlazor;


namespace Local_Tier_List.Data.TierLists
{
    class TierListSaveData
    {
        public List<TierListDTO> TierLists { get; set; }

        public TierListSaveData()
        {
            TierLists = new();
        }

        public TierListSaveData(TierLister tl)
        {
            this.TierLists = new List<TierListDTO>();
            foreach (TierList t in tl.TierLists)
                this.TierLists.Add(TierListDTO.ToDTO(t));
        }
        public TierListSaveData(List<TierList> TierLists) 
        {
            this.TierLists = new List<TierListDTO>();
            foreach (TierList tl in TierLists)
                this.TierLists.Add(TierListDTO.ToDTO(tl));
        }

        // IMPORT -----------------------------------------
        /// <summary>
        /// Load from a Json that represents a list of Tier Lists (DTOS)
        /// </summary>
        /// <returns>Converted List<TierList></returns>
        public static List<TierList> ImportSaveData(string DataJson)
        {
            try
            {
                DataJson = ParseJson(DataJson);
                TierListSaveData? data = JsonSerializer.Deserialize<TierListSaveData>(DataJson, options);
                List<TierList> tierLists = new List<TierList>();
                if (data != null)
                {
                    foreach (TierListDTO tlDTO in data.TierLists)
                        tierLists.Add(TierListDTO.ToObject(tlDTO));
                }
                return tierLists;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading TierListSaveData from file: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Parses the specified JSON string and ensures it conforms to the expected TierLists format.
        /// </summary>
        /// <remarks>If the input is a single TierList object or an array of TierList objects, the method
        /// wraps it in an object with a 'TierLists' property. If the input already contains a 'TierLists' array at the
        /// root, it is returned unchanged. If the input does not match any of these formats, the method returns the
        /// original string after logging an error.</remarks>
        /// <param name="importString">A JSON string representing either a single TierList object, an array of TierList objects, or an object
        /// containing a 'TierLists' array.</param>
        /// <returns>A JSON string formatted with a top-level 'TierLists' array containing the input data, or the original string
        /// if it already matches the expected format.</returns>
        static string ParseJson(string importString)
        {
            JsonNode root = JsonNode.Parse(importString)!;

            if (root is JsonArray)
            {
                root = new JsonObject
                {
                    ["TierLists"] = root
                };
            }
            else if (root is JsonObject obj)
            {
                if (!obj.ContainsKey("TierLists"))
                {
                    if (obj.ContainsKey("name") && obj.ContainsKey("tiers"))
                    {
                        root = new JsonObject
                        {
                            ["TierLists"] = new JsonArray(obj)
                        };
                    }
                    else
                    {
                        throw new InvalidOperationException("Couldn't parse Json: Root Object is not a TierList or TierLists array");
                    }
                }
            }

            foreach (JsonNode? tierList in root["TierLists"]!.AsArray())
            {
                if (tierList is JsonObject tl)
                {
                    if (tl.ContainsKey("structureVersion"))
                    {
                        int version = tl["structureVersion"]!.GetValue<int>();
                        if (version < TierListDTO.CurrentStructureVersion)
                        {
                            FixLegacyData(tl, version);
                            tl["structureVersion"] = TierListDTO.CurrentStructureVersion;
                        }
                    }
                    else
                    {
                        FixLegacyData(tl, 1);
                        tl["structureVersion"] = TierListDTO.CurrentStructureVersion;
                    }
                }
                
            }

            return root.ToJsonString();
        }

        static void FixLegacyData(JsonObject root, int legacyversion)
        {
            Debug.WriteLine("Patching legacy data");
            for (; legacyversion < TierListDTO.CurrentStructureVersion; legacyversion++)
            {
                Debug.WriteLine($"Patching legacy data from version {legacyversion} to {legacyversion + 1}");

                switch (legacyversion)
                {
                    case 1:
                        foreach (JsonObject tier in root["tiers"]!.AsArray())
                        {
                            foreach (JsonObject item in tier["items"]!.AsArray().Cast<JsonObject>())
                            {
                                JsonObject? newImg = null;
                                if (item.ContainsKey("imgBytes") && item["imgBytes"] != null) // prioritizing this since they can contain unrecoverable data
                                {
                                    newImg = new JsonObject
                                    {
                                        ["bytes"] = item["imgBytes"],
                                        ["mime"] = item.ContainsKey("imgMime") ? item["imgMime"] : "image/jpeg"
                                    };
                                }
                                else if (item.ContainsKey("imgLocal") && item["imgLocal"] != null)
                                {
                                    newImg = new JsonObject
                                    {
                                        ["link"] = item["imgLocal"]
                                    };

                                }
                                else if (item.ContainsKey("img") && item["img"] != null)
                                {
                                    if (item["img"] is not JsonObject)
                                    {
                                        newImg = new JsonObject
                                        {
                                            ["link"] = item["img"]?.GetValue<string>()
                                        };
                                    }
                                }

                                if (newImg != null)
                                {
                                    item.Remove("img");
                                    item["img"] = newImg;
                                }

                                item.Remove("imgLocal");
                                item.Remove("imgBytes");
                                item.Remove("imgMime");
                            }
                        }
                        break;
                    case 2:
                        // No changes needed for version 3, the tierlist IDs generate themselves when possible
                        break;
                    default:
                        throw new InvalidOperationException($"Unsupported legacy version: {legacyversion}");
                }
            }

        }


        // EXPORT ------------------------------------------
        public static string ExportTierList(List<TierList> lists)
        {
            TierListSaveData saveData = new TierListSaveData(lists);
            string jsonString = JsonSerializer.Serialize(saveData, options);
            return jsonString;
        }

        public static string ExportTierList(TierList list)
        {
            TierListSaveData saveData = new TierListSaveData(new List<TierList>() { list });
            string jsonString = JsonSerializer.Serialize(saveData, options);
            return jsonString;
        }

        public static string ExportTierList(TierList[] lists)
        {
            TierListSaveData saveData = new TierListSaveData(new List<TierList>(lists));
            string jsonString = JsonSerializer.Serialize(saveData, options);
            return jsonString;
        }

        public static string ExportJson(TierListSaveData saveData)
        {
            try
            {
                return JsonSerializer.Serialize(saveData, options);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error exporting TierListSaveData as JSON: {ex.Message}");
            }
        }

        public string ExportJson()
        {
            return ExportJson(this);
        }

        public static async Task<string> ExportJsonAsync(TierListSaveData saveData)
        {
            try
            {
                await using var stream = new MemoryStream();
                await JsonSerializer.SerializeAsync(stream, saveData, options);
                return Encoding.UTF8.GetString(stream.ToArray());
            }
            catch (Exception ex)
            {
                throw new Exception($"Error export TierListSaveData as JSON: {ex.Message}");
            }
        }

        public async Task<string> ExportJsonAsync()
        {
            return await ExportJsonAsync(this);
        }

        static JsonSerializerOptions options = new()
        {
            //ReferenceHandler = ReferenceHandler.Preserve,
            //DefaultIgnoreCondition = JsonIgnoreCondition.Never,
            IncludeFields = true,
            WriteIndented = true
        };
    }

    class TierListDTO
    {
        public static int CurrentStructureVersion = 3; // This is what will be checked for handling legacy data. Please update whenever changes are made to the structure
        public string id { get; set; }
        public string name { get; set; }
        public List<TierDTO> tiers { get; set; }
        public string? lastModified { get; set; }
        public string color { get; set; }
        public int structureVersion { get; set; } = CurrentStructureVersion;

        public static TierListDTO ToDTO(TierList tierlist)
        {
            TierListDTO tlDTO = new TierListDTO();
            tlDTO.id = tierlist.Id.ToString("N");
            tlDTO.name = tierlist.name;
            tlDTO.color = tierlist.color;
            tlDTO.tiers = new List<TierDTO>();
            foreach (var tierPair in tierlist.tiers)
                tlDTO.tiers.Add(TierDTO.ToDTO(tierPair.Value));
            tlDTO.lastModified = tierlist.lastModified.ToString("O");

            return tlDTO;
        }

        public static TierList ToObject(TierListDTO tierlistDTO)
        {
            TierList tl = new TierList(tierlistDTO.name);
            if (Guid.TryParse(tierlistDTO.id, out Guid guid))
            {
                tl.Id = guid;
            }
            else
            {
                Debug.WriteLine("ERROR IMPORTING TIERLIST: Invalid GUID format, generating new GUID.");
                tl.Id = Guid.NewGuid();
            }
            tl.tiers = new Dictionary<string, Tier>();
            tl.color = tierlistDTO.color;
            foreach (TierDTO tDTO in tierlistDTO.tiers)
            {
                Tier t = TierDTO.ToObject(tDTO);
                tl.tiers.Add(t.name, t);
            }
            if (tierlistDTO.lastModified != null)
                tl.lastModified = DateTime.ParseExact(tierlistDTO.lastModified, "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);
            return tl;
        }

        public TierList ToObject()
        {
            try
            {
                return TierListDTO.ToObject(this);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error converting TierListDTO to TierList: {ex.Message}");
                throw;
            }
        }
    }

    class TierDTO
    {
        public string name { get; set; }
        public string color { get; set; }
        public List<TierItemDTO> items { get; set; }
        public string? lastModified { get; set; }

        static public TierDTO ToDTO(Tier tier)
        {
            TierDTO tDTO = new TierDTO();
            tDTO.name = tier.name;
            tDTO.color = tier.color;
            tDTO.items = new List<TierItemDTO>();
            foreach (TierItem ti in tier.items)
                tDTO.items.Add(TierItemDTO.ToDTO(ti));
            tDTO.lastModified = tier.lastModified.ToString("O");

            return tDTO;
        }

        static public Tier ToObject(TierDTO tierDTO)
        {
            Tier t = new Tier(tierDTO.name, tierDTO.color);
            t.items = new List<TierItem>();
            foreach (TierItemDTO tiDTO in tierDTO.items)
            {
                var ti = TierItemDTO.ToObject(tiDTO);
                ti.parent = t;
                t.items.Add(ti);
            }
            if (tierDTO.lastModified != null)
                t.lastModified = DateTime.ParseExact(tierDTO.lastModified, "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);
            return t;
        }

        public Tier ToObject()
        {
            try
            {
                return TierDTO.ToObject(this);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error converting TierDTO to Tier: {ex.Message}");
                throw;
            }
        }
    }

    class TierItemDTO
    {
        public string name { get; set; }
        public ImageSourceDTO img { get; set; }
        public string[] tags { get; set; }
        public string notes { get; set; }
        public string? lastModified { get; set; }

        public static TierItemDTO ToDTO(TierItem tierItem)
        {
            TierItemDTO tiDTO = new TierItemDTO();
            tiDTO.name = tierItem.name;
            tiDTO.img = ImageSourceDTO.ToDTO(tierItem.img);
            if (tiDTO.img.link != null) tiDTO.img.link = tiDTO.img.link.Trim();
            tiDTO.tags = tierItem.tags;
            tiDTO.lastModified = tierItem.lastModified.ToString("O");
            if (tiDTO.tags == null) tiDTO.tags = Array.Empty<string>();
            tiDTO.notes = tierItem.notes;
            return tiDTO;
        }

        public static TierItem ToObject(TierItemDTO tierItemDTO)
        {
            TierItem ti = new TierItem();
            ti.name = tierItemDTO.name;
            ti.img = ImageSourceDTO.ToObject(tierItemDTO.img);
            ti.tags = tierItemDTO.tags;
            if (tierItemDTO.lastModified != null)
                ti.lastModified = DateTime.ParseExact( tierItemDTO.lastModified, "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);
            if (ti.tags == null) ti.tags = Array.Empty<string>();
            ti.notes = tierItemDTO.notes;
            return ti;
        }

        public TierItem ToObject()
        {
            try
            {
                return TierItemDTO.ToObject(this);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error converting TierItemDTO to TierItem: {ex.Message}");
                throw;
            }
        }
    }

    public class ImageSourceDTO
    {
        public string? link { get; set; }
        public string? mime { get; set; }
        public byte[]? bytes { get; set; }

        public static ImageSourceDTO ToDTO(ImageSource source)
        {
            if (source is LinkedImage linkedImage)
                return new ImageSourceDTO() { link = linkedImage.link, mime = null, bytes = null };
            else if (source is MemoryImage memoryImage)
                return new ImageSourceDTO() { link = null, mime = memoryImage.mime, bytes = memoryImage.bytes };
            else
                throw new InvalidOperationException("Unknown ImageSource type");
        }

        public static ImageSource ToObject(ImageSourceDTO sourceDto)
        {
            if (sourceDto.bytes != null)
                if (sourceDto.mime != null)
                    return new MemoryImage(sourceDto.bytes, sourceDto.mime);
                else return new MemoryImage(sourceDto.bytes, "image/jpeg");
            else if (sourceDto.link != null)
                return new LinkedImage(sourceDto.link);
            else
                throw new InvalidOperationException("ImageSourceDTO must have either bytes or link defined");
        }
    }
}
