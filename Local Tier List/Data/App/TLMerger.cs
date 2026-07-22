using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using static MudBlazor.CategoryTypes;
using Local_Tier_List.Data.TierLists;
using AngleSharp.Common;
using System.Collections;

namespace Local_Tier_List.Data.App
{
    public static class TLMerger
    {
        public static TierList MergeTierLists(TierList current, TierList incoming, MergeOptions options)
        {
            var sw = new Stopwatch();
            sw.Start();
            TierList res = options.TierListAttributes.MergeTierlistAttributes(current, incoming);
            var tiermap = MergeBehaviour.GetTierMap(current.tiers, incoming.tiers, options.AllowNewTiers);
            var tierPos = options.TierPositions.MergeTierPositionOnly(tiermap);
            var tierAtt = options.TierAttributes.MergeTierAttributesOnly(tiermap);
            var usedTiers = new Dictionary<string, MergeBehaviour.TierPosition>(tierPos);
            foreach (var pair in tierAtt)
                usedTiers[pair.Key] = pair.Value;

            //Debug.WriteLine($"TierCounts: {tiermap.Count} {tierPos.Count} {tierAtt.Count} {usedTiers.Count}");

            var itemmap = MergeBehaviour.GetItemMap(current.tiers, incoming.tiers, usedTiers, options.NewItemBehaviour);
            var itemPos = options.ItemPositions.MergeItemPositionsOnly(itemmap);
            var itemAtt = options.ItemAttributes.MergeItemAttributesOnly(itemmap, options.ItemAttributeWhitelist, options.KeepData);
            var usedItems = new Dictionary<string, MergeBehaviour.ItemPosition>(itemPos);
            foreach (var pair in itemAtt)
                usedItems[pair.Key].item = pair.Value.item;

            //Debug.WriteLine($"ItemCounts: {itemmap.Count} {itemPos.Count} {itemAtt.Count} {usedItems.Count}");

            res.tiers = MergeBehaviour.RebuildTiers(usedTiers, usedItems, options.NewItemBehaviour);

            sw.Stop();
            Debug.WriteLine($"Merge finished in {sw.ElapsedMilliseconds}ms");

            return res;
        }
    }
}

public abstract class MergeBehaviour
{
    // For simple merges, only these comparasions need to be changed, but others can too for optimization purposes
    /// <summary>
    /// Compare the two tierlists and return the one in priority for the merge
    /// </summary>
    public abstract TierList Compare(TierList current, TierList incoming);
    /// <summary>
    /// Compare the two tiers and return the one in priority for the merge
    /// </summary>
    public abstract Tier Compare(Tier current, Tier incoming);
    /// <summary>
    /// Compare the two tieritems and return the one in priority for the merge
    /// </summary>
    public abstract TierItem Compare(TierItem current, TierItem incoming);

    internal virtual TierPosition Compare(TierPositionDuo tierduo)
    {
        if (tierduo.incoming == null) 
        {
            if (tierduo.current == null)
                throw new ArgumentException("[TLMERGER] No tiers were bound to a tierduo!");
            else
                return tierduo.current;
        }
        else if (tierduo.current == null)
        {
            return tierduo.incoming;
        }
        else
        {
            Tier used = Compare(tierduo.current.tier, tierduo.incoming.tier);
            if (tierduo.current.tier == used) return tierduo.current;
            else return tierduo.incoming;
        }
        
    }

    internal virtual ItemPosition Compare(ItemPositionDuo duo)
    {
        if (duo.incoming == null)
        {
            if (duo.current == null)
                throw new ArgumentException("[TLMERGER] No items were bound to a itemduo!");
            else
                return duo.current;
        }
        else if (duo.current == null)
        {
            return duo.incoming;
        }
        else
        {
            TierItem used = Compare(duo.current.item, duo.incoming.item);
            if (duo.current.item == used) return duo.current;
            else return duo.incoming;
        }

    }

    /// <summary>
    /// Makes a complete merge using the same merge behavior for all steps
    /// </summary>
    public TierList Merge(TierList current, TierList incoming, bool AcceptNewElements)
    {
        var res = MergeTierlistAttributes(current, incoming);

        var tiermap = GetTierMap(current.tiers, incoming.tiers, AcceptNewElements);
        var restiers = MergeTierPositionOnly(tiermap);

        var newitembehaviour = AcceptNewElements ? NewTierItemBehaviour.AllowAllNew : NewTierItemBehaviour.DontAllowAnyNew;
        var itemmap = GetItemMap(current.tiers, incoming.tiers, restiers, newitembehaviour);
        var resitemsPos = MergeItemPositionsOnly(itemmap);
        var resitemsAtt = MergeItemAttributesOnly(itemmap, MergeOptions.ItemAttributeWhilelistBase, true);
        var useditems = new Dictionary<string, ItemPosition>(resitemsPos);
        foreach (var pair in resitemsAtt)
            useditems[pair.Key].item = pair.Value.item;

        res.tiers = RebuildTiers(restiers, useditems, newitembehaviour);
        return res;
    }

    /// <summary>
    /// To init the base tierlist to be returned, decide between which name and color to use.
    /// </summary>
    internal virtual TierList MergeTierlistAttributes(TierList current, TierList incoming)
    {
        var used = Compare(current, incoming);
        return new TierList(used.name, used.color);
    }

    internal static Dictionary<string, TierPositionDuo> GetTierMap(Dictionary<string, Tier> current, Dictionary<string, Tier> incoming, bool AllowNewTiers)
    {
        var map = new Dictionary<string, TierPositionDuo>();
        int i = 0;
        foreach (var pair in current)
        {
            map.Add(pair.Key, new TierPositionDuo()
            {
                current = new TierPosition()
                {
                    tier = pair.Value,
                    index = i
                }
            });
            i++;
        }
        i = 0;
        foreach (var pair in incoming)
        {
            if (map.ContainsKey(pair.Key))
            {
                map[pair.Key].incoming = new TierPosition()
                {
                    tier = pair.Value,
                    index = i
                };
                i++;
            }
            else if (AllowNewTiers)
            {
                map.Add(pair.Key, new TierPositionDuo()
                {
                    incoming = new TierPosition()
                    {
                        tier = pair.Value,
                        index = i,
                        IsNew = true
                    }
                });
                i++;
            }
        }
        return map;
    }

    internal virtual Dictionary<string, TierPosition> MergeTierPositionOnly(Dictionary<string, TierPositionDuo> map) // If the attribute merge is the same, it doesn't need to be called
    {
        var used = new Dictionary<string, TierPosition>();
        foreach (var duo in map.Values)
        {
            var u = Compare(duo);

            used.Add(u.tier.name, u);
        }

        return used.Values.OrderBy(f => f.index).ToDictionary(e => e.tier.name, e => e);
    }

    internal virtual Dictionary<string, TierPosition> MergeTierAttributesOnly(Dictionary<string, TierPositionDuo> map)
    {
        var res = new Dictionary<string, TierPosition>();
        foreach (var pair in map)
        {
            var u = Compare(pair.Value);
            res.Add(u.tier.name, u);
        }

        return res;
    }

    internal static Dictionary<string, ItemPositionDuo> GetItemMap(Dictionary<string, Tier> current, Dictionary<string, Tier> incoming, Dictionary<string, TierPosition> used, NewTierItemBehaviour newItemBehaviour)
    {
        var map = new Dictionary<string, ItemPositionDuo>();
        int NewGroupCount = 0;
        int i = 0;
        foreach (var pair in current)
        {
            foreach (var item in pair.Value.items)
            {
                map.Add(item.name, new ItemPositionDuo()
                {
                    current = new ItemPosition()
                    {
                        item = item,
                        tierkey = pair.Key,
                        index = i
                    }
                });
                i++;
            }
        }
        i = 0;
        foreach (var pair in incoming)
        {
            foreach (var item in pair.Value.items)
            {
                if (map.ContainsKey(item.name))
                {
                    map[item.name].incoming = new ItemPosition()
                    {
                        item = item,
                        tierkey = pair.Key,
                        index = i
                    };
                    i++;
                }
                else
                {
                    var usedbehaviour = newItemBehaviour == NewTierItemBehaviour.AllowAllNew && !used.ContainsKey(pair.Key) ? NewTierItemBehaviour.AllowAllAndGroup : newItemBehaviour;

                    switch (usedbehaviour)
                    {
                        case NewTierItemBehaviour.DontAllowAnyNew:

                            break;
                        case NewTierItemBehaviour.AllowAllNew:
                            {
                                map.Add(item.name, new ItemPositionDuo()
                                {
                                    incoming = new ItemPosition()
                                    {
                                        item = item,
                                        tierkey = pair.Key,
                                        index = i
                                    }
                                });
                                i++;
                            }
                            break;
                        case NewTierItemBehaviour.AllowAllAndGroup:
                            {
                                if (!used!.ContainsKey(DefaultNewItemTierName)) used.Add(DefaultNewItemTierName, new TierPosition() { tier = new Tier(DefaultNewItemTierName, "#949494"), index = used.Count, IsNew = true});

                                map.Add(item.name, new ItemPositionDuo()
                                {
                                    incoming = new ItemPosition()
                                    {
                                        item = item,
                                        tierkey = DefaultNewItemTierName,
                                        index = NewGroupCount
                                    }
                                });
                                NewGroupCount++;
                            }
                            break;
                        case NewTierItemBehaviour.AllowOnlyInExistingTiers:
                            {
                                if (used != null && used.ContainsKey(pair.Key) && !used[pair.Key].IsNew)
                                {
                                    map.Add(item.name, new ItemPositionDuo()
                                    {
                                        incoming = new ItemPosition()
                                        {
                                            item = item,
                                            tierkey = pair.Key,
                                            index = i
                                        }
                                    });
                                    i++;
                                }
                            }
                            break;
                        case NewTierItemBehaviour.AllowOnlyInNewTiers:
                            {
                                if (used != null && used.ContainsKey(pair.Key) && used[pair.Key].IsNew)
                                {
                                    map.Add(item.name, new ItemPositionDuo()
                                    {
                                        incoming = new ItemPosition()
                                        {
                                            item = item,
                                            tierkey = pair.Key,
                                            index = i
                                        }
                                    });
                                    i++;
                                }
                            }
                            break;
                    }
                }
            }
        }

        return map;
    }

    internal virtual Dictionary<string, ItemPosition> MergeItemPositionsOnly(Dictionary<string, ItemPositionDuo> itemMap)
    {
        //bool HasNewTiers = used.Any((e) => e.Value.IsNew);
        
        //var order = new Dictionary<string, OrderedDictionary<int, TierItem>>();
        var shallowRes = new Dictionary<string, ItemPosition>();
        foreach (var key in itemMap.Keys)
        {
            var u = Compare(itemMap[key]);
            shallowRes.Add(u.item.name, u);
        }

        return shallowRes;
    }

    internal virtual Dictionary<string, ItemPosition> MergeItemAttributesOnly(Dictionary<string, ItemPositionDuo> itemMap, IEnumerable<string> whitelist, bool KeepData)
    {
        var shallowRes = new Dictionary<string, ItemPosition>();
        foreach (var key in itemMap.Keys)
        {
            ItemPosition u = FilterAttributes(itemMap[key], whitelist, KeepData);
            shallowRes.Add(u.item.name, u);
        }

        return shallowRes;
    }

    internal virtual ItemPosition FilterAttributes(ItemPositionDuo duo, IEnumerable<string> whitelist, bool KeepData)
    {
        if (duo.current == null)
        {
            if (duo.incoming == null)
                throw new ArgumentException("[TLMERGE] No items were bound to an Item duo!");
            else
                return duo.incoming;
        }
        else
        {
            if (duo.incoming == null)
                return duo.current;
        }
        TierItem used = Compare(duo.current.item, duo.incoming.item);
        TierItem fallback = used == duo.current.item ? duo.incoming.item : duo.current.item;
        TierItem current = duo.current.item;

        ItemPosition res = new ItemPosition() { item = new TierItem(used.name) };
        if (whitelist.Contains("Image"))
        {
            if (KeepData && (used.img == null ||
            used.img is LinkedImage li && String.IsNullOrEmpty(li.link) ||
            used.img is MemoryImage mi && (mi.bytes == null || mi.bytes.Length <= 0)))
            {
                res.item.img = fallback.img;
            }
            else res.item.img = used.img;
        }
        else res.item.img = current.img;

        if (whitelist.Contains("Tags"))
        {
            if (KeepData)
            {
                if (used.tags == null || used.tags.Length <= 0)
                    res.item.tags = fallback.tags;
                else if (fallback.tags == null || fallback.tags.Length <= 0)
                    res.item.tags = used.tags;
                else
                {
                    var map = used.tags.ToHashSet();
                    foreach (var tag in fallback.tags)
                    {
                        map.Add(tag);
                    }
                    map.RemoveWhere(e => String.IsNullOrEmpty(e));
                    res.item.tags = map.ToArray();
                }
            }
            else res.item.tags = used.tags;
        }
        else res.item.tags = current.tags;

        if (whitelist.Contains("Notes"))
        {
            if (KeepData) res.item.notes = string.IsNullOrEmpty(used.notes) ? fallback.notes : used.notes;
            else res.item.notes = used.notes;
        }
        else res.item.notes = current.notes;

        return res;
    }

    internal virtual ItemPosition DataKeepMerge(ItemPositionDuo duo)
    {
        if (duo.current == null)
        {
            if (duo.incoming == null)
                throw new ArgumentException("[TLMERGE] No items were bound to an Item duo!");
            else
                return duo.incoming;
        }
        else
        {
            if (duo.incoming == null)
                return duo.current;
        }
        var used = Compare(duo.current.item, duo.incoming.item);
        var fallback = used == duo.current.item ? duo.incoming.item : duo.current.item;
        var item = new TierItem(used.name);
        if (used.img == null ||
            used.img is LinkedImage li && String.IsNullOrEmpty(li.link) ||
            used.img is MemoryImage mi && (mi.bytes == null || mi.bytes.Length <= 0))
        {
            item.img = fallback.img;
        }
        else item.img = used.img;

        item.notes = string.IsNullOrEmpty(used.notes) ? fallback.notes : used.notes;

        if (used.tags == null || used.tags.Length <= 0)
            item.tags = fallback.tags;
        else if (fallback.tags == null || fallback.tags.Length <= 0)
            item.tags = used.tags;
        else
        {
            var map = used.tags.ToHashSet();
            foreach (var tag in fallback.tags)
            {
                map.Add(tag);
            }
            map.RemoveWhere(e => String.IsNullOrEmpty(e));
            item.tags = map.ToArray();
        }

        item.parent = used.parent;

        if (used == duo.current.item)
            return new ItemPosition() { item = item, index = duo.current.index, tierkey = duo.current.tierkey };
        else
            return new ItemPosition() { item = item, index = duo.incoming.index, tierkey = duo.incoming.tierkey };
    }

    internal static Dictionary<string, Tier> RebuildTiers(Dictionary<string, TierPosition> tiers, Dictionary<string, ItemPosition> items, NewTierItemBehaviour newItemBehaviour)
    {
        var res = new Dictionary<string, Tier>();
        foreach (var tierpair in tiers)
        {
            var clone = new Tier(tierpair.Value.tier.name, tierpair.Value.tier.color);
            res.Add(tierpair.Key, clone);
        }

        var unordered = new Dictionary<string, List<ItemPosition>>();
        foreach (var key in items.Keys)
        {
            var clone = items[key].item.Clone();
            if (!res.ContainsKey(items[key].tierkey))
            {
                if (newItemBehaviour != NewTierItemBehaviour.AllowOnlyInExistingTiers)
                {
                    if (!res.ContainsKey(DefaultNewItemTierName))
                        res.Add(DefaultNewItemTierName, new Tier(DefaultNewItemTierName, "#949494"));

                    clone.parent = res[DefaultNewItemTierName];
                    items[key].item = clone;
                }
            }
            else
            {
                clone.parent = res[items[key].tierkey];
                items[key].item = clone;
            }
            clone.parent = res[items[key].tierkey];
            items[key].item = clone;

            if (!unordered.ContainsKey(items[key].tierkey))
                unordered.Add(items[key].tierkey, new List<ItemPosition>());

            var temppos = new ItemPosition() { item = clone, index = items[key].index, tierkey = items[key].tierkey };
            unordered[items[key].tierkey].Add(temppos);
        }

        foreach (var pair in unordered)
        {
            var ordered = pair.Value.OrderBy(e => e.index);
            res[pair.Key].items = ordered.Select(e => e.item).ToList();
            
        }
        //Debug.WriteLine($"rebuilt counts: {res.Count} {unordered.Count}");
        return res;
    }

    static string DefaultNewItemTierName = "New Items";

    internal class TierPositionDuo
    {
        public TierPosition current;
        public TierPosition incoming;
    }

    internal class TierPosition
    {
        public Tier tier;
        public int index;
        public bool IsNew;
    }

    internal class ItemPositionDuo
    {
        public ItemPosition current;
        public ItemPosition incoming;
    }
    internal class ItemPosition
    {
        public TierItem item;
        public string tierkey;
        public int index;
    }
}

public enum NewTierItemBehaviour
{
    DontAllowAnyNew,
    AllowAllNew,
    AllowOnlyInExistingTiers,
    AllowOnlyInNewTiers,
    AllowAllAndGroup
}

public class MergeOptions
{
    /// <summary>
    /// Merge Behaviour for Tierlist name and color
    /// </summary>
    /// <remarks>
    /// Set to "None" by default
    /// </remarks>
    public MergeBehaviour TierListAttributes { get; set; } = MergeBehaviourTable.None;
    /// <summary>
    /// Whether to override Merge Behaviour to keep whichever attribute is not empty
    /// </summary>
    /// <remarks>
    /// Set to True by default
    /// </remarks>
    public bool KeepData { get; set; } = true;

    /// <summary>
    /// Merge Behaviour for Tier color
    /// </summary>
    /// <remarks>
    /// Set to "None" by default
    /// </remarks>
    public MergeBehaviour TierAttributes { get; set; } = MergeBehaviourTable.None;
    /// <summary>
    /// Merge Behaviour for Tier order
    /// </summary>
    /// <remarks>
    /// Set to "None" by default
    /// </remarks>
    public MergeBehaviour TierPositions { get; set; } = MergeBehaviourTable.None;
    /// <summary>
    /// Whether to accept incoming Tiers that are not in the current Tierlist
    /// </summary>
    /// <remarks>
    /// Set to True by default
    /// </remarks>
    public bool AllowNewTiers { get; set; } = true;

    /// <summary>
    /// Merge Behaviour for Tier Item attributes (image, tags, notes)
    /// </summary>
    /// <remarks>
    /// Set to "None" by default
    /// </remarks>
    public MergeBehaviour ItemAttributes { get; set; } = MergeBehaviourTable.None;
    /// <summary>
    /// Merge Behaviour for Tier Item order and position (Which Tier and where in the Tier it's positioned)
    /// </summary>
    /// <remarks>
    /// Set to "None" by default
    /// </remarks>
    public MergeBehaviour ItemPositions { get; set; } = MergeBehaviourTable.None;
    /// <summary>
    /// Behaviour when encountering incoming items that aren't in the current Tierlist.
    /// <list type="bullet">
    /// <item> NewTierItemBehaviour.DontAllowAnyNew - Don't accept any incoming items not already in the current Tierlist </item>
    /// <item> NewTierItemBehaviour.AllowAllNew - Allow all new items in any Tier (if a Tier is not present, it will be added in a special Tier)</item>
    /// <item> NewTierItemBehaviour.AllowOnlyInExistingTiers - Allow new items only in existing Tiers</item>
    /// <item> NewTierItemBehaviour.AllowOnlyInNewTiers - Allow new items only in new Tiers</item>
    /// <item> NewTierItemBehaviour.AllowAllAndGroup - Allow all new items an group the new items in a special tier</item>
    /// </list>
    /// </summary>
    /// <remarks>
    /// Set to "AllowAllNew" by default
    /// </remarks>
    public NewTierItemBehaviour NewItemBehaviour { get; set; } = NewTierItemBehaviour.AllowAllNew;
    public IEnumerable<string> ItemAttributeWhitelist { get; set; } = ItemAttributeWhilelistBase;

    public static List<string> ItemAttributeWhilelistBase 
    {
        get
        {
            return new List<string>() { "Image", "Tags", "Notes" };
        }
    }

    // idk if I should do one just for the categories, if I do I think I might as well make every single field customizable
}

public static class MergeBehaviourTable
{
    //public static MergeBehaviour MergeIntoCurrentShallow => MB_MergeIntoCurrentShallow.Instance;
    /// <summary>
    /// Prioritizes the current data (keeping as is).
    /// </summary>
    public static MergeBehaviour None => MB_Current.Instance;
    /// <summary>
    /// Prioritizes the incoming data.
    /// </summary>
    public static MergeBehaviour Incoming => MB_Incoming.Instance;
    //public static MergeBehaviour SmartMergeShallow => MB_SmartMergeShallow.Instance;
    /// <summary>
    /// Prioritizes the elements changed last.
    /// </summary>
    public static MergeBehaviour Smart => MB_Smart.Instance;
}

internal class MB_Current : MergeBehaviour
{
    public static readonly MB_Current Instance = new();

    public override TierList Compare(TierList current, TierList incoming)
    {
        return current;
    }

    public override Tier Compare(Tier current, Tier incoming)
    {
        return current;
    }

    public override TierItem Compare(TierItem current, TierItem incoming)
    {
        return current;
    }
}

internal class MB_Incoming : MergeBehaviour
{
    public static readonly MB_Incoming Instance = new();

    public override TierList Compare(TierList current, TierList incoming)
    {
        return incoming;
    }

    public override Tier Compare(Tier current, Tier incoming)
    {
        return incoming;
    }

    public override TierItem Compare(TierItem current, TierItem incoming)
    {
        return incoming;
    }
}

internal class MB_Smart : MergeBehaviour
{
    public static readonly MB_Smart Instance = new();

    public override TierList Compare(TierList current, TierList incoming)
    {
        if (current.lastModified >= incoming.lastModified)
            return current;
        return incoming;
    }

    public override Tier Compare(Tier current, Tier incoming)
    {
        if (current.lastModified >= incoming.lastModified)
            return current;
        return incoming;
    }

    public override TierItem Compare(TierItem current, TierItem incoming)
    {
        if (current.lastModified >= incoming.lastModified)
            return current;
        return incoming;
    }
}
