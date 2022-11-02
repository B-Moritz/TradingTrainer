using Webapplikasjoner_oblig.Model;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Diagnostics;

namespace Webapplikasjoner_oblig.DAL
{
    public class SearchResultRepositry : ISearchResultRepositry
    {
        private readonly TradingContext _db;

        public SearchResultRepositry(TradingContext db)
        {
            _db = db;
        }

    
        /**
         * A method to retreiv all serchResults from database
         */
        public async Task<List<SearchResult>?> GetAllKeywordsAsync()
        {   
            //list of all search results
            List<SearchResults> keywords = await _db.SearchResults.ToListAsync();

            if (!(keywords is null))
            {
                //list to hold search result objects
                var result = new List<SearchResult>();

                //looping over type retived from database
                //mapping searchResults element to searchResult object
                foreach (var keyword in keywords)
                {
                    //list to replace the list of stock retrived from searchResults table with stockDetails objects
                    var stockDList = new List<StockSearchResult>();

                    //search result object to be used when mapping from searchresults table
                    var searchRusltObject = new SearchResult();

                    searchRusltObject.SearchKeyword = keyword.SearchKeyword;
                    searchRusltObject.SearchTime = keyword.SearchTimestamp;
                   

                    //looping over the list in keywords 
                    foreach (var stock in keyword.Stocks)
                    {
                        //mapping stock to stockDetails
                        var stockDetail = new StockSearchResult()
                        {
                            Symbol = stock.Symbol,
                            StockName = stock.StockName,
                            Type = stock.Type,
                            StockCurrency = stock.Currency,
                            LastUpdated = stock.LastUpdated,
                        };
                       
                        stockDList.Add(stockDetail);
                    }
                    searchRusltObject.StockList = stockDList;
                    result.Add(searchRusltObject);
                }
                return result;
            }
            return null;
         }


        public async Task<SearchResult?> GetOneKeywordAsync(string keyWord)
        {
            if (keyWord == "")
            {
                throw new ArgumentException();
            }            

            // get a search result that has primary key as keyword
            SearchResults searchResult = await _db.SearchResults.FindAsync(keyWord);
            var stockDList = new List<StockSearchResult>();

            if (searchResult is null)
            {
                return null;
            }

            //go over the list of stock the searchreasult holds
            foreach (var searchRes in searchResult.Stocks)
            {

                //make each stock in to stockdetail object with ony neccessary properties extracted
                var stockDetail = new StockSearchResult()
                {
                    Symbol = searchRes.Symbol,
                    StockName = searchRes.StockName,
                };

                //add stockDetail in to a list
                stockDList.Add(stockDetail);

            };

            //from searchResults to searchResult object
            var dbSearchResult = new SearchResult()
            {
                SearchKeyword = searchResult.SearchKeyword,
                SearchTime = searchResult.SearchTimestamp,
                //list containing searchResult objects
                StockList = stockDList

            };

            return dbSearchResult;
        }

       /**
       * This method saves SearchResult object by mapping to SearchResults table
       */
       public async Task SaveSearchResultAsync(SearchResult result)
       {
           // Converting StockSearchResult to Stocks objects if it does not already exist in the database
           var stocksList = new List<Stocks>();

           foreach (var stock in result.StockList)
           {
               // Find the first stock in the database that has the same stock symbol
               Stocks? curDbStock = await _db.Stocks.FindAsync(stock.Symbol);

               if (curDbStock is null)
               {
                   curDbStock = new Stocks();
                   curDbStock.StockName = stock.StockName;
                   curDbStock.Symbol = stock.Symbol;
                   curDbStock.LastUpdated = stock.LastUpdated;
                   curDbStock.Type = stock.Type;
                   curDbStock.Currency = stock.StockCurrency;
               } 
               else if ((DateTime.Now - curDbStock.LastUpdated).TotalHours >= 24) 
               {
                   if (curDbStock.StockQuotes is not null) {
                       _db.StockQuotes.RemoveRange(curDbStock.StockQuotes);
                   }
                   _db.Stocks.Remove(curDbStock);
                   // If the existing stock is old, add the new stocck object
                   curDbStock = new Stocks();
                   curDbStock.StockName = stock.StockName;
                   curDbStock.Symbol = stock.Symbol;
                   curDbStock.LastUpdated = stock.LastUpdated;
                   curDbStock.Type = stock.Type;
                   curDbStock.Currency = stock.StockCurrency;
               }
               stocksList.Add(curDbStock);
           }

           var dbSearchResult = new SearchResults();
           dbSearchResult.SearchKeyword = result.SearchKeyword;
           dbSearchResult.SearchTimestamp = DateTime.Now;
           dbSearchResult.Stocks = stocksList;

           await _db.SearchResults.AddAsync(dbSearchResult);
           await _db.SaveChangesAsync();
       }

    /**
    * This method finds matching searchResult using keyword
    */
    public async Task<bool> FindMatchAsync(string? word)
    {
        try
        {
            var match = await _db.SearchResults.FindAsync(word);

            if(match != null)
            {
                return true;
            }
            return false;
        }
        catch(Exception e)
        {    
            Debug.WriteLine(e.ToString());
            return false;

        }
    } 

    public void DeleteSearchResult(string keyword)
    {
        // This method removes all searchResults of the specified keyword. There should only be one search result stored for each stock
        var remove = _db.SearchResults.Where<SearchResults>(t => t.SearchKeyword == keyword);
        _db.SearchResults.RemoveRange(remove);
        _db.SaveChanges();
      }

    }
}
