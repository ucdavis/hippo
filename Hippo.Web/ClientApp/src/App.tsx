import React, { useEffect, useMemo, useState } from "react";
import { Route, Switch, useLocation } from "react-router-dom";

import { AppNav } from "./AppNav";
import AppContext from "./Shared/AppContext";
import { Account, AppContextShape } from "./types";
import TopSvg from "./Shared/topSvg";

import { Home } from "./components/Home";
import { AccountInfo } from "./components/Account/AccountInfo";
import { RequestForm } from "./components/Account/RequestForm";
import { PendingApproval } from "./components/Account/PendingApproval";
import { ApproveAccounts } from "./components/Account/ApproveAccounts";
import { SponsoredAccounts } from "./components/Account/SponsoredAccounts";
import { authenticatedFetch } from "./util/api";
import { AdminUsers } from "./components/Admin/AdminUsers";
import { Sponsors } from "./components/Admin/Sponsors";
import { AdminApproveAccounts } from "./components/Admin/AdminApproveAccounts";
import { ConditionalRoute } from "./ConditionalRoute";
import { ModalProvider } from "react-modal-hook";
import { Toaster } from "react-hot-toast";

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
        <ModalProvider>
          <Toaster />
          <div className={`account-status-${accountClassName}`}>
            <AppNav></AppNav>
            <div className="top-svg">
              <TopSvg />
            </div>
            <Switch>
              <Route exact path="/" component={Home} />
              <Route path="/active" component={AccountInfo} />
              <Route path="/pendingapproval" component={PendingApproval} />
              <Route path="/create" component={RequestForm} />
              <ConditionalRoute
                roles={["Sponsor"]}
                path="/approve"
                component={ApproveAccounts}
              />
              <ConditionalRoute
                roles={["Sponsor"]}
                path="/sponsored"
                component={SponsoredAccounts}
              />
              <ConditionalRoute
                roles={["Admin"]}
                path="/admin/users"
                component={AdminUsers}
              />
              <ConditionalRoute
                roles={["Admin"]}
                path="/admin/sponsors"
                component={Sponsors}
              />
              <ConditionalRoute
                roles={["Admin"]}
                path="/admin/accountApprovals"
                component={AdminApproveAccounts}
              />
            </Switch>
          </div>
        </ModalProvider>
      </AppContext.Provider>
    );
  } else {
    //center div
    return (
      <div className="row justify-content-center">
        <div className="col-md-8">Loading...</div>
      </div>
    );
  }
};

export default App;
