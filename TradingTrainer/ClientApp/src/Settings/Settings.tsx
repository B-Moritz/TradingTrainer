import { User } from '../LoginForm';
import { useState } from 'react';
import TextField from '@mui/material/TextField';
import Box from '@mui/material/Box';
import MainSettings from './MainSettings';

type SettingsProps = {
    User : User
    SetUser : React.Dispatch<React.SetStateAction<User>>
}

type ProfileInput = {
    Value : string,
    IsValid : boolean
    ErrorMsg : string
    Label : string
    RegexPattern : RegExp
}

type Profile = {
    Id : number
    FirstName : ProfileInput
    LastName : ProfileInput
    Email : ProfileInput
  // The buying power of the user
    FundsAvailable : string
  // The total amount of funds that have been invested by the user since the last reset.
    FundsSpent : string
    Currency : ProfileInput
}

class SettingPages {
    static Main = 1;
    static PwdChange = 2;
    static ConfirmReset = 3;
}

function Settings(props : SettingsProps) : JSX.Element {

    const initialFormInputValues = {
        Id : 0,
        FirstName : {
                Value : "", 
                IsValid : true, 
                ErrorMsg : "The provided first name was not recognized as a valid first name!", 
                Label : "First Name",
                RegexPattern : /^([\w\s]{2,})$/g
            },
        LastName : {
                Value : "", 
                IsValid : true, 
                ErrorMsg : "The provided last name was not recognized as a valid last name!", 
                Label : "Last Name",
                RegexPattern : /^([\w\s]{2,})$/g
            },
        Email : {
            Value : "", 
            IsValid : true, 
            ErrorMsg : "The provided email/username was not recognized as a valid email address!", 
            Label : "Email/Username",
            RegexPattern : /^[a-zA-Z\#\!\%\$\‘\&\+\*\–\/\=\?\^\_\`\.\{\|\}\~]+@[a-zA-Z0-9\-\.]{1,63}$/g
        },
      // The buying power of the user
        FundsAvailable : "",
      // The total amount of funds that have been invested by the user since the last reset.
        FundsSpent : "",
        Currency : {
            Value : "", 
            IsValid : true, 
            ErrorMsg : "The provided currency was not recognized as a valid currency!",
            Label : "Currency",
            RegexPattern : /^[A-Z]{3}$/g
        }
    }

    const [formInputs, setFormInputs] = useState<Profile>(initialFormInputValues);
    const [curSettingsPage, setCurSettingsPage] = useState<number>(SettingPages.Main);

    let currentDisplay = <></>; 
    switch (curSettingsPage) {
        case SettingPages.PwdChange:
            break;
        case SettingPages.PwdChange:
            break;
        default:
            currentDisplay = <MainSettings Profile={formInputs} setProfile={setFormInputs} />
    }

    return(
        <div id="SettingsContainer" className="dashboardContainer waitingParent">
            {currentDisplay}
        </div>

    );

}


export { Profile, ProfileInput }
export default Settings;