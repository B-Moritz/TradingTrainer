import { SearchResultStock } from './StockMarket';
import {useState, useEffect} from 'react';


type SearchResultRowProps = {
    Stock : SearchResultStock
    UserId : number
}

function SearchResultRow(props : SearchResultRowProps) : JSX.Element {
    const [isFavorite, setIsFavorite] = useState(props.Stock.isFavorite);
    const [isWaiting, setIsWaiting] = useState(false);
    const addToWatchlist = (e : React.MouseEvent) => {
        setIsWaiting(true);
        fetch(`/trading/addToFavoriteList?userId=${props.UserId}&symbol=${props.Stock.symbol}`, {
            method: "POST",
            headers: {
                'Accept': 'application/json',
                'Content-Type': 'application/json'
            }}
            ).then((response) => {
                if (!response.ok) {
                    throw new Error(`The server responded with error code ${response.status}.`);
                }
                setIsFavorite(true);
                setIsWaiting(false);
            }).catch((errorResp) => {
                    console.log(errorResp.message);
            });
    }

    const removeFromWatchlist = (e : React.MouseEvent) => {
        setIsWaiting(true);
        fetch(`/trading/deleteFromFavoriteList?userId=${props.UserId}&symbol=${props.Stock.symbol}`, {
            method: "POST",
            headers: {
                'Accept': 'application/json',
                'Content-Type': 'application/json'
            }}
            ).then((response) => {
                if (!response.ok) {
                    throw new Error(`The server responded with error code ${response.status}.`);
                }
                setIsFavorite(false);
                setIsWaiting(false);
            }).catch((errorResp) => {
                    console.log(errorResp.message);
            });
    }
    let button;
    if (!isWaiting) {
        button = (isFavorite ? <button name={"Stock_ " + props.Stock.symbol} className="btn btn-warning" onClick={removeFromWatchlist}>Remove from watchlist</button> : <button name={"Stock_ " + props.Stock.symbol} className="btn btn-secondary" onClick={addToWatchlist}>Add to watchlist</button>);
    } else {
        button = <div className='spinner-border text-black'></div>;
    }
    

    return(
        <tr>
            <td>{props.Stock.symbol}</td>
            <td>{props.Stock.stockName}</td>
            <td>{button}</td>
        </tr>
    );
}

export default SearchResultRow;