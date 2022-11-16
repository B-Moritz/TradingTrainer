import React, { useState, useEffect } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import WaitingDisplay from './WaitingDisplay';


type User = {
    id? : number
    firstName? : string
    lastName? : string
    email? : string
    // The buying power of the user
    fundsAvailable? : string
    // The total amount of funds that have been invested by the user since the last reset.
    fundsSpent? : string
    currency? : string
} 

type LoginProps = {
    SetUser : React.Dispatch<React.SetStateAction<User>>
    SetIsAuthenticated : React.Dispatch<React.SetStateAction<boolean>>
}

function LoginForm(props:LoginProps) : JSX.Element {

    const [usernameInput, setUsernameInput] = useState(true);
    const [pwdInput, setPwdInput] = useState(true);
    const [usr, setUsr] = useState("");
    const [pwd, setPwd] = useState("");
    // Waiting to determine if the user already has an active session

    const [firstRender, setFirstRender] = useState(true);
    const waitDelay = 1000;
    const navigate = useNavigate();
    const [loginWaitingDisplay, setLoginWaitingDisplay] = useState(<WaitingDisplay WaitingText={"Checking for active sessions."}></WaitingDisplay>);
    const [authFailed, setAuthFailed] = useState(false);
    
    useEffect(() => {
        // This function is runs after the first rendering of the component
        // https://reactjs.org/docs/hooks-effect.html#tip-optimizing-performance-by-skipping-effects
        checkExistingSession();
    }, [])



    const initiateLogin = async () : Promise<void> => {
        setLoginWaitingDisplay(<WaitingDisplay WaitingText={"Atempting to authenticate. Please wait ......"}></WaitingDisplay>);

        await loginCall(usr, pwd);
        // Display the spinner
        resetLogin();
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
                    //  If the authentication failed state is active, make sure to disable it.
                    setAuthFailed(false);
                }
                return resp.json();
            }
        ).then((data) => {
            setTimeout(() => {
                setLoginWaitingDisplay(<></>);
                console.log("Redirecting to dashboard.");
                props.SetUser(data.result);
                props.SetIsAuthenticated(true);
                navigate("/TradingDashboard");
            }, waitDelay)
        }).catch(errorResp => {
            alert(errorResp.message);
            setTimeout(() => {
                setLoginWaitingDisplay(<></>);
                console.log("Authentication failed");
            }, waitDelay);
        });
    }

    const checkExistingSession = () => {
        // Check if the user has an active session
        // If the user has an active session -> redirect to application
        fetch("/trading/getUsername").then((response) => {
            if (!response.ok) {
                if (response.status === 401) {
                    console.log("User needs authentication");
                    return;
                }
                throw new Error(`The server responded with status code ${response.status}: ${response.text}`);
            }
            return response.json();
        }).then((data) => {
            setTimeout(() => {
                setLoginWaitingDisplay(<></>);
                if (data) {
                    // The user has already an active session on the server
                    console.log("User has an active session. Redirecting to dashboard.")
                    // Bypas login procedure
                    props.SetUser(data.result);
                    props.SetIsAuthenticated(true);
                    navigate("/TradingDashboard");
                }
            }, waitDelay);
        }).catch((errorResp) => {
            console.log(errorResp.message);
            setTimeout(() => {
                setLoginWaitingDisplay(<></>);
            }, waitDelay);
        });
    }

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

    const resetLogin = () : void => {
        setUsernameInput(true);
        setPwdInput(true);
        setUsr("");
        setPwd("");
    }


    const submit = (event : React.KeyboardEvent) : any => {
        if (event.key === "Enter") {
            initiateLogin();
        }
    }

    return (
        <>
            <div id="LoginContainer" className="waitingParent">
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
                {loginWaitingDisplay}
            </div>
        </>
    );
}

export {User};
export default LoginForm;