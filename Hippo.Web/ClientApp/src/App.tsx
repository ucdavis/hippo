import React, { useEffect, useMemo, useState } from "react";
import { Route, Switch, useLocation } from "react-router-dom";

import { AppNav } from "./AppNav";
import AppContext from "./Shared/AppContext";
import { Account, AppContextShape } from "./types";
import BottomSvg from "./Shared/bottomSvg";

import { Home } from "./components/Home";
import { AccountInfo } from "./components/AccountInfo";
import { RequestForm } from "./components/RequstForm";
import { PendingApproval } from "./components/PendingApproval";
import { ApproveAccounts } from "./components/ApproveAccounts";
import { authenticatedFetch } from "./util/api";
import { AdminUsers } from "./Admin/AdminUsers";
import { Sponsors } from "./Admin/Sponsors";

declare var Hippo: AppContextShape;

const App = () => {
  const [context, setContext] = useState<AppContextShape>(Hippo);

  const loc = useLocation();

  const accountClassName = useMemo(
    () => loc.pathname.replace("/", ""),
    [loc.pathname]
  );

  useEffect(() => {
    // query for user account status
    const fetchAccount = async () => {
      const response = await authenticatedFetch("/api/account/get");

      if (response.ok) {
        if (response.status === 204) {
          // no content means we have no account record for this person
          setContext((ctx) => ({
            ...ctx,
            account: { id: 0, status: "create" } as Account,
          }));
        } else {
          const account = (await response.json()) as Account;
          setContext((ctx) => ({
            ...ctx,
            account,
          }));
        }
      }

      // TODO: handle error case
    };

    fetchAccount();
  }, []);

  if (context.account) {
    return (
      <AppContext.Provider value={[context, setContext]}>
        <div className={`account-status-${accountClassName}`}>
          <AppNav></AppNav>
          <div className="top-svg">
            <BottomSvg />
          </div>
          <Switch>
            <Route exact path="/" component={Home} />
            <Route path="/active" component={AccountInfo} />
            <Route path="/pendingapproval" component={PendingApproval} />
            <Route path="/create" component={RequestForm} />
            <Route path="/approve" component={ApproveAccounts} />
            <Route path="/admin/index" component={AdminUsers} />
            <Route path="/admin/sponsors" component={Sponsors} />
          </Switch>
        </div>
      </AppContext.Provider>
    );
  } else {
    return <div>Loading...</div>;
  }
};

export default App;
