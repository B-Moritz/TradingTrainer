import { User } from '../LoginForm';
import { useState, useEffect } from 'react';
import TextField from '@mui/material/TextField';
import Box from '@mui/material/Box';
import MainSettings from './MainSettings';
import ConfirmReset from './ConfirmReset';
import ResetPwd from './ResetPwd';

type SettingsProps = {
    User : User,
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
            currentDisplay = <ResetPwd User={props.User} SetUser={props.SetUser} CurSettingsPage={curSettingsPage} SetCurSettingsPage={setCurSettingsPage} />
            break;
        case SettingPages.ConfirmReset:
            currentDisplay = <ConfirmReset User={props.User} SetUser={props.SetUser} CurSettingsPage={curSettingsPage} SetCurSettingsPage={setCurSettingsPage} />
            break;
        default:
            currentDisplay = <MainSettings User={props.User} SetUser={props.SetUser} CurSettingsPage={curSettingsPage} SetCurSettingsPage={setCurSettingsPage}/>
    }

    return(
        <div id="SettingsContainer" className="dashboardContainer waitingParent">
            {currentDisplay}
        </div>

    );

}
export {SettingPages}
export default Settings;