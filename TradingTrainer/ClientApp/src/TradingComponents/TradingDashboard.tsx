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

type DashboardProps = {
    UserId? : number
    IsAuthenticated : boolean
    SetUser : React.Dispatch<React.SetStateAction<User>>
    SetIsAuthenticated : React.Dispatch<React.SetStateAction<boolean>>
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
    const [stockListWaiting, setStockListWaiting] = useState(<></>);
    const [reconnectingWaiting, setReconnectingWaiting] = useState(<></>)
    const [watchlist, setWatchlist] = useState<JSX.Element>(<></>);
    const [curSelectedStock, setCurSelectedStock] = useState<StockBase>();
    const [curStockQuote, setCurStockQuote] = useState<StockQuote>(initialQuote);
    // Action dialog related
    const [buyDialogIsActive, setBuyDialogIsActive] = useState(false);
    const [curActionStock, setCurActionStock] = useState<ActionStock>();
    const [curActionDialog, setCurActionDialog] = useState<JSX.Element>(<></>);

    const [emptyQuote, setEmptyQuote] = useState(<div id="EmptyQuote"><p>No quote data</p></div>);
    const [quoteWaiting, setQuoteWaiting] = useState(<></>);
    const [quoteDisplay, setQuoteDisplay] = useState(<></>);
    const [isFirstRender, setIsFirstRender] = useState(true);

    useEffect(() => {setIsFirstRender(false);}, []);

    useEffect(() => {

        if (!props.IsAuthenticated) {
            tryReconnect();
        } else {
            console.log("Updating watchlist")
            updateWatchList();
        }

        
    }, [props.IsAuthenticated]);

    useEffect(() => {
        // Check if it is the first render
        if (isFirstRender) {
            return;
        }

        if (buyDialogIsActive){
             // Create the ActionStock object
            if (curStockQuote === undefined) {
                alert("No stock is selected, please select a stock!");
                return;
            }
            const amount = 0;
            const curActionStock : ActionStock = {
                symbol : curStockQuote.symbol,
                stockName : curStockQuote.stockName,
                quantity : amount,
                price : curStockQuote.price
            }
            // set all relevant states
            setCurActionStock(curActionStock);
            // Show action dialog for buy operation
            setCurActionDialog(<ActionDialog SelectedStock={curActionStock}
                                            SetBuyDialogIsActive={setBuyDialogIsActive}
                                        ></ActionDialog>);
            
        } else {
            setCurActionDialog(<></>);
        }
        

        
    }, [buyDialogIsActive]);

    const buyStock = (stock : StockQuote) => {
        // Create the ActionStock object
        if (stock === undefined) {
            alert("No stock is selected, please select a stock!");
            return;
        }
        const amount = 0;
        const curActionStock : ActionStock = {
            symbol : stock.symbol,
            stockName : stock.stockName,
            quantity : amount,
            price : stock.price
        }

        // set all relevant states
        setCurActionStock(curActionStock);
        // Show action dialog for buy operation
        setCurActionDialog(<ActionDialog SelectedStock={curActionStock}
                                         SetBuyDialogIsActive={setBuyDialogIsActive}
                                    ></ActionDialog>);
        setBuyDialogIsActive(true); 
    }

    const updateWatchList = async () => {
        console.log(props.UserId);
        if (props.UserId === undefined || props.UserId <= 0) {
            navigate("/login");
            throw new Error(`The userid is not valid!`)
        }
        const requestUrl = `/trading/getFavoriteList?userId=${props.UserId}`;
        setStockListWaiting(<WaitingDisplay WaitingText={"Retreiving watchlist from server..."}></WaitingDisplay>)
        const curData = await fetchFromTradingApi(requestUrl);
        setWatchlist(   
            <Watchlist 
                SetBuyDialogIsActive={setBuyDialogIsActive}
                ContentData={curData} 
                RefreshCallback={updateWatchList} 
                SetCurSelectedStock={setCurSelectedStock}
                UpdateQuoteDisplay={updateQuoteDisplay}
            ></Watchlist>
        );
        setTimeout(()=>{
            setStockListWaiting(<></>);
        }, 1000);
    } 

    const fetchFromTradingApi = async (requestUrl : string) : Promise<any> => {
        return fetch(requestUrl).then((response) => {
            if (!response.ok) {
                // The server responded with an error
                const msg = `Error: The server responded with error code: ${response.status}\n \
                Message: ${response.text}`;
                throw new Error(msg);
            }
            return response.json();
        }).catch((errorResp) => {
            alert(errorResp.message);
            console.log(errorResp.message);
            setStockListWaiting(<></>)
        });
    }

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
                console.log("User has an active session. Redirecting to dashboard.")
                // Bypas login procedure
                props.SetUser(data.result);
                props.SetIsAuthenticated(true);
                navigate("/TradingDashboard");
            }
        }).catch((errorResp) => {
            console.log(errorResp.message);
            setReconnectingWaiting(<></>);
        });
    }

    const getStockQuote = async () : Promise<any> => {
        const quoteEndpointUrl = `/trading/GetStockQuote?symbol=${curSelectedStock?.symbol}`;
        return fetch(quoteEndpointUrl).then((response) => {
            if (!response.ok) {
                throw new Error(`Server responded with an error: ${response.status}`);
            }
            return response.json();
        }).catch((err) => {
            alert(err.Message);
        })
    }

    const updateQuoteDisplay = async (stock : StockBase | undefined) => {
        if (stock === undefined) {
            setEmptyQuote(<div id="EmptyQuote"><p>No quote data</p></div>);
            return;
        }
        // Get the quote from server
        setQuoteWaiting(<WaitingDisplay WaitingText={"Retreiving Quote from server..."}></WaitingDisplay>);
        const data : StockQuote = await fetchFromTradingApi(`/trading/GetStockQuote?symbol=${stock.symbol}`);
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
        setCurStockQuote(data);
        setQuoteWaiting(<></>);      
        setEmptyQuote(<></>);
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
                        <div id="WatchlistTab" className="navTab">Watchlist</div>
                        <div id="PortfolioTab" className="navTab">Portfolio</div>
                        <div id="HistoryTab" className="navTab">Trade history</div>
                    </div>
                    {watchlist}
                    {stockListWaiting}
                    {curActionDialog}
                </div>
                <div id="QuoteContainer" className="waitingParent dashboardContainer">
                    <h2>Stock Quote</h2>
                    {quoteDisplay}
                    {emptyQuote}
                    <div className="btn-group" role="group">
                        <button type="button" onClick={() => {updateQuoteDisplay(curSelectedStock);}} className="btn btn-outline btn-lg btn-primary">Refresh</button>
                    </div>
                    {quoteWaiting}
                </div>
                <div id="SearchContainer"></div>
            </main>
            <TradingTrainerFooter></TradingTrainerFooter>
            {reconnectingWaiting}
        </>
    );
}
export { StockQuote }
export default TradingDashboard;