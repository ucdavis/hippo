import React, { Component } from "react";
import { Route, Switch } from "react-router-dom";
import { Home } from "./components/Home";
import { Sample2 } from "./components/Sample2";

export default class App extends Component {
  static displayName = App.name;

  render() {
    return (
      <Switch>
        <Route exact path="/" component={Home} />
        <Route path="/page2" component={Sample2} />
      </Switch>
    );
  }
}
