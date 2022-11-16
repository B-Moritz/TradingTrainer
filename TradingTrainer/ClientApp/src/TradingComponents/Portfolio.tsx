
import StockPortfolioRow from './StockPortfolioRow';
import {DateTimeFormat} from './DisplayUtilities'
import {useState, useEffect} from 'react';

type PortfolioResponse = {
    lastUpdate : string
    totalValueSpent : string
  // The estimated market value of the portfolio
    estPortfolioValue : string
  // The currency used to show the values
    portfolioCurrency : string
  // The portfolio stocks
    stocks : Array<PortfolioStock>
  // The total funds available to use by the user
    buyingPower : string
  // The unrealized profit & loss for the entire portfolio
    unrealizedPL : string
}

type PortfolioStock = {
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

    quantity : string
    // Estimated price of the stock. This value is value is from the
    // Quote endpo in the Alpha Vantage API.
    estPrice : string
    // The relative amount of the stock in the portfolio.
    portfolioPortion : string
    // Estimated total market value of the stock position owned.
    // Based on the estimated price value.
    estTotalMarketValue : string
    // The total funds spent on the stock.
    totalCost : string 
    // The difference between current market value and the total cost
    unrealizedPL : string
}

type PortfolioProps = {
        RefreshCallback : Function
        ContentData : PortfolioResponse 
        SetCurSelectedStock : React.Dispatch<React.SetStateAction<undefined | PortfolioStock>>
        SetBuyDialogIsActive : React.Dispatch<React.SetStateAction<boolean>>
        SetSellDialogIsActive : React.Dispatch<React.SetStateAction<boolean>>
        UpdateQuoteDisplay : (symbol : string) => Promise<any>
}

function Portfolio(props: PortfolioProps) : JSX.Element {

    const [outList, setOutList] = useState<JSX.Element[]>();
    const [emptyDisp, setEmptyDisp] = useState<JSX.Element>();

    useEffect(() => {
        initialList();
    }, [])

    const initialList = () => {
        let curOutList : JSX.Element[] = [];
        let isSelected = false;
        if (props.ContentData.stocks !== undefined) {
            props.ContentData.stocks.forEach((stock, index) => {
                if (index === 0) {
                    props.SetCurSelectedStock(stock);
                    isSelected = true;
                    props.UpdateQuoteDisplay(stock.symbol);
                }
                curOutList.push(<StockPortfolioRow
                                key={"favStock_" + stock.symbol} 
                                CurPortfolioStock={stock}
                                IsSelected={isSelected}
                                SelectStock={selectStock}
                            ></StockPortfolioRow>); 
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
    
    const selectStock = (selectedStock : PortfolioStock) => {
        let curOutList : JSX.Element[] = [];
        let isSelected = false;
        if (props.ContentData.stocks !== undefined) {
            props.ContentData.stocks.forEach((stock, index) => {
                if (stock.symbol === selectedStock.symbol) {
                    props.SetCurSelectedStock(stock);
                    isSelected = true;
                    props.UpdateQuoteDisplay(stock.symbol);
                }
                curOutList.push(<StockPortfolioRow 
                                key={"favStock_" + stock.symbol} 
                                CurPortfolioStock={stock} 
                                IsSelected={isSelected}
                                SelectStock={selectStock}
                            ></StockPortfolioRow>);
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
        <div id="PortfolioContainer" className="stockListContainer">
            <h2>Portfolio</h2>
                <div className="float-left">
                    <p>Last updated: <DateTimeFormat rawDateTimeFormat={props.ContentData.lastUpdate}></DateTimeFormat></p>
                    <p>{"Total value spent: " + props.ContentData.totalValueSpent}</p>
                    <p>{"Estimated portfolio value: " + props.ContentData.estPortfolioValue}</p>
                    <p>{"Buying power: " + props.ContentData.buyingPower}</p>
                    <p>{"Unrealized profit/loss: " + props.ContentData.unrealizedPL}</p>
                    <p>{"Currency: " + props.ContentData.portfolioCurrency}</p>
                </div>
            <p>Click on a stock in the following list to see details</p>
            
            <table className='table'>
                <thead>
                    <tr>
                        <th>Stock symbol</th >
                        <th>Stock Name</th>
                        <th>Quantity</th>
                        <th>Estimated price</th>
                        <th>Portfolio partition %</th>
                        <th>Estimated total value</th>
                        <th>Total cost</th>
                        <th>Unrealized profit/loss</th>
                    </tr>
                </thead>
                <tbody>
                    {outList}
                </tbody>
            </table>
            {emptyDisp}
            <div className="btn-group" role="group">
                <button className="btn btn-lg btn-primary" onClick={refresh}>Refresh</button>
                <button onClick={() => {props.SetBuyDialogIsActive(true);}} type="button" className="btn btn-outline btn-lg btn-success">Buy</button>
                <button onClick={() => {props.SetSellDialogIsActive(true);}} type="button" className="btn btn-outline btn-lg btn-danger">Sell</button>
            </div>
        </div>
    );
}
export {PortfolioStock};
export default Portfolio;