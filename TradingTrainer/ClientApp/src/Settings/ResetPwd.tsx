import TextField from '@mui/material/TextField';
import { resetUserProfile } from '../Service/TradingApi';
import { User } from '../LoginForm';
import { SettingPages } from './Settings';
import { useState } from 'react';
import WaitingDisplay from '../WaitingDisplay';
import { useNavigate } from 'react-router-dom';

type ResetPwdProps = {
    User : User,
    SetUser : React.Dispatch<React.SetStateAction<User>>,
    CurSettingsPage : number, 
    SetCurSettingsPage : React.Dispatch<React.SetStateAction<number>>
}


type PwdInput = {

}

function ResetPwd(props : ResetPwdProps) : JSX.Element {
    const navigate = useNavigate();
    const [isWaiting, setIsWaiting] = useState<JSX.Element>();
    const [curInput, setCurInput] = useState<PwdInput>();
    let outElements : JSX.Element[] = [];

    return(
        <>
            <h2>Reset Password</h2>
            <p>Please enter your new password:</p>
            <nav className="btn-group">
                <button className="btn btn-warning">Confirm</button>
                <button className="btn btn-secondary" onClick={() => {props.SetCurSettingsPage(SettingPages.Main)}}>Cancel</button>
            </nav>
        </>
    );
}

export default ResetPwd;