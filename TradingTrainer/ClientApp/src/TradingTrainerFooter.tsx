import React from 'react';
import './index.css';



function TradingTrainerFooter() : JSX.Element {
    return (
        <footer className="main-footer">
            <div className="container">
                <div className="row">

                    <div className="col">
                        <h4>Trading</h4>
                        <ul className="list-unstyled">
                            <li>
                                <a href="https://icons8.com/icon/DnhznTvrGRgU/trading">
                                    <img src="https://static.thenounproject.com/png/3861549-200.png" style={{ width: "100px", margin: "0px" }} alt="logo" />
                                </a>
                              
                            </li>                                                              
                        </ul>
                    </div>

                    <div className="col">
                        <h4>Want future articles?</h4>
                        <ul className="list-unstyled">
                            <li>
                                <a href="https://icons8.com/icon/DnhznTvrGRgU/trading">AAAAAAA </a>
                            </li>

                        </ul>
                    </div>

                    <div className="col">
                        <h4>Site Hosted By</h4>
                        <ul className="list-unstyled">
                            <li>
                                <a href="https://icons8.com/icon/DnhznTvrGRgU/trading">BBBBBBB </a>
                            </li>
                        </ul>
                    </div>

                    {/* Column2 */}
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