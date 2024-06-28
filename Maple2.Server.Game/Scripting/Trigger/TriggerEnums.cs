namespace Maple2.Server.Game.Scripting.Trigger;

[Flags]
public enum Align { Top = 0, Center = 1, Bottom = 2, Left = 4, Right = 8 }

public enum FieldGame { Unknown, HideAndSeek, GuildVsGame, MapleSurvival, MapleSurvivalTeam, WaterGunBattle }

// ReSharper disable InconsistentNaming
public enum Locale { ALL, KR, CN, NA, JP, TH, TW }
// ReSharper restore All

public enum Weather { Clear = 0, Snow = 1, HeavySnow = 2, Rain = 3, HeavyRain = 4, SandStorm = 5, CherryBlossom = 6, LeafFall = 7 }

public enum BannerType : byte { Lose = 0, GameOver = 1, Winner = 2, Bonus = 3, Draw = 4, Success = 5, Text = 6, Fail = 7, Countdown = 8, }
