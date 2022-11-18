import { useNavigate } from 'react-router-dom';

const navigate = useNavigate()

async function fetchFromTradingApi(requestUrl : string) : Promise<any> {
    return fetch(requestUrl).then((response) => {
        if (!response.ok) {
            // The server responded with an error
            if (response.status === 401) {
                // User is not authorized to access the endpoint
                navigate("/Login");
            }
            const msg = `Error: The server responded with error code: ${response.status}\n \
            Message: ${response.text}`;

            throw new Error(msg);
        }
        return response.json();
    }).catch((errorResp) => {
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