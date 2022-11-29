import { SearchResultStock } from './StockMarket';
import {useState, useEffect} from 'react';
import { useNavigate } from 'react-router-dom';
import { deleteFromFavorite, addToFavorite } from '../Service/TradingApi';


type SearchResultRowProps = {
    Stock : SearchResultStock
    UserId : number
    SetErrorMsg : React.Dispatch<React.SetStateAction<string>>
}

function SearchResultRow(props : SearchResultRowProps) : JSX.Element {
    const [isFavorite, setIsFavorite] = useState(props.Stock.isFavorite);
    const [isWaiting, setIsWaiting] = useState(false);
    const navigate = useNavigate();


    const addToWatchlist = async (e : React.MouseEvent) => {
        setIsWaiting(true);
        await addToFavorite(props.UserId, props.Stock.symbol).then((data) => {
            setIsFavorite(true);
            setIsWaiting(false);
        }).catch((error : Error) => {
            setIsWaiting(false);
            if (error.message.slice(3) === "401") {
                navigate("/login");
            }
            props.SetErrorMsg(error.message);
        });
    }

    const removeFromWatchlist = async (e : React.MouseEvent) => {
        setIsWaiting(true);
        await deleteFromFavorite(props.UserId, props.Stock.symbol).then((data) => {
            setIsFavorite(false);
            setIsWaiting(false);
        }).catch((error : Error) => {
            setIsWaiting(false);
            if (error.message.slice(3) === "401") {
                navigate("/login");
            }
            props.SetErrorMsg(error.message);
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