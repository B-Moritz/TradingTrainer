import {useState} from 'react';
import { StockQuote } from './TradingDashboard';

type ActionStock = {
    symbol : string,
    stockName : string,
    quantity : number,
    price : string
}

type ActionDialogProps = {
    SelectedStock : ActionStock
    SetBuyDialogIsActive : React.Dispatch<React.SetStateAction<boolean>>
}

// remember "is-valid" : "is-invalid" is used to indicate input error
function ActionDialog(props : ActionDialogProps) : JSX.Element {    
    const [isValidAmount, setIsValidAmount] = useState("");
    const [confirmIsActive, setConfirmIsActive ] = useState(false);
    const [curAmount, setCurAmount] = useState(1);
    const [dispError, setDispError] = useState(false);

    const validateAmount = (event : React.FormEvent<HTMLInputElement>) => {
        const curValue = parseInt(event.currentTarget.value);
        setCurAmount(curValue);
        if (curValue < 1 || curValue % 1 !== 0) {
            // Input is not valid
            setIsValidAmount("is-invalid");
            setConfirmIsActive(true);
            setDispError(true)
        } else {
            setIsValidAmount("is-valid");
            setConfirmIsActive(false);
            setDispError(false);
        }
    }

    return(
        <div id="ActionDialogContainer">
            <h3 style={{gridArea: 'title'}}>Buy stock</h3>
            <label style={{gridArea: 'label1'}}>Stock Symbol:</label>
            <p style={{gridArea: 'static1'}}>{props.SelectedStock.symbol}</p>
            <label style={{gridArea: 'label2'}}>Stock Name:</label>
            <p style={{gridArea: 'static2'}}>{props.SelectedStock.stockName}</p>
            <label style={{gridArea: 'label3'}}>Number of shares in portfolio:</label>
            <p style={{gridArea: 'static3'}}>{props.SelectedStock.quantity}</p>
            <label style={{gridArea: 'label4'}}>Estimated price per share:</label>
            <p style={{gridArea: 'static4'}}>{props.SelectedStock.price}</p>
            <div style={{gridArea: 'inputGroup1'}} className="input-group input-group-lg">
                <label htmlFor='StockCounterInput' className='input-group-text'>Number of shares</label>
                <input id="StockCounterInput" 
                       onChange={validateAmount} 
                       className={"form-control " + isValidAmount} 
                       type="number" name="StockCounter" 
                       placeholder='Enter number of shares' 
                       value={curAmount}
                    />
            </div>
            <div id="DialogErrorMsg" style={{gridArea: 'errorMsg', color: 'red'}} className={(dispError ? "d-block" : "d-none")}>Please enter an integer greater than 0!</div>
            <div style={{gridArea: 'group'}} className="btn-group">
                <button className={"btn btn-lg btn-success " + (confirmIsActive ? "disabled" : "")}
                        aria-disabled={!confirmIsActive}
                        >Confirm Transaction</button>
                <button className={"btn btn-lg btn-secondary"} 
                        onClick={() => {props.SetBuyDialogIsActive(false);}}
                        >Cancle</button>
                        
            </div>
        </div>
    );
}
export {ActionStock};
export default ActionDialog;