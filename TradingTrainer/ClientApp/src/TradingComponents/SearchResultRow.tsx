import { SearchResultStock } from './StockMarket';
import {useState, useEffect} from 'react';


type SearchResultRowProps = {
    Stock : SearchResultStock
    UserId : number
}

function SearchResultRow(props : SearchResultRowProps) : JSX.Element {
    const [isFavorite, setIsFavorite] = useState(props.Stock.isFavorite);

    const addToWatchlist = (e : React.MouseEvent) => {
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
            }).catch((errorResp) => {
                    console.log(errorResp.message);
            });
    }

    const removeFromWatchlist = (e : React.MouseEvent) => {
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
            }).catch((errorResp) => {
                    console.log(errorResp.message);
            });
    }

    return(
        <tr>
            <td>{props.Stock.symbol}</td>
            <td>{props.Stock.stockName}</td>
            <td>{(isFavorite ? <button name={props.Stock.symbol} className="btn btn-warning" onClick={removeFromWatchlist}>Remove from watchlist</button> : <button className="btn btn-secondary" onClick={addToWatchlist}>Add to watchlist</button>)}</td>
        </tr>
    );
}

export default SearchResultRow;