import React, { ReactNode } from 'react';
import NavLink from 'react-router-dom';

class LoginForm extends Component {
    Render() : ReactNode {
        return(
            <form>
                <label>Username:</label>
                <input type="text" />
                <label></label>
                <input type="text" />
            </form>
        );
    }

}

export default LoginForm;