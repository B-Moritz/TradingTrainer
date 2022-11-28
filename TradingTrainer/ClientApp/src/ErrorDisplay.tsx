
type ErrorDisplayProps = {
    ErrorMsg : string,
    SetErrorMsg : React.Dispatch<React.SetStateAction<string>>
}

function ErrorDisplay(props : ErrorDisplayProps) : JSX.Element {
    return(
        <div id="ErrorContainer">
            <div id="ErrorDialog">
                <h1>The following error was encountered</h1>
                <p>{props.ErrorMsg}</p>
                <button className="btn btn-lg btn-primary" onClick={() => props.SetErrorMsg("")}>Ok</button>
            </div>
        </div>
    );
}

export default ErrorDisplay;