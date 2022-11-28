import React from 'react';


function Footer(): JSX.Element {
    return (
        <footer>
            <div className="main-footer">
                <div className="container">
                    <div className="row">

                        {/* Column1 */}
                        <div className="col">
                            <h4>About Us</h4>
                            <ul className="list-unstyled">
                                <li>Market</li>
                                <li>
                                    <img src="https://static.thenounproject.com/png/3861549-200.png" style={{ width: "20px", margin: "0px" }} /> <span style={{ width: "20px", margin: "0px" }}>Trading</span>
                                </li>
                            </ul>
                        </div>

                        {/* Column2 */}
                        <div className="col">
                            <h4>Product</h4>
                            <ul className="list-unstyled">
                                <li>Market</li>
                                <a target="_blank" href="https://icons8.com/icon/DnhznTvrGRgU/trading">Trading</a> icon by <a target="_blank" href="https://icons8.com">Icons8</a>
                            </ul>
                        </div>

                        {/* Column3 */}
                        <div className="col">
                            <h4>Service</h4>
                            <ul className="list-unstyled">
                                <li>Market</li>
                            </ul>
                        </div>

                        {/* Column4 */}
                        <div className="col">
                            <h4>Support</h4>
                            <ul className="list-unstyled">
                                <li>Market</li>
                            </ul>
                            <a href="">

                                <img src="https://static.thenounproject.com/png/3861549-200.png" style={{ width: "20px", margin: "0px" }} />TRA
                            </a>
                        </div>

                    </div>
                    <hr />
                    <div className="row">
                        <p className="col-sm text-center">
                            Copyright &copy TradingTrainer; {new Date().getFullYear()}
                        </p>

                    </div>
                    <img src="https://static.thenounproject.com/png/3861549-200.png" style={{ width: "40px", margin: "0px" }} /> <span style={{ width: "40px", margin: "0px" }}>Trading</span>

                </div>
                    rgb(192,192,192)
                4682B4

            </div>







            <ul>
                <li>
                    <a target="_blank" href="https://icons8.com/icon/DnhznTvrGRgU/trading">Trading</a> icon by <a target="_blank" href="https://icons8.com">Icons8</a>

                </li>
            </ul>   

        </footer>
    );
}

export default Footer;