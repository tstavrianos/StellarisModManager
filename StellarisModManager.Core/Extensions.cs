using System.Collections.ObjectModel;

namespace StellarisModManager.Core
{
    using System;

    using PDXModLib.ModData;

    using Stellaris.Data.Json;

    internal static class Extensions
    {
        internal static void MoveItemUp<T>(this ObservableCollection<T> baseCollection, int selectedIndex)
        {
            //# Check if move is possible
            if (selectedIndex <= 0)
                return;

            //# Move-Item
            baseCollection.Move(selectedIndex - 1, selectedIndex);
        }

        internal static void MoveItemDown<T>(this ObservableCollection<T> baseCollection, int selectedIndex)
        {
            //# Check if move is possible
            if (selectedIndex < 0 || selectedIndex + 1 >= baseCollection.Count)
                return;

            //# Move-Item
            baseCollection.Move(selectedIndex + 1, selectedIndex);
        }

        internal static void MoveItemDown<T>(this ObservableCollection<T> baseCollection, T selectedItem)
        {
            //# MoveDown based on Item
            baseCollection.MoveItemDown(baseCollection.IndexOf(selectedItem));
        }

        internal static void MoveItemUp<T>(this ObservableCollection<T> baseCollection, T selectedItem)
        {
            //# MoveUp based on Item
            baseCollection.MoveItemUp(baseCollection.IndexOf(selectedItem));
        }

        internal static bool Matches(this Mod mod, ModsRegistryEntry modsRegistryEntry)
        {
            if (!string.IsNullOrEmpty(modsRegistryEntry.GameRegistryId))
                return modsRegistryEntry.GameRegistryId.Equals(mod.Key, StringComparison.OrdinalIgnoreCase);
            if (!string.IsNullOrEmpty(modsRegistryEntry.SteamId) && !string.IsNullOrEmpty(mod.RemoteFileId))
            {
                return modsRegistryEntry.SteamId.Equals(mod.RemoteFileId, StringComparison.OrdinalIgnoreCase);
            }

            return string.Equals(modsRegistryEntry.DisplayName, mod.Name, StringComparison.OrdinalIgnoreCase);
        }

    }
}
