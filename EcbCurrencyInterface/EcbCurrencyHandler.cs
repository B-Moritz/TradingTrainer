// Webapplikasjoner oblig 1     OsloMet     28.10.2022

// This file contains code used to handle the currency information obtained by the EcbXmlReader.

// The currency calculator was created using the following resources:
// https://www.codementor.io/@dewetvanthomas/tutorial-currency-converter-application-for-c-121yicb1es


using System.Text.RegularExpressions;
using System.Globalization;

namespace EcbCurrencyInterface;

/***
 * This class contains the utility classes used to obtain specific exchange rates from the rss feed of the 
 * European Central Bank (ECB).
 */
public static class EcbCurrencyHandler
{
    // The rss base uri used for the http requests
    private static string _rssUri = "https://www.ecb.europa.eu/rss/fxref-";

    /***
     * This method obtains the exchange rate for the target currency specified
     *      n base = exhangeRate * n target
     * Parameters:
     *      (string) baseCurr: The base currency in the 
     *      (string) targetCurr: The target currency. 
     * Return: The exchange rate is returned (decimal type). The exchange rate is equal to 
     * 1 quantity of the base value in target value:
     * 1 base = exchangeRate * 1 target
     * */
    public async static Task<decimal> GetExchangeRateAsync(string baseCurr, string targetCurr)
    {
        // Verify that the currencies are specified in the right format:
        Regex currencyPattern = new Regex("[a-zA-Z]{3}");
        if (!currencyPattern.IsMatch(baseCurr))
        {
            // Verify the base currency
            throw new ArgumentException("The baseCurr argument has not the right format." + 
                                        " Make sure that it is a 3 character string like USD.");
        }

        if (!currencyPattern.IsMatch(targetCurr))
        {
            // Verify the target currency
            throw new ArgumentException("The targetCurr argument has not the right format." + 
                                        " Make sure that it is a 3 character string like USD.");
        }

        if (string.Compare(targetCurr, baseCurr) == 0)
        {
            // Return 1 if the target and base is equal
            return 1;
        }

        decimal exchangeRateTarget;
        if (string.Compare(targetCurr.ToLower(), "eur") == 0) 
        {
            // If the target is Euro, use base currency as target and take the inverse
            exchangeRateTarget = await GetExchangeRateEuroBase(baseCurr);
            return 1/exchangeRateTarget;
        }
        else 
        {
            // The euro is not target. Normal case 
            exchangeRateTarget = await GetExchangeRateEuroBase(targetCurr);
            if (baseCurr.ToLower() != "eur") 
            {
                // Since the euro is neither target or base, we need to get the exchange rate between euro and base currency
                // n euro = exchangeRateEuroBase * n target
                // n euro = echangeRateBaseEuroBase * n base
                //<=> n base = n target * (exchangeRateEuroBase / echangeRateBaseEuroBase)
                // return value = (exchangeRateEuroBase / echangeRateBaseEuroBase)
                decimal echangeRateBase = await GetExchangeRateEuroBase(baseCurr);
                // The real exchange rate is calculated and returned
                return (exchangeRateTarget / echangeRateBase);
            }
            // Eur is the base
            return exchangeRateTarget;
        }
    }

    /***
     * This method uses the EcbXmlReader to obtain the currency information from Ecb
     * Parameters:
     *      (string) targetCurr: The target currency to find the exchange rate for
     * Return: The method returns the exchangerate between euro (base currency) and the specified target currency
     */
    private async static Task<decimal> GetExchangeRateEuroBase(string targetCurr) 
    {
        // Creation of the xml reader containing the response xml from ECB
        EcbXmlReader ecbReader = await EcbXmlReader.BuildEcbXmlReaderAsync(_rssUri + targetCurr.ToLower() + ".html");

        // Definition of the navigation strings used to traverse the xml file
        string[] navigationNodes = {"item"};
        string[] targetNodes = {"dc:date", "cb:value"};
        // Extract the two target values
        string[]? result = ecbReader.GetNodeValues(navigationNodes, targetNodes);

        if (result is null)
        {
            throw new Exception($"Error while parsing the xml file: {_rssUri}");
        }
        // Parse the echange rate (cb:value)
        CultureInfo cultureInfo = new CultureInfo("en-US");
        decimal exchangeRate = Decimal.Parse(result[1], cultureInfo);
        return exchangeRate;
    }
}