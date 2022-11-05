import React, { Component, ReactNode } from 'react';
import {Route, NavLink, HashRouter} from 'react-router-dom';
import LoginForm from './LoginForm';
import LandingComponent from './LandingComponent';


class Main extends Component {
    render() : ReactNode {
        return(
            <HashRouter>
                <div className="SiteContainer">
                    <Route path="/index" component={LandingComponent} />
                    <Route path="login" component={LoginForm} />
                </div>
            </HashRouter>
        );
    }
}

export default Main;