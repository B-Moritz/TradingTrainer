import { Button } from '@mui/material';
import React from 'react';
import './index.css';



function TradingTrainerFooter() : JSX.Element {
    return (
        <footer className="main-footer">
            <div className="container">
                <div className="row">

                    <div className="col">
                        <h4>References</h4>
                        <ul className="list-unstyled">
                            <li>
                                <a target="_blank" href="https://icons8.com/icon/Q7ahXtQQtuqz/track-and-field">Track and Field</a> icon by <a target="_blank" href="https://icons8.com">Icons8</a>
                            </li>                                                              
                        </ul>
                    </div>

                    <div className="col">
                        <h4>Product</h4>
                        <ul className="list-unstyled">
                            <li>
                                <a href="https://icons8.com/icon/DnhznTvrGRgU/trading">AAAAAAA </a>

                            </li>

                        </ul>
                    </div>

                    <div className="col">
                        <h4>Service</h4>
                        <ul className="list-unstyled">
                            <li>
                                <a href="https://icons8.com/icon/DnhznTvrGRgU/trading">BBBBBBB </a>
                            </li>
                        </ul>
                    </div>

                    <div className="col">
                        <h4>Support</h4>
                        <ul className="list-unstyled">
                            <li>99999999</li>
                        </ul>
                    </div>

                </div>

                <hr />
                <div className="row">
                    <p className="col-lg text-center" >
                        NotSecure through SSL.Copyright &copy; TradingTrainer {new Date().getFullYear()}
                    </p>
                </div>

            </div>


        </footer>
        );
}

export default TradingTrainerFooter;