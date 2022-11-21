async function fetchFromTradingApi(requestUrl : string) : Promise<any> {
    return fetch(requestUrl, {
        method: "Get",
        headers: {
            'Accept': 'application/json',
            'Content-Type': 'application/json'
        }
    }).then((response) => {
        if (!response.ok) {
            // The server responded with an error
            if (response.status === 401) {
                // User is not authorized to access the endpoint
                throw new Error(`${response.status}: ${response.text}`);
            }
            const msg = `Error: The server responded with error code: ${response.status}\n \
            Message: ${response.text}`;

            throw new Error(msg);
        }
        return response.json();
    }).catch((errorResp) => {
        if (errorResp.message.slice(3) === "401") {
            throw(errorResp);
        }
        alert(errorResp.message);
        console.log(errorResp.message);
    });
}

export async function getWatchlist(userId : number) : Promise<any> {
    const requestUrl = `/trading/getFavoriteList?userId=${userId}`;
    return await fetchFromTradingApi(requestUrl);
}

export async function getPortfolio(userId : number) : Promise<any> {
    const requestUrl = `/trading/getPortfolio?userId=${userId}`;
    return await fetchFromTradingApi(requestUrl);
} 

export async function getSearchResult(userId : number, keyword : string) : Promise<any> {
    const requestUrl = `/trading/getUserSearchResult?keyword=${keyword}&userId=${userId}`;
    return await fetchFromTradingApi(requestUrl);
}

export async function getStockQuote(symbol : string) : Promise<any> {
    const requestUrl = `/trading/GetStockQuote?symbol=${symbol}`;
    return await fetchFromTradingApi(requestUrl);
}

export async function getTradeHistory(userId : number) : Promise<any> {
    const requestUrl = `trading/GetAllTrades?userId=${userId}`;
    return await fetchFromTradingApi(requestUrl);
}