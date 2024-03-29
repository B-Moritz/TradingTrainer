import TradingDashboard from './TradingComponents/TradingDashboard';
import { useNavigate, Outlet } from 'react-router-dom';
import { useState, useEffect } from 'react';
import TradingTrainerFooter from './TradingTrainerFooter';
import WaitingDisplay from './WaitingDisplay';
import { User } from './LoginForm';
import logo from './img/icons8-track-and-field-48.png';
import { logoutApiCall, getUsername } from './Service/TradingApi';


type AppContainerProps = {
    User : User
    SetUser : React.Dispatch<React.SetStateAction<User>>
    SetErrorMsg : React.Dispatch<React.SetStateAction<string>>
}

function AppContainer(props : AppContainerProps) : JSX.Element {
    const navigate = useNavigate();
    const [reconnectingWaiting, setReconnectingWaiting] = useState(<></>);

    useEffect(() => {
        // If the site is reloaded and IsAuthenticated is set to false -> try reconnect without navigating back to login
        console.log("Current user id is: " + props.User.id);
        if (props.User.id === 0) {            
            tryReconnect();
        }
    }, [props.User]);

    /*
    This function is used to check if the user already has an active session
    */
    const tryReconnect = async () => {
        setReconnectingWaiting(<WaitingDisplay WaitingText={"Reconnecting, please wait ...."}></WaitingDisplay>);
        await checkExistingSession();
        setReconnectingWaiting(<></>);
    }

    const checkExistingSession = async () => {
        // Check if the user has an active session
        // If the user has an active session -> redirect to application
        await getUsername().then((data) => {
            if (data) {
                // The user has already an active session on the server
                // Bypas login procedure
                props.SetUser(data);
                //props.SetIsAuthenticated(true);
                //navigate("/TradingDashboard");
            }
        }).catch((error : Error) => {
            setReconnectingWaiting(<></>);
            if (error.message.slice(3) === "401") {
                navigate("/login");
            }
            props.SetErrorMsg(error.message);
        });
    }
    
    const navigateToSettings = () => {
        navigate("/tradingDashboard/settings");
    }

    const navigateToTradingDashboard = () => {
        navigate("/tradingDashboard");
    }

    const logout = async () => {
        const cleardUser : User = {
            id : 0,
            firstName : "",
            lastName : "",
            email : "",
            alphaVantageApiKey : "",
            fundsAvailable : "",
            fundsSpent : "",
            currency : ""
        }
        setReconnectingWaiting(<WaitingDisplay WaitingText={"Signing out...."}></WaitingDisplay>);
        await logoutApiCall().then(() => {
            setReconnectingWaiting(<></>);
            props.SetUser(cleardUser);
            navigate("/");
        }).catch((error : Error) => {
            setReconnectingWaiting(<></>);
            if (error.message.slice(3) === "401") {
                props.SetUser(cleardUser);
                navigate("/");
            }
            props.SetErrorMsg(error.message);
        });
    }

    return(
        <>
            <header id="DashboardHeader">
                <nav id="DashboardNav">
                    <img id="TradingTrainerNavIcon" src={logo} alt="Trading Trainer Logo" />
                    <h1 id="TradingTrainerNavTitle">Trading Trainer</h1>
                    <div id="UserSettings" className="navLink" onClick={() => navigateToTradingDashboard()}>Dashboard</div>
                    <div id="UserSettings" className="navLink" onClick={() => navigateToSettings()}>Settings</div>
                    <div id="LogoutNav" className="navLink" onClick={() => logout()}>Logout</div>
                </nav>
            </header>
            <main id="TradingDashboardContainer">
                <Outlet />
            </main>
            <TradingTrainerFooter></TradingTrainerFooter>
            {reconnectingWaiting}
        </>

    );
}

export default AppContainer;