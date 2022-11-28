import { User } from '../LoginForm';
import { useState, useEffect } from 'react';
import TextField from '@mui/material/TextField';
import Box from '@mui/material/Box';
import MainSettings from './MainSettings';

type SettingsProps = {
    User : User
    SetUser : React.Dispatch<React.SetStateAction<User>>
}

class SettingPages {
    static Main = 1;
    static PwdChange = 2;
    static ConfirmReset = 3;
}

function Settings(props : SettingsProps) : JSX.Element {
    const [curSettingsPage, setCurSettingsPage] = useState<number>(SettingPages.Main);
    
    let currentDisplay = <></>; 
    switch (curSettingsPage) {
        case SettingPages.PwdChange:
            break;
        case SettingPages.PwdChange:
            break;
        default:
            currentDisplay = <MainSettings User={props.User} SetUser={props.SetUser}/>
    }

    return(
        <div id="SettingsContainer" className="dashboardContainer waitingParent">
            {currentDisplay}
        </div>

    );

}

export default Settings;