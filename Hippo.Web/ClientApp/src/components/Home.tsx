import React, { Component } from "react";
import { Link } from "react-router-dom";

export class Home extends Component {
  static displayName = Home.name;

  render() {
    return (
      <div className="row justify-content-center">
        <div className="col-md-6">
          <p>
            Welcome, <span className="status-color">Calvin</span>
            <br />
            If you’d like access to HiPPO please answer the questions below
          </p>

          <hr />
          <div className="form-group">
            <label>Who is sponsoring your account?</label>
            <select className="form-select" aria-label="Default select example">
              <option selected>Open this select menu</option>
              <option value="1">One</option>
              <option value="2">Two</option>
              <option value="3">Three</option>
            </select>
          </div>
        </div>
      </div>
    );
  }
}
