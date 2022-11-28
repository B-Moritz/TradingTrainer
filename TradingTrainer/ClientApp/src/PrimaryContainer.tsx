import { Component, ReactNode } from 'react';
import { Outlet } from 'react-router-dom';
import logo from './img/icons8-track-and-field-96.png';
import TradingTrainerFooter from './TradingTrainerFooter';

class PrimaryContainer extends Component {

    render(): ReactNode {
        return (
            <>
                <div id="SiteContainer">
                    <main id="FormContainer">
                        <header id="PrimaryTitle" className="row">
                            <img className="tradingTrainerIcon col-2" src={logo} alt="Trading Trainer Logo" />
                            <h1 className="tradingTrainerTitle display-1 col-9">Trading Trainer</h1>
                        </header>
                        <Outlet />
                    </main>
                </div>
                <TradingTrainerFooter></TradingTrainerFooter>
            </>
        );
    }
}

export default PrimaryContainer;