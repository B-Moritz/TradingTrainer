import { Component, ReactNode } from 'react';
import { Outlet } from 'react-router-dom';

class PrimaryContainer extends Component {

    render(): ReactNode {
        return (
            <>
                <div id="SiteContainer">
                    <main id="FormContainer">
                        <header className="row">
                            <img className="tradingTrainerIcon col-2" src="./icons8-trading-parzival’-1997-outline-color-96.png" alt="Trading Trainer Logo" />
                            <h1 className="tradingTrainerTitle display-1 col-9">Trading Trainer</h1>
                        </header>
                        <Outlet />
                    </main>
                </div>
                <footer>
                    <ul>
                        <li>
                            <a target="_blank" href="https://icons8.com/icon/DnhznTvrGRgU/trading">Trading</a> icon by <a target="_blank" href="https://icons8.com">Icons8</a>
                        </li>
                    </ul>
                </footer>
            </>
        );
    }
}

export default PrimaryContainer;