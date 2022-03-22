import { useMemo, useState } from "react";
import { Route, Switch, useLocation } from "react-router-dom";

import { AppNav } from "./AppNav";
import AppContext from "./Shared/AppContext";
import { AppContextShape } from "./types";
import BottomSvg from "./Shared/bottomSvg";

import { Home } from "./components/Home";
import { AccountInfo } from "./components/Account/AccountInfo";
import { RequestForm } from "./components/Account/RequestForm";
import { PendingApproval } from "./components/Account/PendingApproval";
import { ApproveAccounts } from "./components/Account/ApproveAccounts";
import { SponsoredAccounts } from "./components/Account/SponsoredAccounts";
import { AdminUsers } from "./components/Admin/AdminUsers";
import { Sponsors } from "./components/Admin/Sponsors";
import { AdminApproveAccounts } from "./components/Admin/AdminApproveAccounts";
import { ConditionalRoute } from "./ConditionalRoute";
import { ModalProvider } from "react-modal-hook";
import { Toaster } from "react-hot-toast";
import { Multiple } from "./components/Account/Multiple";

declare var Hippo: AppContextShape;

const App = () => {
  const [context, setContext] = useState<AppContextShape>(Hippo);

  const loc = useLocation();

  const accountClassName = useMemo(
    () => loc.pathname.replace("/", ""),
    [loc.pathname]
  );

  if (context.accounts.length > 0) {
    return (
      <AppContext.Provider value={[context, setContext]}>
        <ModalProvider>
          <Toaster />
          <div className={`account-status-${accountClassName}`}>
            <AppNav></AppNav>
            <div className="top-svg">
              <BottomSvg />
            </div>
            <Switch>
              <Route exact path="/" component={Home} />
              <Route path="/:cluster/active" component={AccountInfo} />
              <Route path="/:cluster/pendingapproval" component={PendingApproval} />
              <Route path="/create" component={RequestForm} />
              <Route path="/multiple" component={Multiple} />
              <ConditionalRoute
                roles={["Sponsor"]}
                path="/:cluster/approve"
                component={ApproveAccounts}
              />
              <ConditionalRoute
                roles={["Sponsor"]}
                path="/:cluster/sponsored"
                component={SponsoredAccounts}
              />
              <ConditionalRoute
                roles={["Admin"]}
                path="/:cluster/admin/users"
                component={AdminUsers}
              />
              <ConditionalRoute
                roles={["Admin"]}
                path="/:cluster/admin/sponsors"
                component={Sponsors}
              />
              <ConditionalRoute
                roles={["Admin"]}
                path="/:cluster/admin/accountApprovals"
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
