import { DateTimeFormat } from './DisplayUtilities';


type TradeRecordData = {
    id : string
    stockSymbol : string
    date : string
    userId : string
  // legger noen atributer
    transactionType : string
    stockCount : string
    saldo : string
}

type TradeRecordProps = {
    Data : TradeRecordData
}

function TradeRecord(props : TradeRecordProps) : JSX.Element {
    return(
        <tr>
            <td>{props.Data.stockSymbol}</td>
            <td>{props.Data.stockCount}</td>
            <td>{props.Data.transactionType}</td>
            <td>{props.Data.saldo}</td>
            <td><DateTimeFormat rawDateTimeFormat={props.Data.date}></DateTimeFormat></td>
        </tr>
    );
}

export { TradeRecordData }
export default TradeRecord;