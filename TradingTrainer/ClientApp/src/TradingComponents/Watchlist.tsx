import StockBaseRow, {StockBase, StockBaseHeader} from './StockBaseRow';
import {DateTimeFormat} from './DisplayUtilities';
import RefreshIcon from '../img/icons8-update-left-rotation-50.png';
import React, { useState, useEffect } from 'react';
import { User } from '../LoginForm';
import WaitingDisplay from '../WaitingDisplay';
import { getWatchlist } from '../Service/TradingApi';

type WatchlistResponse = {
    lastUpdated : string,
    stockList : Array<StockBase>
}

type WatchlistProps = {
    SetCurSelectedStock : React.Dispatch<React.SetStateAction<StockBase>>
    SetBuyDialogIsActive : React.Dispatch<React.SetStateAction<boolean>>
    UpdateQuoteDisplay : (symbol : string) => Promise<any>
    User : User,
    SetStockListWaiting : React.Dispatch<React.SetStateAction<JSX.Element>>,
    CurSelectedStock : StockBase
}

type OutList = {
    lastUpdated : string,
    elements : JSX.Element[]
}

function Watchlist(props : WatchlistProps) : JSX.Element {
    const [outList, setOutList] = useState<OutList>({
        lastUpdated : "",
        elements : []
    });

    useEffect(() => {
        initialList();
    }, [])

    const initialList = () => {
        let curOutList : JSX.Element[] = [];
        let isSelected = false;
        getWatchlist(props.User.id).then((data : WatchlistResponse) => {
            if (data.stockList !== undefined) {
                data.stockList.forEach((stock : any, index : number) => {
                    if (index === 0) {
                        props.SetCurSelectedStock(stock);
                        isSelected = true;
                        props.UpdateQuoteDisplay(stock.symbol);
                    }
                    curOutList.push(<StockBaseRow 
                                    key={"favStock_" + stock.symbol} 
                                    CurStockBase={stock} 
                                    IsSelected={isSelected}
                                    SelectStock={selectStock}
                                ></StockBaseRow>); 
                    isSelected = false;
                });
                if (curOutList.length === 0) {
                    curOutList.push(<div className="emptyTableDisp" key="EmptyWatchlistDisp"><p>Empty table</p></div>);    
                }
                setOutList({
                    lastUpdated : data.lastUpdated,
                    elements : curOutList
                });
            } else {
                setOutList({
                    lastUpdated : "Not awailable",
                    elements : [<div className="emptyTableDisp" key="EmptyWatchlistDisp"><p>Empty table</p></div>]
                });
            }
        });
    }

    const refresh = (e : React.MouseEvent) => {
        selectStock(props.CurSelectedStock);
    }
    
    const selectStock = (selectedStock : StockBase) => {
        let curOutList : JSX.Element[] = [];
        let isSelected = false;
        getWatchlist(props.User.id).then((data : WatchlistResponse) => {
            if (data.stockList !== undefined) {
                data.stockList.forEach((stock : any, index : number) => {
                    if (stock.symbol === selectedStock.symbol) {
                        props.SetCurSelectedStock(stock);
                        isSelected = true;
                        props.UpdateQuoteDisplay(stock.symbol);
                    }
                    curOutList.push(<StockBaseRow 
                                    key={"favStock_" + stock.symbol} 
                                    CurStockBase={stock} 
                                    IsSelected={isSelected}
                                    SelectStock={selectStock}
                                ></StockBaseRow>);
                    isSelected = false;
                });
                if (curOutList.length === 0) {
                    curOutList.push(<div className="emptyTableDisp" key="EmptyWatchlistDisp"><p>Empty table</p></div>); 
                } else {
                    setOutList({
                        lastUpdated : data.lastUpdated,
                        elements : curOutList
                    });
                }
            } else {
                setOutList({
                    lastUpdated : "Not awailable",
                    elements : [<div className="emptyTableDisp" key="EmptyWatchlistDisp"><p>Empty table</p></div>]
                });
            }
        })

    }

    return(
        <>
            <div id="WatchlistContainer" className="stockListContainer">
                <h2>Watchlist</h2>
                <p>Click on a stock in the following list to see details</p>
                <p>Last updated: <DateTimeFormat rawDateTimeFormat={outList.lastUpdated}></DateTimeFormat></p>
                <table className='table'>
                    <thead>
                        <StockBaseHeader></StockBaseHeader>
                    </thead>
                    <tbody>
                        {outList.elements}
                    </tbody>
                </table>
                <div className="btn-group" role="group">
                    <button className="btn btn-lg btn-primary" onClick={refresh}>Refresh</button>
                    <button onClick={() => {props.SetBuyDialogIsActive(true);}} type="button" className="btn btn-outline btn-lg btn-success">Buy</button>
                </div>
            </div>
            { outList.elements.length === 0 && <WaitingDisplay WaitingText={"Retreiving watchlist from server..."}></WaitingDisplay> }
        </>
        
    );
}

export { WatchlistResponse };
export default Watchlist;