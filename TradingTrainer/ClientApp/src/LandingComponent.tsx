import React, {ReactNode, Component} from 'react';
import NavLink from 'react-router-dom';

class LandingComponent extends Component {
    Render() : ReactNode {
        return(
            <div className="FeatureContainer">
                <h1>Trading Trainer</h1>
                <p>
                   Welcome to Trading Trainer. This is an app where you can simulate investment on real priced stocks.
                   As a user, you can buy and sell any stock from the real stock market. This is also known as Paper Trading.
                </p>
            </div>
            
        );
    } 
}

export default LandingComponent;