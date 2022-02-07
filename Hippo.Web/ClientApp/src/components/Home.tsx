import React, { Component } from "react";
import { Link } from "react-router-dom";

export class Home extends Component {
  static displayName = Home.name;

  render() {
    return (
      <div className="row justify-content-center">
        <div className="col-md-6">
          <h3>
            Welcome, <span className="status-color">Calvin</span>
          </h3>
          <p>If you’d like access to HiPPO please answer the questions below</p>

          <hr />
          <div className="form-group">
            <label>Who is sponsoring your account?</label>
            <select className="form-select" aria-label="Default select example">
              <option selected>Open this select menu</option>
              <option value="1">One</option>
              <option value="2">Two</option>
              <option value="3">Three</option>
            </select>
            <p className="form-helper">Help text</p>
          </div>
          <div className="form-group">
            <label className="form-label">What is your SSH key</label>
            <textarea
              className="form-control"
              id="exampleFormControlTextarea1"
            ></textarea>
            <p className="form-helper">Help text</p>
          </div>
          <a href="#" className="btn btn-primary">
            Submit
          </a>
        </div>
      </div>
    );
  }
}
