import React, { Component } from "react";
import { Link } from "react-router-dom";

export class Sample2 extends Component {

  render() {
    return (
      <div>
        <h1>Sample 2!</h1>
        <p>This is another sample page just for testing</p>
        <Link to="/">Go back home</Link>
      </div>
    );
  }
}
