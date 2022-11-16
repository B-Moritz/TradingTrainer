import React from 'react';
import { JsxElement } from 'typescript';

type WaitingProps = {
    WaitingText : string,
}

function WaitingDisplay(props: WaitingProps) : JSX.Element {
    return(
        <div className={"waitingContainer "}>
            <div className="spinner-border text-black"></div>
            <p>{props.WaitingText}</p>
        </div>
    );
}

export default WaitingDisplay;