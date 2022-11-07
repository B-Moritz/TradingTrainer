import React, { ReactNode, Component } from 'react';
import { Link } from 'react-router-dom';

class LoginForm extends Component {
    render() : ReactNode {
        return (
            <>
                <h2>Login to your Trading Trainer account</h2>
                <form>
                    <div className="form-floating mb-3 mt-3">
                        <input id="usernameInput" className="form-control" type="text" placeholder="Enter Username" />
                        <label htmlFor="usernameInput">Username</label>
                    </div>
                    <div className="form-floating mb-3 mt-3">
                        <input id="PasswordInput" className="form-control" type="password" placeholder="Enter Password" />
                        <label htmlFor="PasswordInput">Password</label>
                    </div>
                </form>
                <nav className="landingNavigation">
                    <Link className="btn btn-lg btn-outline-secondary" to="/">Cancel login</Link>
                    <Link className="btn btn-lg btn-outline-primary" to="/TradingDashboard">Login</Link>
                </nav>

            </>
        );
    }

}

export default LoginForm;