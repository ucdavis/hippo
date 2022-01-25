import React from "react";
import { Route, Switch } from "react-router-dom";

import { AppNav } from "./AppNav";
import { Home } from "./components/Home";
import { Sample2 } from "./components/Sample2";

const App = () => {
  return (
    <>
      <AppNav></AppNav>
      <Switch>
        <Route exact path="/" component={Home} />
        <Route path="/page2" component={Sample2} />
      </Switch>
    </>
  );
};

export default App;
