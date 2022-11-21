import { useState, useEffect } from 'react';
import { User } from '../LoginForm';
import { getTradeHistory } from '../Service/TradingApi';
import { useNavigate } from 'react-router-dom';
import WaitingDisplay from '../WaitingDisplay';
import TradeRecord, {TradeRecordData} from './TradeRecord'; 

type TradingHistoryProps = {
    User : User
}

function TradeHistory(props : TradingHistoryProps) {
    const navigate = useNavigate();
    const [tradeData, setTradeData] = useState<Array<TradeRecordData>>();
    const [isWaiting, setIsWaiting] = useState(<WaitingDisplay WaitingText={"Retreiving trade history from server..."}></WaitingDisplay>);
    
    useEffect(() => {refresh()}, []);

    const refresh = () => {
        setIsWaiting(<WaitingDisplay WaitingText={"Retreiving trade history from server..."}></WaitingDisplay>);
        getTradeHistory(props.User.id).then((data) => {
            setTradeData(data);
            setIsWaiting(<></>);
        }).catch((err) => {
            setIsWaiting(<></>);
            navigate("/login");
        });
    }

    const outList : JSX.Element[] = [];
    if (tradeData) {
        tradeData.forEach((record : TradeRecordData, index : number) => {
            outList.push(<TradeRecord Data={record}></TradeRecord>)
        });
    } else {
        outList.push(<tr><td>No history was found</td></tr>);
    }


    return(
        <>
            <div id="TradeHistoryContainer" className="stockListContainer">
                <h2>Trade history</h2>
                <div className='stockListTable'>
                    <table className='table'>
                        <thead>
                            <tr id='TradeHistoryTableHeader'>
                                <th>Symbol</th >
                                <th>Quantity</th>
                                <th>Transaction type</th>
                                <th>Saldo</th>               
                                <th>Timestamp</th>
                            </tr>
                        </thead>
                        <tbody>
                            {outList}
                        </tbody>
                    </table>
                </div>
                <div className="btn-group">
                    <button className="btn btn-primary" onClick={() => refresh()}>Refresh</button>
                    <button className='btn btn-danger'>Clear History</button>
                </div>
            </div>
            {isWaiting}
        </>
    )
}

export default TradeHistory;