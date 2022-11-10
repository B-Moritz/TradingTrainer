// Webapplikasjoner oblig 1     OsloMet     31.10.2022

// This file contains code defining database model used by the application

using System.Reflection;
using Microsoft.EntityFrameworkCore;
using TradingTrainer.Model;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AlphaVantageInterface.Models;
using Microsoft.EntityFrameworkCore.Update;

namespace TradingTrainer.DAL
{


    public class TradingContext : DbContext
    {
        protected readonly IConfiguration _configuration;
        protected readonly IHostEnvironment _environment;


        public TradingContext(IConfiguration configuration,
                              DbContextOptions<TradingContext> options,
                              IHostEnvironment env) : base(options)
        {
            _configuration = configuration;
            _environment = env;
        }

        //Migrations microsoft tutorial 
        //https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/?tabs=dotnet-core-cli

        //.net core web api microsoft tutorial
        //https://learn.microsoft.com/en-us/aspnet/core/tutorials/first-web-api?view=aspnetcore-6.0&tabs=visual-studio
        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            // connect to sqlite database
            options.UseSqlite(_configuration.GetConnectionString("WebApiDatabase"))
                   .UseLazyLoadingProxies();
        }

        public DbSet<Stocks> Stocks { get; set; }
        public DbSet<Users> Users { get; set; }
        public DbSet<Trades> Trades { get; set; }
        public DbSet<SearchResults> SearchResults { get; set; }

        //
        //public DbSet<Portfolio> Portfolio { get; set; }


        // Custom join table
        public DbSet<StockOwnerships> StockOwnerships { get; set; }

        public DbSet<StockQuotes> StockQuotes { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            if (_environment.IsDevelopment())
            {
                // Definition of composite primary keys
                // Documentation used: https://learn.microsoft.com/en-us/ef/core/modeling/keys?tabs=data-annotations
                modelBuilder.Entity<StockOwnerships>().HasKey(c => new { c.UsersId, c.StocksId });

                modelBuilder.Entity<StockQuotes>().HasKey(c => new { c.StocksId, c.Timestamp });


                // Seed database if the app is running in development mode
                // Resource used: https://code-maze.com/migrations-and-seed-data-efcore/

                // Defining users:
                var user1 = new Users
                {
                    UsersId = 1,
                    FirstName = "Dev",
                    LastName = "User",
                    Email = "DevUser@test.com",
                    Password = Convert.FromBase64String("FBqAM9fp5mfCjyAjW0ukPtSv7YTIm0lwg02ulO8pKaw="),
                    Salt = Convert.FromBase64String("x2FRQXYkGrIZ0vqUeY103YG2Nnswwp0h"),
                    AlphaVantageApiKey = _configuration["AlphaVantageApi:ApiKey"],
                    FundsAvailable = 1000000M,
                    FundsSpent = 0,
                    PortfolioCurrency = "NOK"
                };

                // Configure favoriteLists table
                // https://learn.microsoft.com/en-us/ef/core/modeling/relationships?tabs=fluent-api%2Cfluent-api-simple-key%2Csimple-key#join-entity-type-configuration
                // We  specify a many to many relationship between Users
                // and Stocks and define the FavoriteLists table as join table
                modelBuilder.Entity<Users>()
                        .HasMany(favStocks => favStocks.Favorites)
                        .WithMany(u => u.FavoriteUsers)
                        .UsingEntity(j => j.ToTable("FavoriteLists"));

                // Configuration of the StockOccurances join table used to store the many to many relationship between 
                // Search results and stocks
                modelBuilder.Entity<SearchResults>()
                        .HasMany(d => d.Stocks)
                        .WithMany(d => d.SearchResults)
                        .UsingEntity(t => t.ToTable("StockOccurances"));

                modelBuilder.Entity<Users>().HasData(user1);
            }
            else
            {
                // Definition of composite primary keys
                // Documentation used: https://learn.microsoft.com/en-us/ef/core/modeling/keys?tabs=data-annotations
                modelBuilder.Entity<StockOwnerships>().HasKey(c => new { c.UsersId, c.StocksId });
                modelBuilder.Entity<StockQuotes>().HasKey(c => new { c.StocksId, c.Timestamp });

                // Configure favoriteLists table
                // https://learn.microsoft.com/en-us/ef/core/modeling/relationships?tabs=fluent-api%2Cfluent-api-simple-key%2Csimple-key#join-entity-type-configuration
                // We  specify a many to many relationship between Users 
                // and Stocks and define the FavoriteLists table as join table
                modelBuilder.Entity<Users>()
                        .HasMany(favStocks => favStocks.Favorites)
                        .WithMany(u => u.FavoriteUsers)
                        .UsingEntity(j => j.ToTable("FavoriteLists"));

                // Configuration of the StockOccurances join table used to store the many to many relationship between 
                // Search results and stocks
                modelBuilder.Entity<SearchResults>()
                        .HasMany(d => d.Stocks)
                        .WithMany(d => d.SearchResults)
                        .UsingEntity(t => t.ToTable("StockOccurances"));
            }
        }
    }
    public class Stocks
    {
        [Key]
        public string Symbol { get; set; }
        public string StockName { get; set; }
        public string Type { get; set; }
        public DateTime LastUpdated { get; set; }

        public string Currency { get; set; }

        // Navigation properties:
        // List of users that have stock in their favorite list
        virtual public List<Users> FavoriteUsers { get; set; }
        // list of Trades about this stock
        virtual public List<Trades> TradeOccurances { get; set; }
        // List of all stored stockQuotes for the stock (idealy only one record)
        virtual public List<StockQuotes> StockQuotes { get; set; }
        // List of all searchresults that this stock is part of
        virtual public List<SearchResults> SearchResults { get; set; }
        virtual public List<StockOwnerships> Owners { get; set; }
    }

    public class Users
    {
        // Infered primary key because the property UsersId follows convention <Type name>Id
        public int UsersId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public byte[] Password { get; set; }
        public byte[] Salt { get; set; }
        public string AlphaVantageApiKey { get; set; }
        public decimal FundsAvailable { get; set; }
        public decimal FundsSpent { get; set; }

        public string PortfolioCurrency { get; set; }   
        // Navigation properties

        // List containing favorite stocks
        virtual public List<Stocks> Favorites { get; set; }
        // List containing all trades associated to the user
        virtual public List<Trades> Trades { get; set; }

        virtual public List<StockOwnerships> Portfolio { get; set; }
    }

    public class Trades
    {
        // Infered primary key
        public int TradesId { get; set; }
        // The amount of shares of the selected stock that is going to be traded
        public int StockCount { get; set; }
        // The timestamp of the trade
        public DateTime TradeTime { get; set; }
        // Property is true if stocks are bought by user, false if the user is selling
        public bool UserIsBying { get; set; }
        // Money saldo received or payed (sign is determined by UserIsBying property)
        public decimal Saldo { get; set; }

        public string Currency { get; set; }

        // Expllicit foreign keys
        public string StocksId { get; set; }
        public int UsersId { get; set; }

        // Navigation properties

        // Refference to stock that is associated to trade 
        virtual public Stocks? Stock { get; set; }
        // Refference to the user which is performing the trade
        virtual public Users? User { get; set; }
    }

    public class StockOwnerships
    {
        // Important: Note how the primary keys UserId and StocksId follows a convention. 
        // EF detects the keys as foreign keys for User and Stock navigation properties
        // Those keys are also added as primary key in the OnModelCreating method.
        // The result is a custom join table with UsersId and StocksId as primary keys and a 
        // property StockCounter.
        public int UsersId { get; set; }
        public string? StocksId { get; set; }
        // Number of shares owned by the user of the stock refferenced
        public int StockCounter { get; set; }
        public decimal SpentValue { get; set; }

        // Navigation Properties

        // User that owns the stock shares
        virtual public Users? User { get; set; }
        // Stock that is owned by the user refferenced
        virtual public Stocks? Stock { get; set; }
    }

    public class StockQuotes
    {
        [Key] // Attribute sets StocksId as primary key of the table
        public string StocksId { get; set; }
        // Navigation property to the stock that this quote is for
        virtual public Stocks Stock { get; set; }
        public DateTime Timestamp { get; set; }
        public decimal Open { get; set; }
        public decimal High { get; set; }
        public decimal Low { get; set; }
        public decimal Price { get; set; }
        public int Volume { get; set; }
        public DateTime LatestTradingDay { get; set; }
        public decimal PreviousClose { get; set; }
        public decimal Change { get; set; }
        public string ChangePercent { get; set; }
    }

    public class SearchResults
    {
        [Key] // Used to specify that SearchKeyword is primary key.
        // The kyword used to search with the Alpha Vantage api
        public string SearchKeyword { get; set; }
        // Timestamp for when the search executed
        public DateTime SearchTimestamp { get; set; }

        // Navigation properties

        // List of stocks in the search result
        virtual public List<Stocks> Stocks { get; set; }
    }

}
