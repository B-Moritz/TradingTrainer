import React, {useState} from 'react';
import {DateTimeFormat} from './DisplayUtilities';

type StockBase = {
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
}

function StockBaseHeader() : JSX.Element {
    return(
        <tr>
            <th>Symbol</th>
            <th>Name</th>
            <th>Type</th>
            <th>Currency</th>
            <th>Last updated</th>
        </tr>
    );
}

type StockBaseRowProps = {
    CurStockBase : StockBase
    SetCurSelectedStock : React.Dispatch<React.SetStateAction<StockBase | undefined>>
}

function StockBaseRow(props : StockBaseRowProps) : JSX.Element {
    const [isSelected, setIsSelected] = useState(false);
    const selectStock = (e : React.MouseEvent) => {
        setIsSelected(!isSelected);
        props.SetCurSelectedStock(props.CurStockBase);
    }
    return(
        <tr onClick={selectStock} className={(isSelected ? "stockSelected" : "")}>
            <td>{props.CurStockBase.symbol}</td>
            <td>{props.CurStockBase.stockName}</td>
            <td>{props.CurStockBase.type}</td>
            <td>{props.CurStockBase.stockCurrency}</td>
            <td><DateTimeFormat rawDateTimeFormat={props.CurStockBase.lastUpdated}></DateTimeFormat></td>
        </tr>
    );
}
export {StockBase, StockBaseHeader};
export default StockBaseRow;