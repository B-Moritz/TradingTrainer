import StockBaseRow, {StockBase, StockBaseHeader} from './StockBaseRow';
import {DateTimeFormat} from './DisplayUtilities';
import RefreshIcon from '../img/icons8-update-left-rotation-50.png';
import React from 'react';

type WatchlistResponse = {
    lastUpdated : string,
    stockList : Array<StockBase>
}

type WatchlistProps = {
    RefreshCallback : Function
    ContentData : WatchlistResponse 
    SetCurSelectedStock : React.Dispatch<React.SetStateAction<StockBase | undefined>>
}

function Watchlist(props : WatchlistProps) : JSX.Element {
    let emptyDisp : JSX.Element = <></>;
    let outList : JSX.Element[] = [];
    if (props.ContentData.stockList !== undefined) {
        props.ContentData.stockList.forEach((stock, index) => {
            outList.push(<StockBaseRow key={"favStock_" + stock.symbol} CurStockBase={stock} SetCurSelectedStock={props.SetCurSelectedStock}></StockBaseRow>)
        });
        if (outList.length <= 0) {
            emptyDisp = <div className="emptyTableDisp" key="EmptyWatchlistDisp"><p>Empty table</p></div>;    
        }
    } else {
        emptyDisp = <div className="emptyTableDisp" key="EmptyWatchlistDisp"><p>Empty table</p></div>;
    }

    const refresh = (e : React.MouseEvent) => {
        props.RefreshCallback();
    }
    

    return(
        <div id="WatchlistContainer" className="stockListContainer">
            <h2>Watchlist</h2>
            <p>Click on a stock in the following list to see details</p>
            <p>Last updated: <DateTimeFormat rawDateTimeFormat={props.ContentData.lastUpdated}></DateTimeFormat></p>
            <table className='table'>
                <thead>
                    <StockBaseHeader></StockBaseHeader>
                </thead>
                <tbody>
                    {outList}
                </tbody>
            </table>
            {emptyDisp}
            <button className="btn btn-lg btn-primary" onClick={refresh}>
                Refresh</button>
        </div>
    );
}

export { WatchlistResponse };
export default Watchlist;