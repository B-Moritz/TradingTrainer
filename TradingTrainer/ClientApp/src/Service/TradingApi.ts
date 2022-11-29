import { User } from '../LoginForm';

type ValidatedResponse = {
    IsOk : boolean,
    Code : number,
    Title : string
    Data : Promise<any>
}

async function fetchFromTradingApi(requestUrl : string) : Promise<any> {
    const response = await fetch(requestUrl, {
        method: "GET",
        headers: {
            'Accept': 'application/json',
            'Content-Type': 'application/json'
        }
    });
    const curResponseObj : ValidatedResponse = {
        IsOk : response.ok,
        Code : response.status,
        Title : response.statusText,
        Data : await response.json()
    }

    if (curResponseObj.IsOk) {
        return curResponseObj.Data;
    }
    // User is not authorized to access the endpoint
    const msg = `Error: The server responded with error code: ${curResponseObj.Code}\n\n \
    Message: ${curResponseObj.Data}`;
    alert(msg);
    console.log(msg);
    throw new Error(`${curResponseObj.Code}: ${curResponseObj.Title}: ${curResponseObj.Data}`);    
}

async function putTradingApi(requestUrl : string, newObject : any) : Promise<any> {
    const response = await fetch(requestUrl, {
        method: "PUT",
        headers: {
            'Accept': 'application/json',
            'Content-Type': 'application/json'
        },
        body: JSON.stringify(newObject)
    });
    const curResponseObj : ValidatedResponse = {
        IsOk : response.ok,
        Code : response.status,
        Title : response.statusText,
        Data : await response.json()
    }

    if (curResponseObj.IsOk) {
        return curResponseObj.Data;
    }
    // User is not authorized to access the endpoint
    const msg = `Error: The server responded with error code: ${curResponseObj.Code}\n\n \
    Message: ${curResponseObj.Data}`;
    alert(msg);
    console.log(msg);
    throw new Error(`${curResponseObj.Code}: ${curResponseObj.Title}: ${curResponseObj.Data}`);  
}

async function patchTradingApi(requestUrl : string, newObject : any) : Promise<any> {
    const response = await fetch(requestUrl, {
        method: "PATCH",
        headers: {
            'Accept': 'application/json',
            'Content-Type': 'application/json'
        },
        body: JSON.stringify(newObject)
    });
    const curResponseObj : ValidatedResponse = {
        IsOk : response.ok,
        Code : response.status,
        Title : response.statusText,
        Data : await response.json()
    }

    if (curResponseObj.IsOk) {
        return curResponseObj.Data;
    }
    // User is not authorized to access the endpoint
    const msg = `Error: The server responded with error code: ${curResponseObj.Code}\n\n \
    Message: ${curResponseObj.Data}`;
    alert(msg);
    console.log(msg);
    throw new Error(`${curResponseObj.Code}: ${curResponseObj.Title}: ${curResponseObj.Data}`);  
}

async function patchTradingApiSimple(requestUrl : string) : Promise<any> {
    const response = await fetch(requestUrl, {
        method: "PATCH",
        headers: {
            'Accept': 'application/json',
            'Content-Type': 'application/json'
        }
    });
    const curResponseObj : ValidatedResponse = {
        IsOk : response.ok,
        Code : response.status,
        Title : response.statusText,
        Data : await response.json()
    }

    if (curResponseObj.IsOk) {
        return curResponseObj.Data;
    }
    // User is not authorized to access the endpoint
    const msg = `Error: The server responded with error code: ${curResponseObj.Code}\n\n \
    Message: ${curResponseObj.Data}`;
    alert(msg);
    console.log(msg);
    throw new Error(`${curResponseObj.Code}: ${curResponseObj.Title}: ${curResponseObj.Data}`);  
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
    const requestUrl = `/trading/getStockQuote?symbol=${symbol}`;
    return await fetchFromTradingApi(requestUrl);
}

export async function getTradeHistory(userId : number) : Promise<any> {
    const requestUrl = `/trading/getAllTrades?userId=${userId}`;
    return await fetchFromTradingApi(requestUrl);
}

export async function getUserProfile(userId : number) : Promise<any> {
    const requestUrl = `/trading/getUser?userId=${userId}`;
    return await fetchFromTradingApi(requestUrl);
}

export async function saveUserProfile(user : User) : Promise<User> {
    const requestUrl = `/trading/updateUser`;
    return await putTradingApi(requestUrl, user);
}

export async function resetUserProfile(userId : number) : Promise<User> {
    const requestUrl = `/trading/resetProfile?userId={userId}`;
    return await patchTradingApiSimple(requestUrl);
}

export async function resetUserPwd(userId : number, newPwd : string) : Promise<any> {
    const requestUrl = `/trading/resetPwd`;
    return await patchTradingApi(requestUrl, {userId : userId, password : newPwd})
}

export async function resetTradeHistory(userId : number) : Promise<any> {
    const requestUrl = `/trading/ClearAllTradeHistory?userId=${userId}`;
    return await patchTradingApiSimple(requestUrl)
}

export async function logoutApiCall() : Promise<any> {
    const requestUrl = `/trading/logOut`;
    return await patchTradingApiSimple(requestUrl);
}

export async function deleteFromFavorite(userId : number, symbol : string) : Promise<any> {
    const requestUrl = `/trading/deleteFromFavoriteList?userId=${userId}&symbol=${symbol}`;
    return await patchTradingApiSimple(requestUrl);
}

export async function addToFavorite(userId : number, symbol : string) : Promise<any> {
    const requestUrl = `/trading/addToFavoriteList?userId=${userId}&symbol=${symbol}`;
    return await patchTradingApiSimple(requestUrl);
}
 