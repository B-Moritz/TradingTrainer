import React, { ReactNode, Component } from 'react';
import { Link } from 'react-router-dom';

type LoginState = {
    usernameInput : string,
    pwdInput : string
}

type LoginProp = {

}

class LoginForm extends Component<LoginProp, LoginState> {

    constructor(props : LoginProp) {
        super(props);
        this.validateUsername = this.validateUsername.bind(this);
    }

    state : LoginState = {
        usernameInput : "",
        pwdInput : ""
    };

    validateUsername(e : React.FormEvent<HTMLInputElement>) : void {
         const unamePattern : RegExp = /^[a-zA-Z\#\!\%\$\‘\&\+\*\–\/\=\?\^\_\`\.\{\|\}\~]+@[a-zA-Z0-9\-\.]{1,63}$/;
         const username : string = e.currentTarget.value;
         console.log(username);
         if (unamePattern.test(username)) {
            // The username is valid
            console.log("The provided username was valid.");
            this.setState({
                    usernameInput : "is-valid"
            });

            return;
         }
         // The user name is not valid
         console.log("The provided username was not valid.");
         this.setState((prop, state) => {
            return {usernameInput : "is-invalid"};
         });
    }

    render(): ReactNode {

        const usernameInputClasses = "form-control" + " " + this.state.usernameInput;

        return (
            <>
                <h2>Login to your Trading Trainer account</h2>
                <form>
                    <div className="form-floating mb-3 mt-3">
                        <input id="usernameInput" onChange={this.validateUsername} className={usernameInputClasses} type="text" placeholder="Enter Username" />
                        <label htmlFor="usernameInput">Username</label>
                        <div>
                            <p className={
                                "text-danger" + " " + 
                                (this.state.usernameInput === "is-invalid" ? "d-block" : "d-none")}
                                >The username is invalid.</p>
                        </div>
                    </div>
                    <div className="form-floating mb-3 mt-3">
                        <input id="PasswordInput" className="form-control" type="password" placeholder="Enter Password" />
                        <label htmlFor="PasswordInput">Password</label>
                        <div className="invalid-feedback"></div>
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