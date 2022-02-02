import React from "react";
import { Route, Switch } from "react-router-dom";

import { AppNav } from "./AppNav";
import { Home } from "./components/Home";
import { Sample2 } from "./components/Sample2";
import AppContext from "./Shared/AppContext";
import { AppContextShape } from "./types";

declare var Hippo: AppContextShape;

const App = () => {
  return (
      <AppContext.Provider value={Hippo}>
      <AppNav></AppNav>
      <Switch>
        <Route exact path="/" component={Home} />
        <Route path="/page2" component={Sample2} />
      </Switch>
      </AppContext.Provider>
  );
};

export default App;
