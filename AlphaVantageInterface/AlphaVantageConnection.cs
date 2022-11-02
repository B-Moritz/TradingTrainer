// Webapplikasjoner oblig 1     OsloMet     27.10.2022

// This file contains code used to handle the http requests sent to the Alpha Vantage API

// The http rest client created in this project is based on the alpha vantage documentation 
// and the microsoft documentation:
// https://www.alphavantage.co/documentation/
// https://learn.microsoft.com/en-us/dotnet/csharp/tutorials/console-webapiclient
// This article was also used as an inspiration:
// https://code-maze.com/introduction-system-text-json-examples/

using System.Text.Json;
using System.Text.Json.Serialization;
using AlphaVantageInterface.Models;
using System.Text.RegularExpressions;
using System.Globalization;

namespace AlphaVantageInterface {

    /***
    * The AlphaVantageConnection class contains methods for handeling the connection to the Alpha
    * Vantage API. An object of this class can be created by using the BuildAlphaVantageConnection method or by 
    * using the AlphaCantageConnection constructor.
    */
    public class AlphaVantageConnection
    {
        // The api key used by the object instance to create http requests
        private readonly string _apiKey;
        // Http client object used to handle http requests and responses
        private readonly HttpClient _cli = new HttpClient();
        // Flag that indicates if the api key is limited. 
        // If it is true, the object will use the connection status cache to ensure that limits are not exceeded. 
        private bool IsLimited;
        // The file name of the file containing the latest connection status
        private static readonly string _connectionStatusCache = "ConnectionStatusCache.json"; 
        // The base uri used to create the http request to the alpha vantage endpoints
        private readonly string _baseUri = "https://www.alphavantage.co/query?";
        // Deserialization options used to deserialize json
        private readonly JsonSerializerOptions _globalJsonOptions = new JsonSerializerOptions {
                NumberHandling = JsonNumberHandling.AllowReadingFromString
            };
        // The number of http requests that should be accepted per day
        private int CallLimitDaily;
        // The current connection status of the api key
        private ConnectionStatus LatestStatus;

        private string LimitMessage = "Thank you for using Alpha Vantage! Our standard API call frequency is 5 calls per minute and 500 calls per day. Please visit https://www.alphavantage.co/premium/ if you would like to target a higher API call frequency.";


        public AlphaVantageConnection(string apiKey, bool isLimited, int CallLimitDaily)
        {
            // Create the initial status object containing only the key and the limit flag
            ConnectionStatus initialStatus = new ConnectionStatus {
                ApiKey = apiKey,
                IsLimited = isLimited
            };
            // Specify fields used by the object
            this.LatestStatus = initialStatus;
            this._apiKey = apiKey;
            this.IsLimited = isLimited;
            this.CallLimitDaily = CallLimitDaily;
        }

        /***
        * This method instanciates a new AlphaVantageConnection object and verifies that the existing conneciton status 
        * is valid. The method is Asynchronous.
        * Parameters:
        *   (string) apiKey: The api key used to authenticate to the Alpha Vantage endpoint
        *   (bool) isLimited: Flag is set if the api key is limited
        *   (int) CallLimitDaily: The amount of http request that can be made per day.
        * Return (AlphaVantageConnection): The method returns an instance of the AlphaVantageConnection, wraped into a Task object
        */
        public static async Task<AlphaVantageConnection> BuildAlphaVantageConnectionAsync(string apiKey, bool isLimited, int CallLimitDaily)
        {
            if (CallLimitDaily < 0) {
                // Validate the CallLimitDaily argument
                throw new ArgumentException("The CallLimitDaily argument provided was negative.");
            }

            // Create an object of the AlphaVantageConnection class
            AlphaVantageConnection newConnection = new AlphaVantageConnection(apiKey, isLimited, CallLimitDaily);
            // Load the temporary stored data for the api connection and verify that calls can be made
            await newConnection.VerifyStatusCacheAsync();
            // Returns the connection object
            return newConnection;
        }

        /***
        * This method handels the http request to the search endpoint of alpha vantage. A search keyword is given 
        * as an argument and a SearchResult object is returned,containing a list of Stock objects that 
        * match the search keyword.
        * Parameters:
        *   (string) keyword: the keyword used to search for stocks
        * Return: A SearchResult object
        */
        public async Task<SearchResult> FindStockAsync(string keyword)
        {
            // The request uri is constructed
            string function = "SYMBOL_SEARCH";
            string searchUri = $"{_baseUri}function={function}&keywords={keyword}&apikey={_apiKey}";

            // Verifing that the api key can be used
            await VerifyStatusCacheAsync();
            // Making the http request
            var response = await _cli.GetAsync(searchUri, HttpCompletionOption.ResponseHeadersRead);
            // Ensure that HttpRequestException is thrown if the request was not successfull (status code is not in range 200-299).
            response.EnsureSuccessStatusCode();
            // Creates a StreamReader for the response body
            var respStream = response.Content.ReadAsStreamAsync();
            // Adding to the call counter in the status cache
            await AddCallCounterAsync();

            // Extract the response as a string
            string msg = "";
            using (var reader = new StreamReader(await respStream))
            {
                string? line;
                // Read first line
                line = await reader.ReadLineAsync();
                while (line != null)
                {
                    //Read until line is null
                    msg += line;
                    // Read next line
                    line = await reader.ReadLineAsync();
                }
            }

            // Verify that the response message is not a known information message
            Regex infoPattern = new Regex(LimitMessage);
            if (infoPattern.IsMatch(msg))
            {
                // If the response message contains the LimitMessage string, 
                // we can assume that the call limit of 5 calls per minute has been reached.
                throw new AlphaVantageApiCallNotPossible("The number of api calls to Alpha Vantage the last minute exceeded the limit of 5. Please wait for a minute and try again!");
            }
            else 
            {
                // Deserialize the response
                SearchResult? parsedResult = JsonSerializer.Deserialize<SearchResult>(msg, _globalJsonOptions);
                if (parsedResult == null || parsedResult.BestMatches == null) 
                {
                    // Throw an exception with the received message if the message is unexpected
                    throw new AlphaVantageApiCallNotPossible("Message from Alpha Vantage: " + msg);
                }
                return parsedResult;
            }
        }

        /***
        * This method obtains the latest stock quote for the stock which stock symbol was 
        * provided as argument to this method.
        * Parameters:
        *   (string) symbol: The symbol of the stock which the probram sohould obtian the quote for.
        * Return: A new StockQuote object is returned, matching the stock symbol provided as argument.
        */
        public async Task<StockQuote> GetStockQuoteAsync(string symbol) {
            // Definition of the request uri used to reach teh Quote endpoint
            string function = "GLOBAL_QUOTE";
            string requestUri = $"{this._baseUri}function={function}&symbol={symbol}&apikey={this._apiKey}";

            // Verifing that the api key can be used
            await VerifyStatusCacheAsync();
            // Making the http request
            var response = await _cli.GetAsync(requestUri, HttpCompletionOption.ResponseHeadersRead);
            // Ensure that HttpRequestException is thrown if the request was not successfull.
            response.EnsureSuccessStatusCode();
            // Creates a StreamReader for the response body
            var respStream = response.Content.ReadAsStreamAsync();
            // Adding to the call counter in the status cache
            await AddCallCounterAsync();
           
            // Extract the response as a string
            string msg = "";
            using (var reader = new StreamReader(await respStream))
            {
                string? line;
                // Read the first string
                line = await reader.ReadLineAsync();
                while (line != null)
                {
                    // read while line is not null
                    msg += line;
                    // Read next line
                    line = await reader.ReadLineAsync();
                }
            }
            // Verify that the response message is not a known information message
            Regex infoPattern = new Regex(LimitMessage);
            if (infoPattern.IsMatch(msg))
            {
                // If the response message contains the LimitMessage string, 
                // we can assume that the call limit of 5 calls per minute has been reached.
                throw new AlphaVantageApiCallNotPossible("The number of api calls to Alpha Vantage the last minute exceeded the limit of 5. Please wait for a minute and try again!");
            }
            else 
            {
                // Deserialize the response
                StockQuoteTemp? quoteTemp = JsonSerializer.Deserialize<StockQuoteTemp>(msg, _globalJsonOptions);

                if (quoteTemp == null || quoteTemp.GlobalQuote?.Count == 0) 
                {
                    // Throw an exception with the received message if the message is unexpected
                    throw new AlphaVantageApiCallNotPossible("Message from Alpha Vantage: " + msg);

                }

                // The received data is restructured into the StockQuote object 
                // Note how the string are parsed to decimal with CultureInfo.InvaiantCulture. The "." separator is interpreted as decimal separator.
                // The "," separator as thousend separator: "123,456,123.1234" -> 123456123.1234 
                // Documentation used: https://docs.microsoft.com/en-us/dotnet/api/system.text.json.jsonelement?view=net-6.0
                StockQuote newRefinedQuote = new StockQuote {
                    Symbol = quoteTemp.GlobalQuote?["01. symbol"].GetString(),
                    Open = decimal.Parse(quoteTemp.GlobalQuote?["02. open"].GetString(), CultureInfo.InvariantCulture),
                    High = decimal.Parse(quoteTemp.GlobalQuote?["03. high"].GetString(), CultureInfo.InvariantCulture),
                    Low = decimal.Parse(quoteTemp.GlobalQuote?["04. low"].GetString(), CultureInfo.InvariantCulture),
                    Price = decimal.Parse(quoteTemp.GlobalQuote?["05. price"].GetString(), CultureInfo.InvariantCulture),
                    Volume = int.Parse(quoteTemp.GlobalQuote?["06. volume"].GetString()),
                    LatestTradingDay = quoteTemp.GlobalQuote?["07. latest trading day"].GetString(),
                    PreviousClose = decimal.Parse(quoteTemp.GlobalQuote?["08. previous close"].GetString(), CultureInfo.InvariantCulture),
                    Change = decimal.Parse(quoteTemp.GlobalQuote?["09. change"].GetString(), CultureInfo.InvariantCulture),
                    ChangePercent = quoteTemp.GlobalQuote?["10. change percent"].GetString()
                };
                return newRefinedQuote;
            }
        }

        /***
        * This method obtains the stock quote of a stock. The method takes one AlphaVantageInterface.Model.Stock object as argument.
        * The stock symbol of this object is used to obtain the correct quote. The stock object is updatead with the obtianed quote.
        * The method then returns the reference to the stock provided as argument.
        * Parameters:
        *   (Stock) stock: The Stock object that the stock quote should be added to.
        * Return: Reference to the uppdated Stock object
        */
        public async Task GetStockQuoteAsync(Stock stock) {
            if (stock.Symbol is null) {
                // Verify that a stock symbol was provided-
                // If the stock object has no symbol, no api call can be made.
                throw new NullReferenceException("Stock symbol is null.");
            }
            // Get the stock quote
            StockQuote curQuote = await GetStockQuoteAsync(stock.Symbol);
            // reference the StockQuote object in the Stock object
            stock.StockQuoteObject = curQuote;
        }

        /***
        * This method reads the Status Cache file and checks is the call limit per day is within the specified limit
        */
        private async Task VerifyStatusCacheAsync()
        {
            // Get the current status from file
            ConnectionStatus curStatus = await ReadStatusFileAsync();
            // Make sure that call counter is from today
            CleanStatusCache(curStatus);

            if (this.IsLimited && curStatus.CallCounter >= CallLimitDaily) {
                // If the api key is limited and the call counter is higher than the specified limit,
                // Throw an exception
                    throw new AlphaVantageApiCallNotPossible($"The number of calls today ({curStatus.LastCallTime.Date.ToString()}) " + 
                                            $"have reached the limit. Counted {curStatus.CallCounter} compared to " + 
                                            $"{CallLimitDaily} possible!");
            }
            // Update the LatestStatus property with the latest status
            this.LatestStatus = curStatus;
        }

        /***
        *  This method ensures that the call counter in the status cache is incremented by 1
        */
        private async Task AddCallCounterAsync()
        {
            await UpdateStatusCacheAsync(1);
        }

        /***
        * This method updates the status cache of the connection to the Alpha Vantage API and 
        * writes the changes to the file.
        * Parameters: 
        *   (int) addCounter: The amount of calls that the call counter should be incremented by
        * Return: A Task object, ensures that exceptions are passed to the caller from the asynchronous method
        */
        private async Task UpdateStatusCacheAsync(int addCounter = 0) {
            // Create the FileStream object
            FileStream curStatusStream = File.Open(_connectionStatusCache, System.IO.FileMode.OpenOrCreate);
            // Read the cached status from file
            ConnectionStatus? curStatus = await JsonSerializer.DeserializeAsync<ConnectionStatus>(curStatusStream);
            
            if (curStatus is null) {
                // If the file is empty, add a new cache object
                curStatus = new ConnectionStatus {
                    ApiKey = this._apiKey,
                    LastCallTime = DateTime.Today,
                    CallCounter = addCounter,
                    IsLimited = this.IsLimited
                };
            }     
            else {
                // Ensure that the callcounter is reset if the last timestamp was yesterday (local time).
                CleanStatusCache(curStatus);               

                if (this.IsLimited && curStatus.CallCounter >= CallLimitDaily) {
                    // Close file with no writing if the call counter exceeds the specified limit
                    curStatusStream.Dispose();
                    throw new AlphaVantageApiCallNotPossible($"The number of calls today ({curStatus.LastCallTime.Date.ToString()}) " + 
                                            $"have reached the limit. Counted {curStatus.CallCounter} compared to " + 
                                            $"{CallLimitDaily} possible!");
                }
            }
            // Increment the call counter
            curStatus.CallCounter += addCounter;

            // Remove existing data from the StatusCache file
            curStatusStream.SetLength(0);

            // Write to file
            await JsonSerializer.SerializeAsync<ConnectionStatus>(curStatusStream, curStatus);
            curStatusStream.Dispose();
            // Update the status cache property
            this.LatestStatus = curStatus;
        }

        /***
        * This method ensures that the status cache is cleared if the last registred date is from the previous day
        * Parameters: 
        *   (ConnectionStatus) curStatus: The status that shoul be verified and changed
        */
        private void CleanStatusCache(ConnectionStatus curStatus)
        {
            if (DateTime.Today.Date == curStatus.LastCallTime.AddDays(1).Date) 
                {
                    // The last call was yesterday. Set call counter to 0
                    curStatus.CallCounter = 0;
                    curStatus.LastCallTime = DateTime.Today;                
                }
        }

        /***
        * This method reads the ConnectionStatusCache file and returns the current ConnectionStatus object.
        * The method creates a new ConnectionStatusCache file if it does not exist and adds a new ConnectionStatus
        * object if the file is empty.
        */
        private async Task<ConnectionStatus> ReadStatusFileAsync() 
        {
            ConnectionStatus? latestStatus;
            Stream statusFileStream;
            try {
                // Try to read the ConnectionStatusCache file
                statusFileStream = File.OpenRead(_connectionStatusCache);
                latestStatus = await JsonSerializer.DeserializeAsync<ConnectionStatus>(statusFileStream);
            } catch (FileNotFoundException) 
            {
                // Create the ConnectionStatusCache file
                statusFileStream =  File.Create(_connectionStatusCache);
                latestStatus = null;
            }
            
            if (latestStatus is null) {
                // If the status cache was empty, create a new ConnectionStatus object
                ConnectionStatus initialStatus = new ConnectionStatus {
                    ApiKey = this._apiKey,
                    CallCounter = 0,
                    LastCallTime = DateTime.Today,
                    IsLimited = this.IsLimited
                };
                // Write the new ConnectionStatus to file
                await JsonSerializer.SerializeAsync<ConnectionStatus>(statusFileStream, initialStatus);
                statusFileStream.Dispose();
                // Return the ConnectionStatus
                return initialStatus;
            }
            // Return the existing ConnectionStatus
            statusFileStream.Dispose();
            return latestStatus;
        }


        
    }

    /***
    * This is a custom Exception object used in cases where the aplpha vantage apikey has exceeded the limits,
    * and cannot be used for a period.
    */
    public class AlphaVantageApiCallNotPossible : Exception
    {
        // Defining custom exceptions: 
        // https://learn.microsoft.com/en-us/dotnet/standard/exceptions/how-to-create-user-defined-exceptions
        public AlphaVantageApiCallNotPossible() {}

        public AlphaVantageApiCallNotPossible(string msg) : base(msg) {}

        public AlphaVantageApiCallNotPossible(string msg, Exception inner) : base(msg, inner) {}
    }
}

