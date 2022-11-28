import { resetUserProfile } from '../Service/TradingApi';
import { User } from '../LoginForm';
import { SettingPages } from './Settings';
import { useState } from 'react';
import WaitingDisplay from '../WaitingDisplay';
import { useNavigate } from 'react-router-dom';

type ConfirmResetProps = {
    User : User,
    SetUser : React.Dispatch<React.SetStateAction<User>>,
    CurSettingsPage : number, 
    SetCurSettingsPage : React.Dispatch<React.SetStateAction<number>>,
    SetErrorMsg : React.Dispatch<React.SetStateAction<string>>
}

function ConfirmReset(props : ConfirmResetProps) : JSX.Element {
    const navigate = useNavigate();
    const [isWaiting, setIsWaiting] = useState<JSX.Element>();

    const executeReset = async () => {
        setIsWaiting(<WaitingDisplay WaitingText={"Resetting profile...."}></WaitingDisplay>);
        await resetUserProfile(props.User.id).then((data : User) => {
            setIsWaiting(<></>);
            props.SetUser({
                id : 0,
                firstName : data.firstName,
                lastName : data.lastName,
                email : data.email,
                currency : data.currency,
            });
            props.SetCurSettingsPage(SettingPages.Main);
        }).catch((error : Error) => {
            if (error.message.slice(3) === "401") {
                navigate("/login");
            }
            props.SetErrorMsg(error.message);
        });
    }

    return(
        <>
            <h2>Reseting your profile</h2>
            <p>Are your sure you want to reset your profile?</p>
            <nav className="btn-group">
                <button className="btn btn-danger" onClick={() => {executeReset()}}>Confirm</button>
                <button className="btn btn-secondary" onClick={() => {props.SetCurSettingsPage(SettingPages.Main)}}>Cancle</button>
            </nav>
            {isWaiting}
        </>
    );
}

export default ConfirmReset;