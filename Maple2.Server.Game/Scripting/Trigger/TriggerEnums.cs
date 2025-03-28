namespace Maple2.Server.Game.Scripting.Trigger;

// ReSharper disable InconsistentNaming
public enum Align { center = 0, left = 1, right = 2, bottomLeft = 3, bottomRight = 4, topCenter = 5, centerLeft = 6, centerRight = 7 }
// ReSharper restore All

public enum FieldGame { Unknown, HideAndSeek, GuildVsGame, MapleSurvival, MapleSurvivalTeam, WaterGunBattle }

// ReSharper disable InconsistentNaming
public enum Locale { ALL, KR, CN, NA, JP, TH, TW }
// ReSharper restore All

public enum Weather { Clear = 0, Snow = 1, HeavySnow = 2, Rain = 3, HeavyRain = 4, SandStorm = 5, CherryBlossom = 6, LeafFall = 7 }

public enum BannerType : byte { Lose = 0, GameOver = 1, Winner = 2, Bonus = 3, Draw = 4, Success = 5, Text = 6, Fail = 7, Countdown = 8, }

public enum SideNpcTalkType : byte { Default = 0, Movie = 1, CutIn = 2, TalkBottom = 3, Invasion = 4, Wedding = 5 }
