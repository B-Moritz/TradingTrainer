import TextField from '@mui/material/TextField';
import { resetUserPwd } from '../Service/TradingApi';
import { User } from '../LoginForm';
import { SettingPages } from './Settings';
import { useState } from 'react';
import WaitingDisplay from '../WaitingDisplay';
import { useNavigate } from 'react-router-dom';

type ResetPwdProps = {
    User : User,
    SetUser : React.Dispatch<React.SetStateAction<User>>,
    CurSettingsPage : number, 
    SetCurSettingsPage : React.Dispatch<React.SetStateAction<number>>,
    SetErrorMsg : React.Dispatch<React.SetStateAction<string>>
}

type PwdAttr = {
    PwdVal : string,
    IsValid  : boolean,
    ErrorMsg : string
}

type PwdInput = {
    FirstPwd : PwdAttr,
    LastPwd : PwdAttr
}

function ResetPwd(props : ResetPwdProps) : JSX.Element {

    const initialPwds : PwdInput = {
        FirstPwd : {
            PwdVal : "",
            IsValid : true,
            ErrorMsg : ""
        },
        LastPwd : {
            PwdVal : "",
            IsValid : true,
            ErrorMsg : ""
        }
    }

    const navigate = useNavigate();
    const [isWaiting, setIsWaiting] = useState<JSX.Element>();
    const [curInput, setCurInput] = useState<PwdInput>(initialPwds);
    
    const validatePwd = (pwd : string) : PwdAttr =>  {

        const returnObj : PwdAttr = {
            PwdVal : pwd, 
            IsValid : true, 
            ErrorMsg : ""
        }

        if (pwd.length < 9) {
            // The password length is invalid
            returnObj.IsValid = false;
            returnObj.ErrorMsg = "The password length is less than 9 characters!";
            return returnObj;
        }
        if (!(/\d/.test(pwd))) {
            // The passsword does not contain any numbers
            returnObj.IsValid = false;
            returnObj.ErrorMsg = "The password does not contain any numbers (0-9)!";
            return returnObj;
        }
        if (!(/\w/.test(pwd))) {
            // The password does not contain any word characters
            returnObj.IsValid = false;
            returnObj.ErrorMsg = "The password does not contain any word characters!";
            return returnObj;
        }
        if (!(/[$&+,:;=?@#|'<>-^*()%!]/.test(pwd))) {
            // The password does not contain any special characters
            returnObj.IsValid = false;
            returnObj.ErrorMsg = "The password does not contain any special characters!";
            return returnObj;
        }
        return returnObj;
    }

    const onPwdInputChangeFirst = (e : React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement>) => {
        // Validate Password
        const newInputObj = validatePwd(e.currentTarget.value);
        const newPwdObj : PwdInput = {
            FirstPwd : newInputObj,
            LastPwd : curInput.LastPwd
        }
        setCurInput(newPwdObj);
    }
    
    const onPwdInputChangeLast = (e : React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement>) => {
        // Validate Password
        const newInputObj = validatePwd(e.currentTarget.value);
        const newPwdObj : PwdInput = {
            FirstPwd : curInput.FirstPwd,
            LastPwd : newInputObj
        }
        // Verify that the two inputs values are equal
        if (newPwdObj.FirstPwd.PwdVal !== newPwdObj.LastPwd.PwdVal) {
            newPwdObj.LastPwd.ErrorMsg = "The provided passwords are not equal!";
            newPwdObj.LastPwd.IsValid = false;
        }
        setCurInput(newPwdObj);
    }
    
    const executeReset = async () => {
        setIsWaiting(<WaitingDisplay WaitingText={"Resetting profile...."}></WaitingDisplay>);
        await resetUserPwd(props.User.id, curInput.LastPwd.PwdVal).then((data) => {
            setIsWaiting(<></>);
            props.SetCurSettingsPage(SettingPages.Main);
        }).catch((error : Error) => {
            setIsWaiting(<></>);
            if (error.message.slice(3) === "401") {
                navigate("/login");
            }
            props.SetErrorMsg(error.message);
        });
    }
    

    return(
        <>
            <h2>Reset Password</h2>
            <p>Please enter your new password:</p>
            {
            (curInput.FirstPwd.IsValid ? 
                    <TextField 
                        className="settingsFormInput"
                        id="FirstPwdInput-Settings"
                        label="Password" 
                        type="password"
                        variant="outlined" 
                        value={curInput.FirstPwd.PwdVal}
                        onChange={onPwdInputChangeFirst}
                        required
                    />
                : 
                    <TextField 
                        className="settingsFormInput"
                        id="FirstPwdInput-Settings"
                        label="Password" 
                        type="password"
                        variant="outlined" 
                        value={curInput.FirstPwd.PwdVal}
                        onChange={onPwdInputChangeFirst}
                        helperText={curInput.FirstPwd.ErrorMsg}
                        required
                        error
                    />)
            }
            {
            (curInput.LastPwd.IsValid ? 
                    <TextField 
                        className="settingsFormInput"
                        id="LastPwdInput-Settings"
                        label="Password" 
                        type="password"
                        variant="outlined" 
                        value={curInput.LastPwd.PwdVal}
                        onChange={onPwdInputChangeLast}
                        required
                    />
                : 
                    <TextField 
                        className="settingsFormInput"
                        id="LastPwdInput-Settings"
                        label="Password" 
                        type="password"
                        variant="outlined" 
                        value={curInput.LastPwd.PwdVal}
                        onChange={onPwdInputChangeLast}
                        helperText={curInput.LastPwd.ErrorMsg}
                        required
                        error
                    />)
            }
            <nav className="btn-group">
                <button className="btn btn-warning" onClick={() => {executeReset()}}>Confirm</button>
                <button className="btn btn-secondary" onClick={() => {props.SetCurSettingsPage(SettingPages.Main)}}>Cancle</button>
            </nav>
        </>
    );
}

export default ResetPwd;