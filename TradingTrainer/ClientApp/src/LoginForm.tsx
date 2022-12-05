import React, { useState, useEffect } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import WaitingDisplay from './WaitingDisplay';
import { getUsername, authenticate } from './Service/TradingApi';


type User = {
    id : number
    firstName? : string
    lastName? : string
    email? : string
    alphaVantageApiKey? : string
    // The buying power of the user
    fundsAvailable? : string
    // The total amount of funds that have been invested by the user since the last reset.
    fundsSpent? : string
    currency? : string
} 

type LoginProps = {
    // Function used to set the current loged in user
    SetUser : React.Dispatch<React.SetStateAction<User>>
    User : User
    SetErrorMsg : React.Dispatch<React.SetStateAction<string>>
}

function LoginForm(props : LoginProps) : JSX.Element {

    const [usernameInput, setUsernameInput] = useState(true);
    const [pwdInput, setPwdInput] = useState(true);
    // State containing the value in the username input element
    const [usr, setUsr] = useState("");
    // State containing the value in the password input element
    const [pwd, setPwd] = useState("");
    

    const [firstRender, setFirstRender] = useState(true);
    // time used to test the loading displays
    const waitDelay = 1000;
    // object used for programatic navigation with react routing
    const navigate = useNavigate();
    
    const defaultWaitMsg = "Checking for active sessions."
    //const [loginWaitingDisplay, setLoginWaitingDisplay] = useState(<WaitingDisplay WaitingText={"Checking for active sessions."}></WaitingDisplay>);
    const [isWaiting, setIsWaiting] = useState({
        active : true,
        // The message displayed in the waiting display
        msg : defaultWaitMsg
    });
    const [authFailed, setAuthFailed] = useState(false);

    useEffect(() => {
        // This function runs after the first rendering of the component
        // https://reactjs.org/docs/hooks-effect.html#tip-optimizing-performance-by-skipping-effects
        checkExistingSession();
    }, [])

    useEffect(() => {
        // Navigate to the dashboard once it is sure that the user object is available
        if (props.User.id !== 0) {
            navigate("/TradingDashboard");
        }
    }, [props.User])



    const initiateLogin = async () : Promise<void> => {
        //setLoginWaitingDisplay(<WaitingDisplay WaitingText={"Atempting to authenticate. Please wait ......"}></WaitingDisplay>);
        setIsWaiting({
            active : true, 
            msg : "Atempting to authenticate. Please wait ......"
        });
        await authenticateUser(usr, pwd);
        // Display the spinner
        resetLogin();
    }

    // https://developer.mozilla.org/en-US/docs/Web/API/Fetch_API/Using_Fetch
    // This method executes the call to /trading/login through the fetch api.
    const authenticateUser = async (usr: string, pwd: string) : Promise<void> => {
        const endpoint = "/trading/login";
        // Defining the payload to be sent to the server
        const credentials = {
            username: usr,
            password: pwd
        };
        // Make the authentication http patch request
        await authenticate(credentials).then((data) => {
            // Authentication was successfull 
            if (authFailed) {
                //  If the authentication failed state is active, make sure to disable it.
                setAuthFailed(false);
            }
            setTimeout(() => {
                setIsWaiting({
                    active : false,
                    msg : defaultWaitMsg
                });
                props.SetUser(data);
            }, waitDelay)
        }).catch((errorResp) => {
            // An error occured during authentication
            setTimeout(() => {
                if (/401/g.test(errorResp.message)) {
                    // The server responded with unauthorized -> indicates tha the password or username is wrong
                    setAuthFailed(true);
                }
                // Disable waiting display
                setIsWaiting({
                    active : false, 
                    msg : defaultWaitMsg
                });
            }, waitDelay);
        });
    }


    const checkExistingSession = async () => {
        // Check if the user has an active session
        // If the user has an active session -> redirect to application
        await getUsername().then((data) => {
            setTimeout(() => {
                //setLoginWaitingDisplay(<></>);
                if (data) {
                    // The user has already an active session on the server
                    props.SetUser(data);
                }
            }, waitDelay);
        }).catch((errorResp) => {
            console.log(errorResp.message);
            setTimeout(() => {
                //setLoginWaitingDisplay(<></>);
                setIsWaiting({
                    active : false, 
                    msg : defaultWaitMsg
                });
            }, waitDelay);
        })
    }

    const validateUsername = (e : React.FormEvent<HTMLInputElement>) : void => {
        if (firstRender) {
            setFirstRender(false);
        }
         const unamePattern : RegExp = /^[a-zA-Z\#\!\%\$\‘\&\+\*\–\/\=\?\^_\`\.\{\|\}\~]+@[a-zA-Z0-9\-\.]{1,63}$/;
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
                {isWaiting.active && <WaitingDisplay WaitingText={isWaiting.msg}></WaitingDisplay>}
            </div>
        </>
    );
}

export type {User};
export default LoginForm;