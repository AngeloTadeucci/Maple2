﻿using System.Collections.Generic;
using Maple2.Database.Model;
using Microsoft.EntityFrameworkCore;

namespace Maple2.Database.Context;

public sealed class InitializationContext : DbContext {
    internal DbSet<Account> Account { get; set; }
    internal DbSet<Character> Character { get; set; }
    internal DbSet<Item> Item { get; set; }
    internal DbSet<Club> Club { get; set; }
    internal DbSet<ClubMember> ClubMember { get; set; }

    public InitializationContext(DbContextOptions options) : base(options) {
        ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder) {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<Account>(Maple2.Database.Model.Account.Configure);
        modelBuilder.Entity<Character>(Maple2.Database.Model.Character.Configure);
        modelBuilder.Entity<Item>(Maple2.Database.Model.Item.Configure);
        modelBuilder.Entity<Club>(Maple2.Database.Model.Club.Configure);
        modelBuilder.Entity<ClubMember>(Maple2.Database.Model.ClubMember.Configure);
    }

    public bool Initialize() {
        bool created = Database.EnsureCreated();
        if (!created) {
            return false;
        }

        Database.ExecuteSqlRaw("ALTER TABLE account AUTO_INCREMENT = 10000000000");
        Database.ExecuteSqlRaw("ALTER TABLE `character` AUTO_INCREMENT = 20000000000");
        Database.ExecuteSqlRaw("ALTER TABLE club AUTO_INCREMENT = 30000000000");
        
        // item is the entity that could grow the most, so put it last
        Database.ExecuteSqlRaw("ALTER TABLE item AUTO_INCREMENT = 1000000000000");

        TestTables();

        return true;
    }

    public void TestTables() {
        var account = new Account();
        Account.Add(account);
        SaveChanges();
        
        var character = new Character();
        character.AccountId = account.Id;
        character.Name = "First";
        Character.Add(character);
        var character2 = new Character();
        character2.AccountId = account.Id;
        character2.Name = "Second";
        Character.Add(character2);
        SaveChanges();

        var club = new Club();
        club.Name = "Club";
        club.LeaderId = character.Id;
        club.Members = new List<ClubMember>();
        club.Members.Add(new ClubMember {
            CharacterId = character.Id,
        });
        club.Members.Add(new ClubMember {
            CharacterId = character2.Id,
        });
        Club.Add(club);
        SaveChanges();
    }
}