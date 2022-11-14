import { useState, useEffect } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import TradingTrainerFooter from '../TradingTrainerFooter';
import Watchlist, {WatchlistResponse} from './Watchlist';
import WaitingDisplay from '../WaitingDisplay';
import StockQuote from './StockQuote';
import { StockBase } from './StockBaseRow';
import { User } from '../LoginForm';

type DashboardProps = {
    UserId? : number
    IsAuthenticated : boolean
    SetUser : React.Dispatch<React.SetStateAction<User>>
    SetIsAuthenticated : React.Dispatch<React.SetStateAction<boolean>>
}

function TradingDashboard(props: DashboardProps) : JSX.Element {
    const navigate = useNavigate();
    const initialWatchlistResponse : WatchlistResponse = {
        lastUpdated : "",
        stockList : []
    };
    const [stockListWaiting, setStockListWaiting] = useState(<></>);
    const [reconnectingWaiting, setReconnectingWaiting] = useState(<></>)
    const [watchlist, setWatchlist] = useState<JSX.Element>(<></>);
    const [curSelectedStock, setCurSelectedStock] = useState<StockBase>();


    useEffect(() => {

        if (!props.IsAuthenticated) {
            tryReconnect();
        } else {
            console.log("Updating watchlist")
            updateWatchList();
        }

        
    }, [props.IsAuthenticated]);

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
                ContentData={curData} 
                RefreshCallback={updateWatchList} 
                SetCurSelectedStock={setCurSelectedStock}
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

    const buyStock = () => {
        
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
                </div>
                <StockQuote CurStock={curSelectedStock}></StockQuote>
                <div id="SearchContainer"></div>
            </main>
            <TradingTrainerFooter></TradingTrainerFooter>
            {reconnectingWaiting}
        </>
    );
}

export default TradingDashboard;