import { useState, useEffect } from 'react';
import { User } from '../LoginForm';
import { getTradeHistory, resetTradeHistory } from '../Service/TradingApi';
import { useNavigate } from 'react-router-dom';
import WaitingDisplay from '../WaitingDisplay';
import TradeRecord, {TradeRecordData} from './TradeRecord'; 

type TradingHistoryProps = {
    User : User,
    SetErrorMsg : React.Dispatch<React.SetStateAction<string>>
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
        }).catch((error : Error) => {
            setIsWaiting(<></>);
            if (error.message.slice(3) === "401") {
                navigate("/login");
            }
            props.SetErrorMsg(error.message);
        });
    }

    const clearTradeHistory = async () => {
        setIsWaiting(<WaitingDisplay WaitingText={"Retreiving trade history from server..."}></WaitingDisplay>);
        await resetTradeHistory(props.User.id).then(() => {
            setTradeData([]);
            setIsWaiting(<></>);
        }).catch((error : Error) => {
            setIsWaiting(<></>);
            if (error.message.slice(3) === "401") {
                navigate("/login");
            }
            props.SetErrorMsg(error.message);
        });
    }

    const outList : JSX.Element[] = [];
    if (tradeData) {
        tradeData.forEach((record : TradeRecordData, index : number) => {
            outList.push(<TradeRecord key={record.id} Data={record}></TradeRecord>)
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
                    <button className='btn btn-danger' onClick={() => clearTradeHistory()}>Clear History</button>
                </div>
            </div>
            {isWaiting}
        </>
    )
}

export default TradeHistory;