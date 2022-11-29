
import StockPortfolioRow from './StockPortfolioRow';
import {DateTimeFormat} from './DisplayUtilities'
import WaitingDisplay from '../WaitingDisplay';
import { getPortfolio } from '../Service/TradingApi';
import { User } from '../LoginForm';
import { StockBase } from './StockBaseRow';
import {useState, useEffect} from 'react';
import { useNavigate } from 'react-router-dom';
import { DashboardTabNames } from './TradingDashboard';
import { ColorPrice } from './DisplayUtilities';

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
        SetCurSelectedStock : React.Dispatch<React.SetStateAction<PortfolioStock | undefined>>
        SetStockListTab : React.Dispatch<React.SetStateAction<number[]>>
        SetErrorMsg : React.Dispatch<React.SetStateAction<string>>
        UpdateQuoteDisplay : (symbol : string) => Promise<any>
        User : User
        CurSelectedPortfolioStock : PortfolioStock | undefined
        ActiveTab : number
}

type OutList = {
    lastUpdated : string,
    totalValueSpent : string,
    estPortfolioValue : string,
    buyingPower : string,
    unrealizedPL : string,
    portfolioCurrency : string,
    elements : JSX.Element[]
}


function Portfolio(props: PortfolioProps) : JSX.Element {
    const navigate = useNavigate();
    let outList : OutList = {
        lastUpdated : "",
        totalValueSpent : "",
        estPortfolioValue : "",
        buyingPower : "",
        unrealizedPL : "",
        portfolioCurrency : "",
        elements : [<tr className="emptyTableDisp" key="EmptyPortfolioDisp"><td>Empty table</td></tr>]
    };

    const [portfolioData, setPortfolioData] = useState<PortfolioResponse>();
    const [isWaiting, setIsWaiting] = useState<JSX.Element>();

    useEffect(() => {
        if (props.ActiveTab === DashboardTabNames.PortfolioList) {
            refresh();
        }
        
    }, [props.User, props.ActiveTab])

    useEffect(() => {
        selectStock(undefined)
    }, [portfolioData]);

    const refresh = () => {
        setIsWaiting(<WaitingDisplay WaitingText={"Retreiving user portfolio from server..."}></WaitingDisplay>);
        getPortfolio(props.User.id).then((data : PortfolioResponse) => {
            setPortfolioData(data);
            setIsWaiting(<></>);
        }).catch((error : Error) => {
            if (error.message.slice(3) === "401") {
                navigate("/login");
            }
            props.SetErrorMsg(error.message);
        });
        //selectStock(props.CurSelectedPortfolioStock);
    }
    
    const selectStock = (selectedStock? : PortfolioStock) => {
        if (!selectedStock && portfolioData) {
            // No stocks are selected - set the first stock in the list as selected
            if (portfolioData.stocks.length > 0) {
                props.SetCurSelectedStock(portfolioData.stocks[0]);
                props.UpdateQuoteDisplay(portfolioData.stocks[0].symbol);
            } else {
                props.SetCurSelectedStock(undefined);
            }

        } else if (selectedStock) {
            props.SetCurSelectedStock(selectedStock);
            props.UpdateQuoteDisplay(selectedStock.symbol);
        }
    }

    const curOutList : JSX.Element[] = [];
    let isSelected = false;
    if (portfolioData !== undefined) {
        portfolioData.stocks.forEach((stock : any, index : number) => {
            if (props.CurSelectedPortfolioStock !== undefined 
                && stock.symbol === props.CurSelectedPortfolioStock.symbol) {
                isSelected = true;
            }
            curOutList.push(<StockPortfolioRow 
                            key={"favStock_" + stock.symbol}
                            Stock={stock}
                            IsSelected={isSelected}
                            SelectStock={selectStock}
                        ></StockPortfolioRow>);
            isSelected = false;
        });
        if (curOutList.length <= 0) {
            curOutList.push(<tr className="emptyTableDisp" key="EmptyPortfolioDisp"><td>Empty table</td></tr>);    
        }

        outList = {
            lastUpdated : portfolioData.lastUpdate,
            totalValueSpent : portfolioData.totalValueSpent,
            estPortfolioValue : portfolioData.estPortfolioValue,
            buyingPower : portfolioData.buyingPower,
            unrealizedPL : portfolioData.unrealizedPL,
            portfolioCurrency : portfolioData.portfolioCurrency,
            elements : curOutList
        };
        
    } else {
        // There is no portfolio result from server
        
    }

    return(
        <>
            <div id="PortfolioContainer" className="stockListContainer">
                <h2>Portfolio</h2>
                <div id="portfolioSummary" className="float-left">
                    <div className="portfolioSummaryBlock">
                        <div><span className='portfolioSummaryKey fw-bold'>Last updated: </span><span className='portfolioSummaryValue'><DateTimeFormat rawDateTimeFormat={outList.lastUpdated}></DateTimeFormat></span></div>
                        <div><span className='portfolioSummaryKey fw-bold'>Total value spent: </span><span className='portfolioSummaryValue'>{outList.totalValueSpent}</span></div>
                        <div><span className='portfolioSummaryKey fw-bold'>Estimated portfolio value: </span><span className='portfolioSummaryValue'>{outList.estPortfolioValue}</span></div>
                    </div>
                    <div className="portfolioSummaryBlock">
                        <div><span className='portfolioSummaryKey fw-bold'>Buying power </span><span className='portfolioSummaryValue'>{outList.buyingPower}</span></div>
                        <div><span className='portfolioSummaryKey fw-bold'>Unrealized profit/loss: </span><span className='portfolioSummaryValue'><ColorPrice RawValue={outList.unrealizedPL}></ColorPrice></span></div>
                        <div><span className='portfolioSummaryKey fw-bold'>Currency: </span><span className='portfolioSummaryValue'>{outList.portfolioCurrency}</span></div>
                    </div>
                </div>
                <p>Click on a stock in the following list to see details</p>
                <div id="PortfolioTable" className='stockListTable'>
                    {outList.elements}
                </div>
                <div className="btn-group" role="group">
                    <button className="btn btn-lg btn-primary" onClick={refresh}>Refresh</button>
                    <button onClick={() => {props.SetStockListTab([DashboardTabNames.BuyDisplay, DashboardTabNames.PortfolioList]);}} 
                            type="button" className="btn btn-outline btn-lg btn-success">Buy</button>
                    <button onClick={() => {props.SetStockListTab([DashboardTabNames.SellDisplay, DashboardTabNames.PortfolioList]);}} 
                            type="button" className="btn btn-outline btn-lg btn-danger">Sell</button>
                </div>
            </div>
            {isWaiting}
        </>
    );
}
export type { PortfolioStock };
export default Portfolio;