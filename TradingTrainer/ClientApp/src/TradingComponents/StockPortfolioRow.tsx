import React, {useState} from 'react';
import {DateTimeFormat} from './DisplayUtilities';
import { PortfolioStock } from './Portfolio';

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

type StockPortfolioRowProps = {
    CurPortfolioStock : PortfolioStock
    SelectStock: (selectedStock : PortfolioStock) => void
    IsSelected: boolean
}

function StockPortfolioRow(props : StockPortfolioRowProps) : JSX.Element {
    const selectStock = (e : React.MouseEvent) => {
        props.SelectStock(props.CurPortfolioStock);
    }

    /*
                        <th>Stock symbol</th>
                        <th>Stock Name</th>
                        <th>Quantity</th>
                        <th>Estimated price</th>
                        <th>Portfolio partition %</th>
                        <th>Estimated total value</th>
                        <th>Total cost</th>
                        <th>Unrealized profit/loss</th>
    */ 

    return(
        <tr onClick={selectStock} className={(props.IsSelected ? "stockSelected" : "")}>
            <td>{props.CurPortfolioStock.symbol}</td>
            <td>{props.CurPortfolioStock.stockName}</td>
            <td>{props.CurPortfolioStock.quantity}</td>
            <td>{props.CurPortfolioStock.estPrice}</td>
            <td>{props.CurPortfolioStock.portfolioPortion}</td>
            <td>{props.CurPortfolioStock.estTotalMarketValue}</td>
            <td>{props.CurPortfolioStock.totalCost}</td>
            <td>{props.CurPortfolioStock.unrealizedPL}</td>
        </tr>
    );
}
export {StockBase, StockBaseHeader};
export default StockPortfolioRow;