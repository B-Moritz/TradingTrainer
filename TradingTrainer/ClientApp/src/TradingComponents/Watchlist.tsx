import StockBaseRow, {StockBase, StockBaseHeader} from './StockBaseRow';
import {DateTimeFormat} from './DisplayUtilities';
import RefreshIcon from '../img/icons8-update-left-rotation-50.png';
import React, { useState, useEffect } from 'react';

type WatchlistResponse = {
    lastUpdated : string,
    stockList : Array<StockBase>
}

type WatchlistProps = {
    RefreshCallback : Function
    ContentData : WatchlistResponse 
    SetCurSelectedStock : React.Dispatch<React.SetStateAction<StockBase | undefined>>
    SetBuyDialogIsActive : React.Dispatch<React.SetStateAction<boolean>>
    UpdateQuoteDisplay : (symbol : string) => Promise<any>
}

function Watchlist(props : WatchlistProps) : JSX.Element {
    const [outList, setOutList] = useState<JSX.Element[]>();
    const [emptyDisp, setEmptyDisp] = useState<JSX.Element>();

    useEffect(() => {
        initialList();
    }, [])

    const initialList = () => {
        let curOutList : JSX.Element[] = [];
        let isSelected = false;
        if (props.ContentData.stockList !== undefined) {
            props.ContentData.stockList.forEach((stock, index) => {
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
            if (curOutList.length <= 0) {
                setEmptyDisp(<div className="emptyTableDisp" key="EmptyWatchlistDisp"><p>Empty table</p></div>);    
            } else {
                setOutList(curOutList);
            }
        } else {
            setEmptyDisp(<div className="emptyTableDisp" key="EmptyWatchlistDisp"><p>Empty table</p></div>);
        }
    }

    const refresh = (e : React.MouseEvent) => {
        props.RefreshCallback();
    }
    
    const selectStock = (selectedStock : StockBase) => {
        let curOutList : JSX.Element[] = [];
        let isSelected = false;
        if (props.ContentData.stockList !== undefined) {
            props.ContentData.stockList.forEach((stock, index) => {
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
            if (curOutList.length <= 0) {
                setEmptyDisp(<div className="emptyTableDisp" key="EmptyWatchlistDisp"><p>Empty table</p></div>); 
            } else {
                setOutList(curOutList);
            }
        } else {
            setEmptyDisp(<div className="emptyTableDisp" key="EmptyWatchlistDisp"><p>Empty table</p></div>);
        }
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
            <div className="btn-group" role="group">
                <button className="btn btn-lg btn-primary" onClick={refresh}>Refresh</button>
                <button onClick={() => {props.SetBuyDialogIsActive(true);}} type="button" className="btn btn-outline btn-lg btn-success">Buy</button>
            </div>
        </div>
    );
}

export { WatchlistResponse };
export default Watchlist;