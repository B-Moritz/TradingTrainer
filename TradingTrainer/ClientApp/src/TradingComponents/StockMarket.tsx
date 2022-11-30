import React, {useState, useEffect} from 'react';
import { useNavigate } from 'react-router-dom';
import SearchResultRow from './SearchResultRow';
import { getSearchResult } from '../Service/TradingApi';
import { User } from '../LoginForm';
import WaitingDisplay from '../WaitingDisplay';
import { Navigate } from 'react-router-dom';

type StockMarketProps = {
    User : User
    SetErrorMsg : React.Dispatch<React.SetStateAction<string>>
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
    const navigate = useNavigate();
    const [curStockList, setCurStockList] = useState<JSX.Element[]>([]);
    const [waitDisplay, setWaitDisplay] = useState<JSX.Element>(<></>)
    const [curKeyword, setCurKeyword] = useState("");

    const updateSearchResult =  async (userId : number, keyword : string) : Promise<any> => {
        // If the keyword is empty - display empty table
        if (keyword === "") {
            setCurStockList([<tr className="emptyTableDisp" key="EmptySearchResult"><td>No stock was found.</td><td></td><td></td></tr>]);
            return;
        }
        // Displaying waiting before the http call is made
        setWaitDisplay(<WaitingDisplay WaitingText={"Retreiving search results from server..."}></WaitingDisplay>);
        // Make the http call towards the server
        getSearchResult(userId, keyword).then((data) => {
            const outList : JSX.Element[] = [];
            data.stockList.forEach((stock : SearchResultStock, index : number) => {
                outList.push(
                    <SearchResultRow key={"Stock_" + stock.symbol} Stock={stock} UserId={userId} SetErrorMsg={props.SetErrorMsg}></SearchResultRow>
                )
            });
            if (outList.length === 0) {
                // No stocks were found
                outList.push(<tr className="emptyTableDisp" key="EmptySearchResult"><td>No stock was found.</td><td></td><td></td></tr>)
            }
            setCurStockList(outList);
            setWaitDisplay(<></>)
        }).catch((error : Error) => {
            setWaitDisplay(<></>)
            if (error.message.slice(3) === "401") {
                navigate("/login");
            }
            props.SetErrorMsg(error.message);
        });
    }

    const createStockList = (e : React.KeyboardEvent<HTMLInputElement>) => {
        if (e.key === "Enter") {
            //const keyword = e.currentTarget.value;
            // Validate input
            updateSearchResult(props.User.id, curKeyword);
        }
    }

    return(
        <>
            <div id="StockMarketContainer" className="stockListContainer">
                <h2>Stock Market</h2>
                <div className="form-floating mb-3 mt-3">
                    <input id="StockMarketInput"
                        onKeyDown={createStockList}
                        onChange={(e : React.FormEvent<HTMLInputElement>) => setCurKeyword(e.currentTarget.value)}
                        className={"form-control "}
                        type="text" name="StockSearchField" 
                        placeholder='Enter a stock symbol or company name' 
                        />
                    <label htmlFor='StockMarketInput'>Enter a stock symbol or company name</label>
                    <button className="btn btn-primary my-3" onClick={() => updateSearchResult(props.User.id, curKeyword)}>Search</button>
                    <table className='table'>
                        {curStockList.length > 0 && 
                        <>
                            <thead>
                                <tr>
                                    <th>Stock Symbol</th>
                                    <th>Stock Name</th>
                                    <th>Add as favorite</th>
                                </tr>
                            </thead>
                            <tbody>
                                {curStockList}
                            </tbody>
                        </>
                        }
                    </table>
                </div>
            </div>
            {waitDisplay}
        </>

    );
}
export type { SearchResultStock };
export default StockMarket;