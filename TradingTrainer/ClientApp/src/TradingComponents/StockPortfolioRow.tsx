import React, {useState} from 'react';
import {DateTimeFormat} from './DisplayUtilities';
import { PortfolioStock } from './Portfolio';
import { ColorPrice } from './DisplayUtilities';

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
    Stock : PortfolioStock
    SelectStock: (selectedStock : PortfolioStock) => void
    IsSelected: boolean
}

function StockPortfolioRow(props : StockPortfolioRowProps) : JSX.Element {
    const selectStock = (e : React.MouseEvent) => {
        props.SelectStock(props.Stock);
    }

    return(
        <div onClick={(props.IsSelected ? () => {} : selectStock)} className={"portfolioRow " + (props.IsSelected ? "stockSelected" : "")}>
            <div id="StockSymbolPortfolio" className='portfolioRowElement fw-bold'>
                {props.Stock.symbol}
            </div>
            <div id="StockNamePortfolio" className='portfolioRowElement'>
                <p className='rowTitle fw-bold'>Stock Name:</p>
                <p>{props.Stock.stockName}</p>
            </div>
            <div id="StockQuantityPortfolio" className='portfolioRowElement'>
                <p className='rowTitle fw-bold'>Quantity:</p>
                <p>{props.Stock.quantity}</p>
            </div>
            <div id="StockPricePortfolio" className='portfolioRowElement'>
                <p className='rowTitle fw-bold'>Estimated price per share:</p>
                <p>{props.Stock.estPrice}</p>
            </div>
            <div id="PortionPortfolio" className='portfolioRowElement'>
                <p className='rowTitle fw-bold'>Portfolio portion:</p>
                <p>{props.Stock.portfolioPortion}</p>
            </div>
            <div id="MarketValuePortfolio" className='portfolioRowElement'>
                <p className='rowTitle fw-bold'>Estimated market value:</p>
                <p>{props.Stock.estTotalMarketValue}</p>
            </div>
            <div id="TotalConstPortfolio" className='portfolioRowElement'>
                <p className='rowTitle fw-bold'>Funds spent on stock:</p>
                <p>{props.Stock.totalCost}</p>
            </div>
            <div id="UnrealizedPortfolio" className='portfolioRowElement'>
                <p className='rowTitle fw-bold'>Unrealized Profit/Loss:</p>
                <ColorPrice RawValue={props.Stock.unrealizedPL}></ColorPrice>
            </div>
        </div>
    );
}
export type {StockBase, StockBaseHeader};
export default StockPortfolioRow;