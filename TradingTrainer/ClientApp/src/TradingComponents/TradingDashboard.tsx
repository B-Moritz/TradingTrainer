import { useState, useEffect } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import TradingTrainerFooter from '../TradingTrainerFooter';
import Watchlist, {WatchlistResponse} from './Watchlist';
import WaitingDisplay from '../WaitingDisplay';
//import StockQuote from './StockQuote'; // Deprecated
import { StockBase } from './StockBaseRow';
import { User } from '../LoginForm';
import ActionDialog, { ActionStock } from './ActionDialog';
import { DateTimeFormat, ColorPrice } from './DisplayUtilities';
import { setUncaughtExceptionCaptureCallback } from 'process';
import Portfolio, {PortfolioStock} from './Portfolio';
import StockMarket from "./StockMarket";
import { getStockQuote } from '../Service/TradingApi';
import TradeHistory from './TradeHistory';

type DashboardProps = {
    User : User
    //IsAuthenticated : boolean
    SetUser : React.Dispatch<React.SetStateAction<User>>
    //SetIsAuthenticated : React.Dispatch<React.SetStateAction<boolean>>
}

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

class DashboardTabNames {
    static WatchList = 1;
    static PortfolioList = 2;
    static StockMarket = 3;
    static TradeHistory = 4;
    static BuyDisplay = 5;
    static SellDisplay = 6;
}

function TradingDashboard(props: DashboardProps) : JSX.Element {
    const navigate = useNavigate();
    const initialWatchlistResponse : WatchlistResponse = {
        lastUpdated : "",
        stockList : []
    };

    const initialStock : StockBase = {
        symbol : "",
        stockName : "",
        type : "",
        stockCurrency : "",
        lastUpdated : ""
    }

    const initialQuote : StockQuote = {
        symbol : "",
        // The stock name
        stockName : "",
        // The stock type (e.g equity)
        type : "",
        // The currency that the stock is traded in
        stockCurrency : "",
        // Object create timestamp
        lastUpdated : "",
        open : "",
        high : "",
        low : "",
        price : "",
        volume : 0,
        latestTradingDay : "",
        previousClose : "",
        change : "",
        changePercent : ""
    }
    const [reconnectingWaiting, setReconnectingWaiting] = useState(<></>)
    //const [stockList, setStockList] = useState<JSX.Element>(<></>);
    const [curSelectedStock, setCurSelectedStock] = useState<StockBase>(initialStock);
    const [curSelectedPortfolioStock, setCurSelectedPortfolioStock] = useState<PortfolioStock>();
    const [curStockQuote, setStockQuote] = useState<StockQuote>(initialQuote);

    const [userAvailable, setUserAvailable] = useState(false);
    // The firs value is the id of the active display. The second value is the id of the display that the program should return to
    const [stockListTab, setStockListTab] = useState<Array<number>>([DashboardTabNames.WatchList, DashboardTabNames.WatchList]);

    useEffect(() => {
        // If the site is reloaded and IsAuthenticated is set to false -> try reconnect without navigating back to login
        console.log("Current user id is: " + props.User.id);
        if (props.User.id === 0) {            
            tryReconnect();
        } else {
            setUserAvailable(true);
        }
    }, [props.User]);

    const createSellDialog = () : JSX.Element => {
        // Create the ActionStock object
        if (curSelectedPortfolioStock === undefined) {
            alert("No stock is selected, please select a stock!");
            setStockListTab([stockListTab[1], stockListTab[1]]);
            return(<></>);
        }
        const curActionStock : ActionStock = {
            symbol : curSelectedPortfolioStock.symbol,
            stockName : curSelectedPortfolioStock.stockName,
            quantity : curSelectedPortfolioStock.quantity,
            price : curSelectedPortfolioStock.estPrice
        }
        // set all relevant states
        // setCurActionStock(curActionStock);
        // Show action dialog for buy operation
        return(<ActionDialog SelectedStock={curActionStock}
                            SetStockListTab={setStockListTab}
                            CallbackTab={stockListTab[1]}
                            isBuyDialog={false}
                            UserId={props.User.id}
                        ></ActionDialog>);
        
    }
    

    const createBuyDialog = () : JSX.Element => {
        // Create the ActionStock object
        if (curStockQuote === undefined 
            || (curSelectedPortfolioStock === undefined && stockListTab[1] === 2) 
            || (curSelectedStock === undefined && stockListTab[1] === 1)) {
            alert("No stock is selected, please select a stock!");
            setStockListTab([stockListTab[1], stockListTab[1]]);
        }
        const curActionStock : ActionStock = {
            symbol : curStockQuote.symbol,
            stockName : curStockQuote.stockName,
            quantity : (stockListTab[1] === 2 && curSelectedPortfolioStock ? curSelectedPortfolioStock.quantity : "Unknown"),
            price : curStockQuote.price
        }
        // set all relevant states
        //setCurActionStock(curActionStock);
        // Show action dialog for buy operation
        return(<ActionDialog SelectedStock={curActionStock}
                            SetStockListTab={setStockListTab}
                            CallbackTab={stockListTab[1]}
                            isBuyDialog={true}
                            UserId={props.User.id}
                        ></ActionDialog>);
    }

    /*
    This function is used to check if the user already has an active session
    */
    const tryReconnect = async () => {
        setReconnectingWaiting(<WaitingDisplay WaitingText={"Reconnecting, please wait ...."}></WaitingDisplay>);
        await checkExistingSession();
        setReconnectingWaiting(<></>);
    }

    const checkExistingSession = async () => {
        // Check if the user has an active session
        // If the user has an active session -> redirect to application
        await fetch("/trading/getUsername").then((response) => {
            if (!response.ok) {
                if (response.status === 401) {
                    console.log("User needs authentication");
                    navigate('/login');
                }
                throw new Error(`The server responded with status code ${response.status}: ${response.text}`);
            }
            return response.json();
        }).then((data) => {
            if (data) {
                // The user has already an active session on the server
                // Bypas login procedure
                props.SetUser(data.result);
                //props.SetIsAuthenticated(true);
                //navigate("/TradingDashboard");
            }
        }).catch((errorResp) => {
            console.log(errorResp.message);
            setReconnectingWaiting(<></>);
        });
    }

    const updateQuoteDisplay = async (symbol : string | undefined) => {
        if (!symbol) {
            // No stock is selected
            return;
        }
        setStockQuote(initialQuote);
        // Get the quote from server
        getStockQuote(symbol).then((data : StockQuote) => {
            setStockQuote(data);
        }).catch((error) => {
            navigate("/login");
        });
    }
    
    let stockList;
    switch(stockListTab[0]) {
        case DashboardTabNames.StockMarket:
            stockList = <StockMarket User={props.User}></StockMarket>
            break;
        case DashboardTabNames.PortfolioList:
            stockList = <Portfolio 
                SetStockListTab={setStockListTab}
                UpdateQuoteDisplay={updateQuoteDisplay}
                CurSelectedPortfolioStock={curSelectedPortfolioStock}
                SetCurSelectedStock={setCurSelectedPortfolioStock}
                User={props.User}
                ActiveTab={stockListTab[0]}
            ></Portfolio>
            break;
        case DashboardTabNames.TradeHistory:
            stockList = <TradeHistory User={props.User}></TradeHistory>
            break;
        case DashboardTabNames.BuyDisplay:
            stockList = createBuyDialog();
            break;
        case DashboardTabNames.SellDisplay:
            stockList = createSellDialog();
            break;
        default:
            stockList = <Watchlist 
                            SetStockListTab={setStockListTab}
                            SetCurSelectedStock={setCurSelectedStock}
                            // Sending dow callback to update the quote - used by the watchlist row component
                            UpdateQuoteDisplay={updateQuoteDisplay}
                            CurSelectedStock={curSelectedStock}
                            User={props.User}
                            ActiveTab={stockListTab[0]}
                        ></Watchlist>      
    }
    let stockQuote
    let stockQuoteWaiting;
    if (curStockQuote.symbol !== "") {
        stockQuote = <div>
                        <label>Symbol: </label>
                        <span>{curStockQuote.symbol}</span><br />
                        <label>Name: </label>
                        <span>{curStockQuote.stockName}</span><br />
                        <label>Last updated: </label>
                        <DateTimeFormat rawDateTimeFormat={curStockQuote.lastUpdated}></DateTimeFormat><br />
                        <label>Open: </label>
                        <span>{curStockQuote.open}</span><br />
                        <label>High: </label>
                        <span>{curStockQuote.high}</span><br />
                        <label>Low: </label>
                        <span>{curStockQuote.low}</span><br />
                        <label>Price: </label>
                        <span>{curStockQuote.price}</span><br />
                        <label>Volume: </label>
                        <span>{curStockQuote.volume}</span><br />
                        <label>Latest trading day: </label>
                        <DateTimeFormat rawDateTimeFormat={curStockQuote.latestTradingDay}></DateTimeFormat><br />
                        <label>Previous close: </label>
                        <span>{curStockQuote.previousClose}</span><br />
                        <label>Change: </label>
                        <ColorPrice RawValue={curStockQuote.change}></ColorPrice><br />
                        <label>Change percent: </label>
                        <ColorPrice RawValue={curStockQuote.changePercent}></ColorPrice>
                    </div>
        stockQuoteWaiting = <></>;
    } else {
        stockQuote = <div id="EmptyQuote"><p>No quote data</p></div>;
        if (curSelectedPortfolioStock || curSelectedStock.symbol !== "") {
            stockQuoteWaiting = <WaitingDisplay WaitingText={"Retreiving Quote from server..."}></WaitingDisplay>;    
        } else {
            stockQuoteWaiting = <></>;
        }
    }

    return (
        <>
            <header>
                <h1>Trading Dashboard</h1>  
            </header>
            <nav>

            </nav>
            <main id="TradingDashboardContainer">
                <div id="StockListContainer" className="waitingParent dashboardContainer">
                    <div id="StockListNavigation">
                        <div id="WatchlistTab" 
                             className={"navTab " + (stockListTab[1] === DashboardTabNames.WatchList ? "selectedTab" : "")} 
                             onClick={() => setStockListTab([DashboardTabNames.WatchList, DashboardTabNames.WatchList])}>Watchlist</div>
                        <div id="PortfolioTab" 
                             className={"navTab " + (stockListTab[1] === DashboardTabNames.PortfolioList ? "selectedTab" : "")} 
                             onClick={() => setStockListTab([DashboardTabNames.PortfolioList, DashboardTabNames.PortfolioList])}>Portfolio</div>
                        <div id="StockMarketTab" 
                             className={"navTab " + (stockListTab[1] === DashboardTabNames.StockMarket ? "selectedTab" : "")} 
                             onClick={() => setStockListTab([DashboardTabNames.StockMarket, DashboardTabNames.StockMarket])}>Stock Market</div>
                        <div id="HistoryTab" 
                             className={"navTab " + (stockListTab[1] === DashboardTabNames.TradeHistory ? "selectedTab" : "")} 
                             onClick={() => setStockListTab([DashboardTabNames.TradeHistory, DashboardTabNames.TradeHistory])}>Trade history</div>
                    </div>
                    {stockList}
                </div>
                <div id="QuoteContainer" className="waitingParent dashboardContainer">
                    <h2>Stock Quote</h2>
                    {stockQuote}
                    <div className="btn-group" role="group">
                        <button type="button" onClick={() => {
                            if (stockListTab[0] === 1) {
                                updateQuoteDisplay(curSelectedStock?.symbol);
                            } else {
                                updateQuoteDisplay(curSelectedPortfolioStock?.symbol);
                            }}} className="btn btn-outline btn-lg btn-primary">Refresh</button>
                    </div>
                    {stockQuoteWaiting}
                </div>
                <div id="SearchContainer"></div>
            </main>
            <TradingTrainerFooter></TradingTrainerFooter>
            {reconnectingWaiting}
        </>
    );
}
export { StockQuote, DashboardTabNames }
export default TradingDashboard;