using System.Collections.Generic;
using Oxide.Core;

namespace Oxide.Plugins
{
    [Info("Hemp Troll", "Bazz3l", "1.0.2")]
    [Description("Gives a random item to players when picking up hemp bushes.")]
    class HempTroll : RustPlugin
    {
        #region Fields
        PluginConfig _config;
        StoredData _data;
        #endregion

        #region Storage
        class StoredData
        {
            public List<ulong> Players = new List<ulong>();
        }

        void SaveData()
        {
            Interface.Oxide.DataFileSystem.WriteObject(Name, _data);
        }
        #endregion

        #region Config
        PluginConfig GetDefaultConfig()
        {
            return new PluginConfig
            {
                Items = new List<TrollItem> {
                    new TrollItem("rifle.ak", 1),
                    new TrollItem("hat.wolf", 1),
                    new TrollItem("coffeecan.helmet", 1),
                    new TrollItem("metal.facemask", 1),
                    new TrollItem("metal.plate.torso", 1),
                    new TrollItem("electric.teslacoil", 1)
                }
            };
        }

        class PluginConfig
        {
            public List<TrollItem> Items;
        }

        class TrollItem
        {
            public string Shortname;
            public int Amount;

            public TrollItem(string shortname, int amount)
            {
                Shortname = shortname;
                Amount = amount;
            }
        }
        #endregion

        #region Oxide
        protected override void LoadDefaultConfig() => Config.WriteObject(GetDefaultConfig(), true);

        protected override void LoadDefaultMessages()
        {
            lang.RegisterMessages(new Dictionary<string, string> {
                {"GiveItem", "<color=#DC143C>Hemp Troll</color>: You picked up {0}x {1}."}
            }, this);
        }

        void Init()
        {
            _config = Config.ReadObject<PluginConfig>();
            _data = Interface.Oxide.DataFileSystem.ReadObject<StoredData>(Name);
        }

        void OnCollectiblePickup(Item item, BasePlayer player, CollectibleEntity entity)
        {
            if (player == null || player.inventory == null || !entity.PrefabName.Contains("hemp-collectable"))
            {
                return;
            }

            if (HasClaimed(player) || IsFull(player.inventory))
            {
                return;
            }

            GiveItem(player);
        }
        #endregion

        #region Core
        void GiveItem(BasePlayer player)
        {
            TrollItem trollItem = _config.Items.GetRandom();
            if (trollItem == null)
            {
                return;
            }

            Item item = ItemManager.CreateByName(trollItem.Shortname, trollItem.Amount);
            if (item == null)
            {
                return;
            }

            player.GiveItem(item);

            _data.Players.Add(player.userID);

            SaveData();
            
            player.ChatMessage(Lang("GiveItem", player.UserIDString, item.amount, item.info.displayName.translated));
        }

        bool HasClaimed(BasePlayer player)
        {
            return _data.Players.Contains(player.userID);
        }

        bool IsFull(PlayerInventory inventory)
        {
            return inventory.containerBelt.IsFull() && inventory.containerMain.IsFull();
        }
        #endregion

        #region Helpers
        string Lang(string key, string id = null, params object[] args) => string.Format(lang.GetMessage(key, this, id), args);
        #endregion
    }
}