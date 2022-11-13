import { useState, useEffect } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import TradingTrainerFooter from '../TradingTrainerFooter';
import Watchlist, {WatchlistResponse} from './Watchlist';
import WaitingDisplay from '../WaitingDisplay';
import StockQuote from './StockQuote';
import { StockBase } from './StockBaseRow';

type DashboardProps = {
    UserId? : number
    IsAuthenticated : boolean
}

function TradingDashboard(props: DashboardProps) : JSX.Element {
    const navigate = useNavigate();
    const initialWatchlistResponse : WatchlistResponse = {
        lastUpdated : "",
        stockList : []
    };
    const [stockListWaiting, setStockListWaiting] = useState(<></>);
    const [watchlistData, setWatchlistData] = useState(initialWatchlistResponse);
    const [curSelectedStock, setCurSelectedStock] = useState<StockBase>();

    useEffect(() => {
        if (props.IsAuthenticated) {
            updateWatchList();
        }
    },[]);

    const updateWatchList = async () => {
        console.log(props.UserId);
        if (props.UserId === undefined || props.UserId <= 0) {
            navigate("/login");
            throw new Error(`The userid is not valid!`)
        }
        const requestUrl = `/trading/getFavoriteList?userId=${props.UserId}`;
        setStockListWaiting(<WaitingDisplay WaitingText={"Retreiving watchlist from server..."}></WaitingDisplay>)
        setWatchlistData(await fetchFromTradingApi(requestUrl));
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

    return (
        <>
            <header>
                <h1>Trading Dashboard</h1>  
            </header>
            <nav>

            </nav>
            <main id="TradingDashboardContainer">
                <div id="StockListContainer" className="waitingParent">
                    <div id="StockListNavigation">
                        <div id="WatchlistTab" className="navTab">Watchlist</div>
                        <div id="PortfolioTab" className="navTab">Portfolio</div>
                        <div id="HistoryTab" className="navTab">Trade history</div>
                    </div>
                    <Watchlist 
                        ContentData={watchlistData} 
                        RefreshCallback={updateWatchList} 
                        SetCurSelectedStock={setCurSelectedStock}
                    ></Watchlist>
                    {stockListWaiting}
                </div>
                <StockQuote CurStock={curSelectedStock}></StockQuote>
                <div id="SearchContainer"></div>
            </main>
            <TradingTrainerFooter></TradingTrainerFooter>
        </>
    );
}

export default TradingDashboard;