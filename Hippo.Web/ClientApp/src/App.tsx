import React, { useMemo } from "react";
import { Route, Switch, useLocation } from "react-router-dom";

import { AppNav } from "./AppNav";
import AppContext from "./Shared/AppContext";
import { AppContextShape } from "./types";

import { Home } from "./components/Home";
import { AccountInfo } from "./components/AccountInfo";
import { RequestForm } from "./components/RequstForm";
import { PendingApproval } from "./components/PendingApproval";
import BottomSvg from "./Shared/bottomSvg";

declare var Hippo: AppContextShape;

const App = () => {
  const loc = useLocation();

  const accountClassName = useMemo(
    () => loc.pathname.replace("/", ""),
    [loc.pathname]
  );

  return (
    <AppContext.Provider value={Hippo}>
      <div className={`account-status-${accountClassName}`}>
        <AppNav></AppNav>
        <div className="bottom-svg">
          <BottomSvg />
        </div>
        <Switch>
          <Route exact path="/" component={Home} />
          <Route path="/active" component={AccountInfo} />
          <Route path="/pendingapproval" component={PendingApproval} />
          <Route path="/create" component={RequestForm} />
        </Switch>
      </div>
    </AppContext.Provider>
  );
};

export default App;
