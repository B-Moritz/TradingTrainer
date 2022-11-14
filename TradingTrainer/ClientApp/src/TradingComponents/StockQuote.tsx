import {useEffect, useState} from 'react';
import {StockBase} from './StockBaseRow';
import WaitingDisplay from '../WaitingDisplay';
import { DateTimeFormat, ColorPrice } from './DisplayUtilities';

type StockQuote = {
    // The unique identifier of a stock
    symbol : string,
    // The stock name
    stockName : string,
    // The stock type (e.g equity)
    type : string,
    // The currency that the stock is traded in
    stockCurrency : string,
    // Object create timestamp
    lastUpdated : string,
    open : string,
    high : string,
    low : string,
    price : string,
    volume : number,
    latestTradingDay : string,
    previousClose : string,
    change : string,
    changePercent : string

}

type StockQuoteProps = {
    CurStock : StockBase | undefined
}

function StockQuote(props : StockQuoteProps) : JSX.Element {

    const [emptyQuote, setEmptyQuote] = useState(<div id="EmptyQuote"><p>No quote data</p></div>);
    const [quoteWaiting, setQuoteWaiting] = useState(<></>);
    const [quoteDisplay, setQuoteDisplay] = useState(<></>);
    
    useEffect(() => {
        if (props.CurStock === undefined) {
            setEmptyQuote(<div id="EmptyQuote"><p>No quote data</p></div>);
        } else {
            updateQuoteDisplay()
        }

    }, [props.CurStock]);

    const getStockQuote = async () : Promise<any> => {
        const quoteEndpointUrl = `/trading/GetStockQuote?symbol=${props.CurStock?.symbol}`;
        return fetch(quoteEndpointUrl).then((response) => {
            if (!response.ok) {
                throw new Error(`Server responded with an error: ${response.status}`);
            }
            return response.json();
        }).catch((err) => {
            alert(err.Message);
        })
    }

    const updateQuoteDisplay = async () => {
        // Get the quote from server
        setQuoteWaiting(<WaitingDisplay WaitingText={"Retreiving Quote from server..."}></WaitingDisplay>);
        const data : StockQuote = await getStockQuote();
        setQuoteDisplay(
            <div>
                <label>Symbol: </label>
                <span>{data.symbol}</span><br />
                <label>Name: </label>
                <span>{data.stockName}</span><br />
                <label>Last updated: </label>
                <DateTimeFormat rawDateTimeFormat={data.lastUpdated}></DateTimeFormat><br />
                <label>Open: </label>
                <span>{data.open}</span><br />
                <label>High: </label>
                <span>{data.high}</span><br />
                <label>Low: </label>
                <span>{data.low}</span><br />
                <label>Price: </label>
                <span>{data.price}</span><br />
                <label>Volume: </label>
                <span>{data.volume}</span><br />
                <label>Latest trading day: </label>
                <DateTimeFormat rawDateTimeFormat={data.latestTradingDay}></DateTimeFormat><br />
                <label>Previous close: </label>
                <span>{data.previousClose}</span><br />
                <label>Change: </label>
                <ColorPrice RawValue={data.change}></ColorPrice><br />
                <label>Change percent: </label>
                <ColorPrice RawValue={data.changePercent}></ColorPrice>
            </div>
        );
        setQuoteWaiting(<></>);      
        setEmptyQuote(<></>);
    }
    
    return(
        <div id="QuoteContainer" className="waitingParent dashboardContainer">
            <h2>Stock Quote</h2>
            {quoteDisplay}
            {emptyQuote}
            <div className="btn-group" role="group">
                <button type="button" className="btn btn-outline btn-lg btn-primary">Refresh</button>
            </div>
            {quoteWaiting}
        </div>
    );
}
export {StockQuote};
export default StockQuote;