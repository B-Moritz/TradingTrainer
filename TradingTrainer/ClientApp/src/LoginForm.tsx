import React, { ReactNode, Component } from 'react';
import { Link } from 'react-router-dom';
import { serialize } from 'v8';

type LoginState = {
    usernameInput : string,
    pwdInput : string,
    usr : string,
    pwd : string
}

type LoginProp = {

}

class LoginForm extends Component<LoginProp, LoginState> {

    constructor(props : LoginProp) {
        super(props);
        this.validateUsername = this.validateUsername.bind(this);
        this.validatePwd = this.validatePwd.bind(this);
        this.initiateLogin = this.initiateLogin.bind(this);
    }

    state : LoginState = {
        usernameInput : "",
        pwdInput : "",
        usr : "",
        pwd : ""
    };

    validateUsername(e : React.FormEvent<HTMLInputElement>) : void {
         const unamePattern : RegExp = /^[a-zA-Z\#\!\%\$\‘\&\+\*\–\/\=\?\^\_\`\.\{\|\}\~]+@[a-zA-Z0-9\-\.]{1,63}$/;
         const username : string = e.currentTarget.value;
         if (unamePattern.test(username)) {
            // The username is valid
            //console.log("The provided username was valid.");
            this.setState({
                    usernameInput : "is-valid",
                    usr : username
            });

            return;
         }
         // The user name is not valid
         //console.log("The provided username was not valid.");
         this.setState((prop, state) => {
            return {usernameInput : "is-invalid"};
         });
    }

    validatePwd(e : React.FormEvent<HTMLInputElement>) : void {
        const curPwd = e.currentTarget.value;
        this.setState({
            pwd : curPwd
        });
    }

    async initiateLogin() {
        
        await this.loginCall(this.state.usr, this.state.pwd);
        
        this.setState({
            usernameInput : "",
            pwdInput : "",
            usr : "",
            pwd : ""
        });
    }

    // https://developer.mozilla.org/en-US/docs/Web/API/Fetch_API/Using_Fetch
    async loginCall(usr: string, pwd: string) {
        const endpoint = "/trading/login";
        const credentials = {
            username: usr,
            password: pwd
        };
        await fetch(endpoint, {
            method: "POST",
            headers: {
                'Accept': 'application/json',
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(credentials)
        }).then(
            (resp) => {
                if (!resp.ok) {
                    // The server responded with an error
                    const msg = `Error: The server responded with error code: ${resp.status}\n \
                                 Message: ${resp.text}`;
                    throw new Error(msg);
                }
                return resp.json;
            }
        ).then((data) => {
            // Handle the data promise
            console.log(data);
        }).catch(errorResp => {
            console.log(`${errorResp.state}: ${errorResp.Message}`);
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
                        <input id="PasswordInput" onChange={this.validatePwd} className="form-control" type="password" placeholder="Enter Password" />
                        <label htmlFor="PasswordInput">Password</label>
                        <div className="invalid-feedback"></div>
                    </div>
                </form>
                <nav className="landingNavigation">
                    <Link className="btn btn-lg btn-outline-secondary" to="/">Cancel login</Link>
                    <Link className="btn btn-lg btn-outline-primary" onClick={this.initiateLogin} to="/TradingDashboard">Login</Link>
                </nav>

            </>
        );
    }

}

export default LoginForm;