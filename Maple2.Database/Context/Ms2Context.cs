using Maple2.Database.Model;
using Maple2.Database.Model.Shop;
using Microsoft.EntityFrameworkCore;

namespace Maple2.Database.Context;

public sealed class Ms2Context(DbContextOptions options) : DbContext(options) {
    internal DbSet<Account> Account { get; set; } = null!;
    internal DbSet<Character> Character { get; set; } = null!;
    internal DbSet<CharacterConfig> CharacterConfig { get; set; } = null!;
    internal DbSet<CharacterUnlock> CharacterUnlock { get; set; } = null!;
    internal DbSet<Guild> Guild { get; set; } = null!;
    internal DbSet<GuildMember> GuildMember { get; set; } = null!;
    internal DbSet<GuildApplication> GuildApplication { get; set; } = null!;
    internal DbSet<Home> Home { get; set; } = null!;
    internal DbSet<Item> Item { get; set; } = null!;
    internal DbSet<PetConfig> PetConfig { get; set; } = null!;
    internal DbSet<ItemStorage> ItemStorage { get; set; } = null!;
    internal DbSet<Club> Club { get; set; } = null!;
    internal DbSet<ClubMember> ClubMember { get; set; } = null!;
    internal DbSet<SkillTab> SkillTab { get; set; } = null!;
    internal DbSet<Buddy> Buddy { get; set; } = null!;
    internal DbSet<UgcMap> UgcMap { get; set; } = null!;
    internal DbSet<UgcMapCube> UgcMapCube { get; set; } = null!;
    internal DbSet<UgcResource> UgcResource { get; set; } = null!;
    internal DbSet<Mail> Mail { get; set; } = null!;
    internal DbSet<MesoListing> MesoMarket { get; set; } = null!;
    internal DbSet<SoldMesoListing> MesoMarketSold { get; set; } = null!;
    internal DbSet<CharacterShopData> CharacterShopData { get; set; } = null!;
    internal DbSet<CharacterShopItemData> CharacterShopItemData { get; set; } = null!;
    internal DbSet<GameEventUserValue> GameEventUserValue { get; set; } = null!;
    internal DbSet<SystemBanner> SystemBanner { get; set; } = null!;
    internal DbSet<UgcMarketItem> UgcMarketItem { get; set; } = null!;
    internal DbSet<SoldUgcMarketItem> SoldUgcMarketItem { get; set; } = null!;
    internal DbSet<SoldMeretMarketItem> SoldMeretMarketItem { get; set; } = null!;
    internal DbSet<BlackMarketListing> BlackMarketListing { get; set; } = null!;
    internal DbSet<Achievement> Achievement { get; set; } = null!;
    internal DbSet<Quest> Quest { get; set; } = null!;
    internal DbSet<ServerInfo> ServerInfo { get; set; } = null!;
    internal DbSet<Medal> Medal { get; set; } = null!;
    internal DbSet<BannerSlot> BannerSlots { get; set; } = null!;
    internal DbSet<HomeLayout> HomeLayout { get; set; } = null!;
    internal DbSet<HomeLayoutCube> UgcCubeLayout { get; set; } = null!;
    internal DbSet<Marriage> Marriage { get; set; } = null!;
    internal DbSet<WeddingHall> WeddingHall { get; set; } = null!;
    internal DbSet<Nurturing> Nurturing { get; set; } = null!;
    internal DbSet<DungeonRecord> DungeonRecord { get; set; } = null!;
    internal DbSet<PlayerReport> PlayerReports { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder) {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<Account>(Maple2.Database.Model.Account.Configure);
        modelBuilder.Entity<Character>(Maple2.Database.Model.Character.Configure);
        modelBuilder.Entity<CharacterConfig>(Maple2.Database.Model.CharacterConfig.Configure);
        modelBuilder.Entity<CharacterUnlock>(Maple2.Database.Model.CharacterUnlock.Configure);
        modelBuilder.Entity<Guild>(Maple2.Database.Model.Guild.Configure);
        modelBuilder.Entity<GuildMember>(Maple2.Database.Model.GuildMember.Configure);
        modelBuilder.Entity<GuildApplication>(Maple2.Database.Model.GuildApplication.Configure);
        modelBuilder.Entity<Home>(Maple2.Database.Model.Home.Configure);
        modelBuilder.Entity<Item>(Maple2.Database.Model.Item.Configure);
        modelBuilder.Entity<PetConfig>(Maple2.Database.Model.PetConfig.Configure);
        modelBuilder.Entity<ItemStorage>(Maple2.Database.Model.ItemStorage.Configure);
        modelBuilder.Entity<Club>(Maple2.Database.Model.Club.Configure);
        modelBuilder.Entity<ClubMember>(Maple2.Database.Model.ClubMember.Configure);
        modelBuilder.Entity<SkillTab>(Maple2.Database.Model.SkillTab.Configure);
        modelBuilder.Entity<Buddy>(Maple2.Database.Model.Buddy.Configure);
        modelBuilder.Entity<UgcMap>(Maple2.Database.Model.UgcMap.Configure);
        modelBuilder.Entity<UgcMapCube>(Maple2.Database.Model.UgcMapCube.Configure);
        modelBuilder.Entity<UgcResource>(Maple2.Database.Model.UgcResource.Configure);
        modelBuilder.Entity<Mail>(Maple2.Database.Model.Mail.Configure);
        modelBuilder.Entity<SystemBanner>(Maple2.Database.Model.SystemBanner.Configure);
        modelBuilder.Entity<UgcMarketItem>(Maple2.Database.Model.UgcMarketItem.Configure);
        modelBuilder.Entity<SoldUgcMarketItem>(Maple2.Database.Model.SoldUgcMarketItem.Configure);
        modelBuilder.Entity<Achievement>(Maple2.Database.Model.Achievement.Configure);
        modelBuilder.Entity<Quest>(Maple2.Database.Model.Quest.Configure);
        modelBuilder.Entity<Medal>(Maple2.Database.Model.Medal.Configure);
        modelBuilder.Entity<BannerSlot>(BannerSlot.Configure);
        modelBuilder.Entity<HomeLayout>(Maple2.Database.Model.HomeLayout.Configure);
        modelBuilder.Entity<HomeLayoutCube>(HomeLayoutCube.Configure);
        modelBuilder.Entity<Marriage>(Maple2.Database.Model.Marriage.Configure);
        modelBuilder.Entity<WeddingHall>(Maple2.Database.Model.WeddingHall.Configure);
        modelBuilder.Entity<Nurturing>(Maple2.Database.Model.Nurturing.Configure);

        modelBuilder.Entity<MesoListing>(MesoListing.Configure);
        modelBuilder.Entity<SoldMesoListing>(SoldMesoListing.Configure);
        modelBuilder.Entity<SoldMeretMarketItem>(Maple2.Database.Model.SoldMeretMarketItem.Configure);
        modelBuilder.Entity<CharacterShopData>(Maple2.Database.Model.Shop.CharacterShopData.Configure);
        modelBuilder.Entity<CharacterShopItemData>(Maple2.Database.Model.Shop.CharacterShopItemData.Configure);
        modelBuilder.Entity<BlackMarketListing>(Maple2.Database.Model.BlackMarketListing.Configure);

        modelBuilder.Entity<GameEventUserValue>(Maple2.Database.Model.GameEventUserValue.Configure);

        modelBuilder.Entity<ServerInfo>(Maple2.Database.Model.ServerInfo.Configure);
        modelBuilder.Entity<PlayerReport>(PlayerReport.Configure);

        modelBuilder.Entity<DungeonRecord>(Maple2.Database.Model.DungeonRecord.Configure);
    }
}
