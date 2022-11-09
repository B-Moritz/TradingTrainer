import React, {ReactNode, Component} from 'react';
import { Link } from 'react-router-dom';

class LandingComponent extends Component {
    render() : ReactNode {
        return(
            <div className="FeatureContainer">
                <article>
                    <p>
                        Welcome to Trading Trainer. This is an app where you can simulate investment on real priced stocks.
                        As a user, you can buy and sell any stock from the real stock market. This is also known as Paper Trading.
                    </p>
                </article>
                <nav className="landingNavigation">
                    <Link className="btn btn-lg btn-outline-primary" to="/login">Login</Link>
                    <Link className="btn btn-lg btn-outline-primary" to="/registerForm">Register</Link>
                </nav>

            </div>
        );
    } 
}

export default LandingComponent;