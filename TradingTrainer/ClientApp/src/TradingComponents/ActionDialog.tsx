import { StockQuote } from './StockQuote';

type ActionStock = {
    symbol : string,
    stockName : string,
    quantity : number,
    price : number
}

type ActionDialogProps = {
    SelectedStock : ActionStock
}

function ActionDialog(props : ActionDialogProps) : JSX.Element {
    
    return(
        <div id="ActionDialogContainer">
            <h3>Buy stock</h3>
            <label style={{gridArea: 'label1'}}>Stock Symbol:</label>
            <p style={{gridArea: 'static1'}}>${props.SelectedStock.symbol}</p>
            <label style={{gridArea: 'label2'}}>Stock Name:</label>
            <p style={{gridArea: 'static2'}}>${props.SelectedStock.stockName}</p>
            <label style={{gridArea: 'label3'}}>Number of shares in portfolio:</label>
            <p style={{gridArea: 'static3'}}>${props.SelectedStock.quantity}</p>
            <label style={{gridArea: 'label4'}}>Estimated price per share:</label>
            <p style={{gridArea: 'static4'}}>${props.SelectedStock.price}</p>;
            <div style={{gridArea: 'inputGroup1'}} className="input-group">
                <label className="input-group-addon">Number of shares</label>
                <input id="StockCounterInput" className="form-control" type="number" name="StockCounter" value="1" />
            </div>
            <div id="DialogErrorMsg" style={{gridArea: 'errorMsg'}}></div>
            <div className="btn-group">
                <button className="btn btn-lg btn-success">Confirm Transaction</button>
                <button className="btn btn-lg btn-secondary">Cancle</button>
            </div>
        </div>
    );
}