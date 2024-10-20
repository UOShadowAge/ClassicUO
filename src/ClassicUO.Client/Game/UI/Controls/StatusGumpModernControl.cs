using ClassicUO.Game.UI.Controls;
using ClassicUO.Game.GameObjects;
using ClassicUO.Renderer;
using ClassicUO.Assets;
using ClassicUO.Utility;
using Microsoft.Xna.Framework;
using System;
using ClassicUO.Configuration;
using ClassicUO.Game.Data;
using ClassicUO.Game.Managers;
using ClassicUO.Game.UI.Gumps;
using ClassicUO.Input;
using ClassicUO.Resources;

namespace ClassicUO.Game.UI
{
    internal abstract class StatusGumpBaseControl : Gump
    {
        public PlayerMobile Player;

        protected StatusGumpBaseControl(PlayerMobile pm = null) : base(0, 0)
        {
            Player = pm != null ? pm : World.Player;

            CanCloseWithRightClick = false;
            CanMove = false;
        }

        protected Label[] _labels;
        protected Point _point;
        protected long _refreshTime;
    }
    
    internal class StatusGumpModernControl : StatusGumpBaseControl
    {
        private readonly GumpPicWithWidth[] _fillBars = new GumpPicWithWidth[3];
        private Label[] _labels;
        private PlayerMobile _player;

        public PlayerMobile Player
        {
            get => _player;
            set
            {
                _player = value;
            }
        }

        public StatusGumpModernControl(PlayerMobile player)
        {
            // Ensure _labels array is initialized before anything else
            _labels = new Label[(int)MobileStats.NumStats];
            
            Player = player ?? World.Player;
            
            IsVisible = true;
            IsEnabled = true;

            InitializeControls();
        }

        private void InitializeControls()
        {
            // Background
            Add(new GumpPic(0, 0, 0x2A6C, 0));

            // Health, Mana, Stamina bars
            Add(new GumpPic(70, 31, 0x0805, 0));
            Add(new GumpPic(70, 45, 0x0805, 0));
            Add(new GumpPic(70, 59, 0x0805, 0));
            
            ushort gumpIdHp = 0x0806;
            
            if (Player.IsPoisoned)
            {
                gumpIdHp = 0x0808;
            }
            else if (Player.IsYellowHits)
            {
                gumpIdHp = 0x0809;
            }
            
            _fillBars[(int)FillStats.Hits] = new GumpPicWithWidth(70, 31, gumpIdHp, 0, 0);
            _fillBars[(int)FillStats.Mana] = new GumpPicWithWidth(70, 45, 0x0806, 0, 0);
            _fillBars[(int)FillStats.Stam] = new GumpPicWithWidth(70, 59, 0x0806, 0, 0);
            
            Add(_fillBars[(int)FillStats.Hits]);
            Add(_fillBars[(int)FillStats.Mana]);
            Add(_fillBars[(int)FillStats.Stam]);
            
            UpdateStatusFillBar(FillStats.Hits, Player.Hits, Player.HitsMax);
            UpdateStatusFillBar(FillStats.Mana, Player.Mana, Player.ManaMax);
            UpdateStatusFillBar(FillStats.Stam, Player.Stamina, Player.StaminaMax);
            
            // Stat Labels
            AddStatTextLabel(Player.Strength.ToString(), MobileStats.Strength, 88, 77);
            AddStatTextLabel(Player.Dexterity.ToString(), MobileStats.Dexterity, 88, 105);
            AddStatTextLabel(Player.Intelligence.ToString(), MobileStats.Intelligence, 88, 133);
            AddStatTextLabel(Player.Hits.ToString(), MobileStats.HealthCurrent, 141, 70);
            AddStatTextLabel(Player.HitsMax.ToString(), MobileStats.HealthMax, 141, 83);
            AddStatTextLabel(Player.Stamina.ToString(), MobileStats.StaminaCurrent, 141, 98);
            AddStatTextLabel(Player.StaminaMax.ToString(), MobileStats.StaminaMax, 141, 111);
            AddStatTextLabel(Player.Mana.ToString(), MobileStats.ManaCurrent, 141, 126);
            AddStatTextLabel(Player.ManaMax.ToString(), MobileStats.ManaMax, 141, 139);
            
            //TODO: Fill Rest
            
        }

        private void AddStatTextLabel(string text, MobileStats stat, int x, int y, int maxWidth = 0, ushort hue = 0x0386, TEXT_ALIGN_TYPE alignment = TEXT_ALIGN_TYPE.TS_LEFT)
        {
            Label label = new Label(text, false, hue, maxWidth, align: alignment, font: 1)
            {
                X = x,
                Y = y
            };

            _labels[(int)stat] = label;
            Add(label);
        }

        private void UpdateStatus()
        {
            if (Player != null && Player.Name != null)
            {
                // Update fill bars
                UpdateStatusFillBar(FillStats.Hits, Player.Hits, Player.HitsMax);
                UpdateStatusFillBar(FillStats.Mana, Player.Mana, Player.ManaMax);
                UpdateStatusFillBar(FillStats.Stam, Player.Stamina, Player.StaminaMax);
            
                // Update stat labels with latest values
                _labels[(int)MobileStats.Name].Text = Player.Name;
                _labels[(int)MobileStats.Strength].Text = Player.Strength.ToString();
                _labels[(int)MobileStats.Dexterity].Text = Player.Dexterity.ToString();
                _labels[(int)MobileStats.Intelligence].Text = Player.Intelligence.ToString();
                _labels[(int)MobileStats.HealthCurrent].Text = Player.Hits.ToString();
                _labels[(int)MobileStats.HealthMax].Text = Player.HitsMax.ToString();
                _labels[(int)MobileStats.StaminaCurrent].Text = Player.Stamina.ToString();
                _labels[(int)MobileStats.StaminaMax].Text = Player.StaminaMax.ToString();
                _labels[(int)MobileStats.ManaCurrent].Text = Player.Mana.ToString();
                _labels[(int)MobileStats.ManaMax].Text = Player.ManaMax.ToString();
                
                //TODO: Fill Rest
            }
        }

        private void UpdateStatusFillBar(FillStats id, int current, int max)
        {
            if (_fillBars[(int)id] == null) return;

            ushort gumpId = 0x0806;

            if (id == FillStats.Hits)
            {
                gumpId = Player.IsPoisoned ? (ushort)0x0808 : Player.IsYellowHits ? (ushort)0x0809 : (ushort)0x0806;
            }

            if (max > 0)
            {
                _fillBars[(int)id].Graphic = gumpId;
                _fillBars[(int)id].Percent = CalculatePercents(max, current, 109);
            }
        }

        private static int CalculatePercents(int max, int current, int maxValue)
        {
            if (max > 0)
            {
                max = current * 100 / max;

                if (max > 100)
                {
                    max = 100;
                }

                if (max > 1)
                {
                    max = maxValue * max / 100;
                }
            }

            return max;
        }

        private enum FillStats
        {
            Hits,
            Mana,
            Stam
        }

        private enum MobileStats
        {
            Name,
            Strength,
            Dexterity,
            Intelligence,
            HealthCurrent,
            HealthMax,
            StaminaCurrent,
            StaminaMax,
            ManaCurrent,
            ManaMax,
            WeightMax,
            Followers,
            WeightCurrent,
            LowerReagentCost,
            SpellDamageInc,
            FasterCasting,
            FasterCastRecovery,
            StatCap,
            HitChanceInc,
            DefenseChanceInc,
            LowerManaCost,
            DamageChanceInc,
            SwingSpeedInc,
            Luck,
            Gold,
            AR,
            RF,
            RC,
            RP,
            RE,
            Damage,
            Sex,
            HeatTimerSeconds,
            CriminalTimerSeconds,
            BandageTimerSeconds,
            NumStats,
        }
    }
}
