import React, {useState, useEffect} from 'react';
import SearchResultRow from './SearchResultRow';

type StockMarketProps = {
    UserId : number
}

type SearchResult = {
    SearchKeyword : string
    SearchTime : string
    StockList : Array<SearchResultStock>
}

type SearchResultStock = {
        // The unique identifier of a stock
        symbol : string,
        // The stock name
        stockName : string,
        // The stock type (e.g equity)
        type : string,
        // The currency that the stock is traded in
        stockCurrency : string,
        // Object create timestamp
        lastUpdated : string
        isFavorite : boolean
}

function StockMarket(props : StockMarketProps) : JSX.Element {

    const [curStockList, setCurStockList] = useState<JSX.Element>(<></>);

    const getSearchResults =  async (keyword : string, userId : number) : Promise<any> => {
        return await fetch(`/trading/getUserSearchResult?keyword=${keyword}&userId=${userId}`, {
            method: "Get",
            headers: {
                'Accept': 'application/json',
                'Content-Type': 'application/json'
            }
        }).then((resp) => {
            if (!resp.ok) {
                throw new Error(`The server responded with error code ${resp.status}`);
            }
            return resp.json();
        }).then((data) => {
            const outList : JSX.Element[] = [];
            data.stockList.forEach((stock : SearchResultStock, index : number) => {
                outList.push(
                    <SearchResultRow Stock={stock} UserId={userId}></SearchResultRow>
                )
            });

            const searchList = <>
                <thead>
                    <tr>
                        <th>Stock Name</th>
                        <th>Stock Symbol</th>
                        <th>Add as favorite</th>
                    </tr>
                </thead>
                <tbody>
                    {outList}
                </tbody>
            </>;
            setCurStockList(searchList);
        }).catch((errorResp) => {
            console.log(errorResp.message);
        });
    }

    const createStockList = (e : React.KeyboardEvent<HTMLInputElement>) => {
        if (e.key === "Enter") {
            const keyword = e.currentTarget.value;
            // Validate input
            getSearchResults(keyword, props.UserId);
        }

    }

    return(
        <div id="StockMarketContainer" className="stockListContainer">
            <h2>Stock Market</h2>
            <div className="form-floating mb-3 mt-3">
                <input id="StockMarketInput"
                       onKeyDown={createStockList}
                       className={"form-control "} 
                       type="text" name="StockSearchField" 
                       placeholder='Enter a stock symbol or company name' 
                    />
                    <label htmlFor='StockMarketInput'>Enter a stock symbol or company name</label>
                <table className='table'>
                    {curStockList}
                </table>
            </div>
        </div>
    );
}
export { SearchResultStock };
export default StockMarket;