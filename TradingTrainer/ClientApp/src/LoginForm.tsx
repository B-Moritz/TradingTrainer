import React, { useState, ReactNode } from 'react';
import { Link, useNavigate } from 'react-router-dom';

type LoginProps = {}

function LoginForm(props:LoginProps) : JSX.Element {

    const [usernameInput, setUsernameInput] = useState(true);
    const [pwdInput, setPwdInput] = useState(true);
    const [usr, setUsr] = useState("");
    const [pwd, setPwd] = useState("");
    const [isWaiting, setIsWaiting] = useState(false);
    const [authFailed, setAuthFailed] = useState(false);
    const [firstRender, setFirstRender] = useState(true);

    const navigate = useNavigate();
    
    const validateUsername = (e : React.FormEvent<HTMLInputElement>) : void => {
        if (firstRender) {
            setFirstRender(false);
        }
         const unamePattern : RegExp = /^[a-zA-Z\#\!\%\$\‘\&\+\*\–\/\=\?\^\_\`\.\{\|\}\~]+@[a-zA-Z0-9\-\.]{1,63}$/;
         const username : string = e.currentTarget.value;
         if (unamePattern.test(username)) {
            // The username is valid
            //console.log("The provided username was valid.");
            setUsernameInput(true);
            setUsr(username);

            return;
         }
         // The user name is not valid
         //console.log("The provided username was not valid.");
         setUsernameInput(false);
         setUsr(username);
    }

    const validatePwd = (e : React.FormEvent<HTMLInputElement>) : void => {
        const curPwd = e.currentTarget.value;

        setPwd(curPwd);
    }

    const initiateLogin = async () : Promise<void> => {
        setIsWaiting(true);

        await loginCall(usr, pwd);
        // Display the spinner
        resetLogin();
    }

    const resetLogin = () : void => {

        setUsernameInput(true);
        setPwdInput(true);
        setUsr("");
        setPwd("");
    }

    // https://developer.mozilla.org/en-US/docs/Web/API/Fetch_API/Using_Fetch
    // This method executes the call to /trading/login through the fetch api.
    const loginCall = async (usr: string, pwd: string) : Promise<void> => {
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
                    if (resp.status == 401) {
                        setAuthFailed(true);
                    }
                    throw new Error(msg);
                }
                console.log("Logon successful!");
                if (authFailed) {
                    setAuthFailed(false);
                }
                return resp.json;
            }
        ).then((data) => {
            // Handle the data promise
            setTimeout(() => {
                setIsWaiting(false);
                console.log("Redirecting to dashboard.")
                navigate("/TradingDashboard");
            }, 2000)
        }).catch(errorResp => {
            console.log(errorResp.message);
            setTimeout(() => {
                setIsWaiting(false);
                console.log("Authentication failed");
            }, 2000);
        });
    }

    const submit = (event : React.KeyboardEvent) : any => {
        if (event.key === "Enter") {
            initiateLogin();
        }
    }

    return (
        <>
            <div className={(isWaiting ? "d-none" : "d-block")}>
                <h2>Log In to your Trading Trainer account</h2>
                <form>
                    <div className="form-floating mb-3 mt-3">
                        <input id="usernameInput" value={usr} 
                               onChange={validateUsername} 
                               className={"form-control" + " " + (firstRender ? "" : (usernameInput ? "is-valid" : "is-invalid"))} 
                               type="text" placeholder="Enter Username" 
                               onKeyDown={submit}
                               />
                        <label htmlFor="usernameInput">Username</label>
                        <div>
                            <p className={
                                "text-danger" + " " + 
                                (usernameInput ? "d-none" : "d-block")}
                                >The username is invalid.</p>
                        </div>
                    </div>
                    <div className="form-floating mb-3 mt-3">
                        <input id="PasswordInput" value={pwd} 
                               onChange={validatePwd} 
                               className="form-control" 
                               type="password" 
                               placeholder="Enter Password"
                               onKeyDown={submit}
                                />
                        <label htmlFor="PasswordInput">Password</label>
                        <div className="invalid-feedback"></div>
                    </div>
                    <div>
                        <p className={"text-danger" + " " + (authFailed ? "d-block" : "d-none")}>Wrong username or password. Contact the administrator if you need to reset your passsword!</p>

                    </div>
                </form>
                <nav className="landingNavigation">
                    <Link className="btn btn-lg btn-outline-secondary" to="/">Cancel login</Link>
                    <button 
                        className={"btn btn-lg btn-outline-primary" + (usernameInput && pwdInput ? "" : "disabled")} 
                        onClick={(usernameInput && pwdInput ? initiateLogin : () => {})}
                        >Login</button>
                </nav>
            </div>
            <div className={"WaitingContainer " + (isWaiting ? "d-block" : "d-none")}>
                <p>Please wait ....</p>
                <div className="spinner-border text-info"></div>
            </div>
        </>
    );
}

export default LoginForm;